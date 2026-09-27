using System.Buffers;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using System.Xml;

namespace Nibras.BuildingBlocks.Web;

/// <summary>
/// The wire format of document 22 §1.4: camelCase properties, camelCase string enums, timestamps in UTC with
/// millisecond precision and <c>Z</c>, ISO 8601 durations, nulls omitted, and unknown input properties refused.
/// A service puts its source-generated context first in the resolver chain; reflection is the fallback.
/// </summary>
public static class NibrasJson
{
    public static JsonSerializerOptions Configure(JsonSerializerOptions options, IEnumerable<IJsonTypeInfoResolver>? contexts = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = false;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.NumberHandling = JsonNumberHandling.Strict;
        options.ReadCommentHandling = JsonCommentHandling.Disallow;
        options.AllowTrailingCommas = false;

        // Arabic travels as text; the HTML-sensitive characters < > & ' " are always escaped (REQ-SEC-017).
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.Converters.Add(new UtcTimestampConverter());
        options.Converters.Add(new UtcDateTimeConverter());
        options.Converters.Add(new IsoDurationConverter());

        var position = 0;
        foreach (var context in contexts ?? [])
        {
            options.TypeInfoResolverChain.Insert(position++, context);
        }

        if (options.TypeInfoResolverChain.Count == 0)
        {
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
        }

        return options;
    }

    internal const string TimestampFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

    internal static bool EndsInZ(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return false;
        }

        var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        return span.Length > 0 && span[^1] == (byte)'Z';
    }
}

/// <summary><c>2026-09-19T05:30:00.000Z</c> on the wire; an input without the <c>Z</c> is refused, never guessed.</summary>
internal sealed class UtcTimestampConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!NibrasJson.EndsInZ(ref reader) || !reader.TryGetDateTimeOffset(out var value))
        {
            throw new JsonException("A timestamp is ISO 8601 in UTC with the Z suffix.");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.UtcDateTime.ToString(NibrasJson.TimestampFormat, CultureInfo.InvariantCulture));
    }
}

/// <summary>The same format for a <see cref="DateTime"/>; one of unspecified kind is ambiguous and refused.</summary>
internal sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!NibrasJson.EndsInZ(ref reader) || !reader.TryGetDateTimeOffset(out var value))
        {
            throw new JsonException("A timestamp is ISO 8601 in UTC with the Z suffix.");
        }

        return value.UtcDateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => throw new InvalidOperationException("A DateTime of unspecified kind has no instant; use DateTimeOffset or DateOnly."),
        };
        writer.WriteStringValue(utc.ToString(NibrasJson.TimestampFormat, CultureInfo.InvariantCulture));
    }
}

/// <summary>ISO 8601 durations such as <c>PT45M</c>, never a bare number whose unit is a guess.</summary>
internal sealed class IsoDurationConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return XmlConvert.ToTimeSpan(reader.GetString() ?? throw new JsonException("A duration is an ISO 8601 string."));
        }
        catch (FormatException exception)
        {
            throw new JsonException("A duration is an ISO 8601 string such as PT45M.", exception);
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(XmlConvert.ToString(value));
    }
}
