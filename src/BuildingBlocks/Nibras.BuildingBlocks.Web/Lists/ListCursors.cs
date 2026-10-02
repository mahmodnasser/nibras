using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nibras.BuildingBlocks.Application.Lists;

namespace Nibras.BuildingBlocks.Web.Lists;

/// <summary>
/// The keyset cursor of document 22 §2.1: <c>base64url(JSON {v, k, s, f, d})</c>, a dot, and an HMAC-SHA256 tag
/// truncated to 8 bytes. The tag also covers the endpoint's route, so a cursor from another endpoint, a changed
/// sort, a changed filter or an edited cursor is refused. The tenant is not in it: the tenant filter applies
/// whatever a cursor says.
/// </summary>
public sealed class ListCursors
{
    /// <summary>The configuration key of the per-service secret, base64 of at least 32 bytes, from the secret store.</summary>
    public const string KeySetting = "Nibras:Pagination:CursorKey";

    private const int Version = 1;
    private const int TagLength = 8;

    private readonly byte[] _key;

    public ListCursors(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        var configured = configuration[KeySetting];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            _key = Convert.FromBase64String(configured);
            if (_key.Length < 32)
            {
                throw new InvalidOperationException($"{KeySetting} must be at least 32 bytes.");
            }
        }
        else if (environment.IsProduction())
        {
            throw new InvalidOperationException($"{KeySetting} is not configured; production cursors must survive a restart and a second replica.");
        }
        else
        {
            _key = RandomNumberGenerator.GetBytes(32); // development and tests: cursors live as long as the process
        }
    }

    public string Encode(IReadOnlyList<object?> key, string sort, string filterHash, KeysetDirection direction, string route)
    {
        ArgumentNullException.ThrowIfNull(key);
        var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer))
        {
            json.WriteStartObject();
            json.WriteNumber("v", Version);
            json.WriteStartArray("k");
            foreach (var value in key)
            {
                if (ListValues.Format(value) is { } text)
                {
                    json.WriteStringValue(text);
                }
                else
                {
                    json.WriteNullValue();
                }
            }

            json.WriteEndArray();
            json.WriteString("s", sort);
            json.WriteString("f", filterHash);
            json.WriteString("d", direction == KeysetDirection.Next ? "next" : "prev");
            json.WriteEndObject();
        }

        var payload = buffer.ToArray();
        return Base64Url.EncodeToString(payload) + "." + Base64Url.EncodeToString(Tag(payload, route));
    }

    /// <summary>The key values typed by the sort's fields, and the direction; null when the cursor does not belong to this query.</summary>
    public (IReadOnlyList<object?> Key, KeysetDirection Direction)? Decode<TEntity>(
        string cursor, ListFieldMap<TEntity> fields, IReadOnlyList<SortField> sort, string normalizedSort, string filterHash, string route)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(sort);
        var dot = cursor.IndexOf('.', StringComparison.Ordinal);
        if (dot <= 0)
        {
            return null;
        }

        byte[] payload, tag;
        try
        {
            payload = Base64Url.DecodeFromChars(cursor.AsSpan(0, dot));
            tag = Base64Url.DecodeFromChars(cursor.AsSpan(dot + 1));
        }
        catch (FormatException)
        {
            return null;
        }

        if (!CryptographicOperations.FixedTimeEquals(tag, Tag(payload, route)))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            if (root.GetProperty("v").GetInt32() != Version
                || root.GetProperty("s").GetString() != normalizedSort
                || root.GetProperty("f").GetString() != filterHash)
            {
                return null;
            }

            var direction = root.GetProperty("d").GetString() switch
            {
                "next" => KeysetDirection.Next,
                "prev" => KeysetDirection.Previous,
                _ => (KeysetDirection?)null,
            };
            var values = root.GetProperty("k");
            if (direction is null || values.GetArrayLength() != sort.Count)
            {
                return null;
            }

            var key = new object?[sort.Count];
            for (var i = 0; i < sort.Count; i++)
            {
                var field = fields.Fields[sort[i].Field];
                var element = values[i];
                if (element.ValueKind == JsonValueKind.Null && field.IsNullable)
                {
                    continue;
                }

                if (element.ValueKind != JsonValueKind.String || !ListValues.TryParse(element.GetString()!, field.CoreType, out key[i]))
                {
                    return null;
                }
            }

            return (key, direction.Value);
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return null;
        }
    }

    private byte[] Tag(byte[] payload, string route)
    {
        var routeBytes = Encoding.UTF8.GetBytes(route);
        var signed = new byte[payload.Length + 1 + routeBytes.Length];
        payload.CopyTo(signed, 0);
        signed[payload.Length] = 0;
        routeBytes.CopyTo(signed, payload.Length + 1);
        return HMACSHA256.HashData(_key, signed)[..TagLength];
    }
}
