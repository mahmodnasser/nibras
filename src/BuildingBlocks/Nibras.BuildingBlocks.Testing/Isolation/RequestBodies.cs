using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nibras.BuildingBlocks.Testing.Isolation;

/// <summary>
/// A well-formed body for an endpoint's request type in the wire format of document 22 §2 (camelCase, string enums,
/// ISO 8601), with tenant B's identifier in every property that names one, so the attack reaches the handler.
/// </summary>
internal static class RequestBodies
{
    private const int MaxDepth = 3;

    /// <param name="type">The request type the endpoint binds.</param>
    /// <param name="identifier">Tenant B's identifier for a property such as <c>sectionId</c>, or null when no table matches.</param>
    public static JsonObject Sample(Type type, Func<string, Guid?> identifier) => Sample(type, identifier, 0);

    private static JsonObject Sample(Type type, Func<string, Guid?> identifier, int depth)
    {
        var body = new JsonObject();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                     .OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var name = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            body[name] = Value(property.PropertyType, name, identifier, depth);
        }

        return body;
    }

    private static JsonNode? Value(Type declared, string name, Func<string, Guid?> identifier, int depth)
    {
        var type = Nullable.GetUnderlyingType(declared) ?? declared;
        if (type == typeof(Guid))
        {
            var id = (IsolationSurface.IsIdentifier(name) ? identifier(name) : null) ?? Guid.CreateVersion7();
            return JsonValue.Create(id);
        }

        if (type == typeof(string))
        {
            return "isolation";
        }

        if (type == typeof(bool))
        {
            return false;
        }

        if (type.IsEnum)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(Enum.GetNames(type)[0]);
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)
            || type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return 1;
        }

        if (type == typeof(DateOnly))
        {
            return "2026-09-01";
        }

        if (type == typeof(DateTimeOffset) || type == typeof(DateTime))
        {
            return "2026-09-01T08:00:00.000Z";
        }

        if (type == typeof(TimeOnly))
        {
            return "08:00:00";
        }

        if (type == typeof(TimeSpan))
        {
            return "PT45M";
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return new JsonArray();
        }

        return depth < MaxDepth && type.IsClass ? Sample(type, identifier, depth + 1) : null;
    }
}

/// <summary>How a route segment or a property name finds the tenant-owned table it refers to.</summary>
internal static class ResourceNames
{
    /// <summary>
    /// The table a resource names: the segment in snake case (<c>attendance-sessions</c> is <c>attendance_sessions</c>),
    /// unless the target maps it. Null when the service has no such tenant-owned table.
    /// </summary>
    public static string? TableFor(string? resource, IsolationTarget target, IReadOnlySet<string> tables)
    {
        if (resource is null)
        {
            return null;
        }

        if (target.ResourceTables.TryGetValue(resource, out var mapped))
        {
            return mapped;
        }

        var snake = resource.Replace('-', '_');
        return tables.Contains(snake) ? snake : null;
    }

    /// <summary>The resources an identifier property may refer to: <c>sectionId</c> to <c>sections</c>, <c>classId</c> to <c>classes</c>.</summary>
    public static IEnumerable<string> ResourcesFor(string identifierName)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifierName);
        if (identifierName == "id")
        {
            yield break;
        }

        var stem = Kebab(identifierName[..^2]);
        yield return stem + "s";
        yield return stem + "es";
        if (stem.EndsWith('y'))
        {
            yield return stem[..^1] + "ies";
        }
    }

    private static string Kebab(string camel)
    {
        var text = new StringBuilder(camel.Length + 4);
        foreach (var c in camel)
        {
            if (char.IsUpper(c) && text.Length > 0)
            {
                text.Append('-');
            }

            text.Append(char.ToLower(c, CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }
}
