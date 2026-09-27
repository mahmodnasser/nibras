namespace Nibras.Contracts.ServiceName;

/// <summary>
/// The exchange this service publishes on and its routing keys. A key is added here only after its row
/// exists in Appendix E (document 07, part 8); the slice that first publishes an event adds it.
/// </summary>
public static class RoutingKeys
{
    public const string Exchange = "nibras.servicename";
}
