namespace Nibras.Contracts.ServiceName.ErrorCodes;

/// <summary>
/// The error catalog of the ServiceName service. The eight cross-cutting codes of Appendix K.1 come first;
/// the service's own codes from its Appendix K section are added by the slices that raise them.
/// </summary>
public static class ServiceNameErrorCodes
{
    public const string Prefix = "SERVICENAME_";

    public const string ValidationFailed = Prefix + "VALIDATION_FAILED";
    public const string PermissionDenied = Prefix + "PERMISSION_DENIED";
    public const string TenantMismatch = Prefix + "TENANT_MISMATCH";
    public const string NotFound = Prefix + "NOT_FOUND";
    public const string ConcurrencyConflict = Prefix + "CONCURRENCY_CONFLICT";
    public const string IdempotencyReplay = Prefix + "IDEMPOTENCY_REPLAY";
    public const string RateLimited = Prefix + "RATE_LIMITED";
    public const string DependencyUnavailable = Prefix + "DEPENDENCY_UNAVAILABLE";
}
