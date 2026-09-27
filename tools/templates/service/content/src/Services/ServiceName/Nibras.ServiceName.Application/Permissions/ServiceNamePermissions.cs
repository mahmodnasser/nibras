namespace Nibras.ServiceName.Application.Permissions;

/// <summary>
/// The permission strings of this service, exactly as Appendix B lists them. Every endpoint names one
/// constant from this class (architecture rule TC-TST-117); the slice that adds an endpoint adds its constant.
/// </summary>
public static class ServiceNamePermissions
{
    /// <summary>The Appendix L.4 namespace every permission of this service starts with.</summary>
    public const string Namespace = "servicename.";
}
