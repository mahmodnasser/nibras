using System.Globalization;
using System.Text.Json;

namespace Nibras.BuildingBlocks.Web.Lists;

/// <summary>
/// Reads and writes the scalar values of the list grammar with the invariant culture (document 22 §3.1): dates
/// as <c>YYYY-MM-DD</c>, instants as ISO 8601 UTC with <c>Z</c>, numbers with <c>.</c>, enums as their camelCase name.
/// </summary>
internal static class ListValues
{
    private const string InstantFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'";

    public static bool TryParse(string text, Type type, out object? value)
    {
        value = null;
        var invariant = CultureInfo.InvariantCulture;
        switch (type)
        {
            case var t when t == typeof(string):
                value = text;
                return true;
            case var t when t == typeof(Guid):
                if (Guid.TryParseExact(text, "D", out var guid)) { value = guid; return true; }
                return false;
            case var t when t == typeof(bool):
                if (text is "true" or "false") { value = text == "true"; return true; }
                return false;
            case var t when t == typeof(int):
                if (int.TryParse(text, NumberStyles.AllowLeadingSign, invariant, out var i)) { value = i; return true; }
                return false;
            case var t when t == typeof(long):
                if (long.TryParse(text, NumberStyles.AllowLeadingSign, invariant, out var l)) { value = l; return true; }
                return false;
            case var t when t == typeof(decimal):
                if (decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, invariant, out var d)) { value = d; return true; }
                return false;
            case var t when t == typeof(DateOnly):
                if (DateOnly.TryParseExact(text, "yyyy-MM-dd", invariant, DateTimeStyles.None, out var date)) { value = date; return true; }
                return false;
            case var t when t == typeof(TimeOnly):
                if (TimeOnly.TryParseExact(text, ["HH:mm:ss", "HH:mm:ss.fffffff", "HH:mm"], invariant, DateTimeStyles.None, out var time)) { value = time; return true; }
                return false;
            case var t when t == typeof(DateTimeOffset) || t == typeof(DateTime):
                if (!text.EndsWith('Z')
                    || !DateTimeOffset.TryParse(text, invariant, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var instant))
                {
                    return false;
                }

                value = t == typeof(DateTime) ? instant.UtcDateTime : instant;
                return true;
            case { IsEnum: true }:
                foreach (var name in Enum.GetNames(type))
                {
                    if (string.Equals(JsonNamingPolicy.CamelCase.ConvertName(name), text, StringComparison.Ordinal))
                    {
                        value = Enum.Parse(type, name);
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }

    /// <summary>The canonical text of a value; instants keep every tick, because a cursor must resume exactly.</summary>
    public static string? Format(object? value) => value switch
    {
        null => null,
        string s => s,
        bool b => b ? "true" : "false",
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
        DateTimeOffset instant => instant.UtcDateTime.ToString(InstantFormat, CultureInfo.InvariantCulture),
        DateTime instant => instant.ToUniversalTime().ToString(InstantFormat, CultureInfo.InvariantCulture),
        Guid guid => guid.ToString("D"),
        Enum e => JsonNamingPolicy.CamelCase.ConvertName(e.ToString()),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}
