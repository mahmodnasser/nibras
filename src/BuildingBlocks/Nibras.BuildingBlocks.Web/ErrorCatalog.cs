using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Nibras.BuildingBlocks.Web;

/// <summary>One row of a service's error catalog (Appendix K): the code, its HTTP status, the English developer title and whether a guardian may read it.</summary>
public sealed record ErrorCode(string Code, int Status, string Title, bool ParentSafe);

/// <summary>
/// The error catalog of one service. The cross-cutting codes of Appendix K.1 are registered with the service's
/// prefix; the service adds its own section's codes. A code missing from the catalog is a defect, never a guess:
/// turning it into a response fails, and the failure itself becomes <c>_INTERNAL_ERROR</c>.
/// </summary>
public sealed class ErrorCatalog
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string PermissionDenied = "PERMISSION_DENIED";
    public const string TenantMismatch = "TENANT_MISMATCH";
    public const string NotFound = "NOT_FOUND";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string RateLimited = "RATE_LIMITED";
    public const string DependencyUnavailable = "DEPENDENCY_UNAVAILABLE";

    /// <summary>Emitted by the middleware for an unhandled exception (document 22 §8; proposed for Appendix K.1 as the ninth).</summary>
    public const string InternalError = "INTERNAL_ERROR";

    private readonly Dictionary<string, ErrorCode> _codes = new(StringComparer.Ordinal);

    public ErrorCatalog(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        if (!prefix.EndsWith('_') || prefix.Any(c => c is not ((>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_')))
        {
            throw new ArgumentException($"'{prefix}' is not an upper-snake-case prefix such as ATTENDANCE_.", nameof(prefix));
        }

        Prefix = prefix;

        // _IDEMPOTENCY_REPLAY is a 200 success, not an error body; SL-API-004's replay header carries it.
        Add(prefix + ValidationFailed, StatusCodes.Status400BadRequest, parentSafe: true);
        Add(prefix + PermissionDenied, StatusCodes.Status403Forbidden, parentSafe: true);
        Add(prefix + TenantMismatch, StatusCodes.Status403Forbidden, parentSafe: false);
        Add(prefix + NotFound, StatusCodes.Status404NotFound, parentSafe: true);
        Add(prefix + ConcurrencyConflict, StatusCodes.Status409Conflict, parentSafe: true);
        Add(prefix + RateLimited, StatusCodes.Status429TooManyRequests, parentSafe: true);
        Add(prefix + DependencyUnavailable, StatusCodes.Status503ServiceUnavailable, parentSafe: true);
        Add(prefix + InternalError, StatusCodes.Status500InternalServerError, parentSafe: false);
    }

    /// <summary>The service prefix, for example <c>ATTENDANCE_</c>.</summary>
    public string Prefix { get; }

    public IReadOnlyCollection<ErrorCode> Codes => _codes.Values;

    /// <summary>Adds a code of the service's own Appendix K section. The title defaults to the meaning in sentence case.</summary>
    public ErrorCatalog Add(string code, int status, bool parentSafe, string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (!code.StartsWith(Prefix, StringComparison.Ordinal) || code.Length == Prefix.Length)
        {
            throw new ArgumentException($"Code '{code}' does not carry the service prefix {Prefix}.", nameof(code));
        }

        if (status is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "An error code maps to a 4xx or 5xx status.");
        }

        if (!_codes.TryAdd(code, new ErrorCode(code, status, title ?? TitleOf(code[Prefix.Length..]), parentSafe)))
        {
            throw new ArgumentException($"Code '{code}' is already in the catalog.", nameof(code));
        }

        return this;
    }

    public ErrorCode Get(string code) =>
        _codes.TryGetValue(code, out var row)
            ? row
            : throw new KeyNotFoundException($"Error code '{code}' is not in the {Prefix} catalog (Appendix K).");

    /// <summary>The cross-cutting code for a suffix such as <see cref="NotFound"/>.</summary>
    public ErrorCode CrossCutting(string suffix) => Get(Prefix + suffix);

    private static string TitleOf(string meaning)
    {
        var words = meaning.ToLowerInvariant().Replace('_', ' ');
        return char.ToUpper(words[0], CultureInfo.InvariantCulture) + words[1..];
    }
}
