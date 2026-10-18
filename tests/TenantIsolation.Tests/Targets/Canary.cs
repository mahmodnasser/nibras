using Nibras.BuildingBlocks.Testing.Isolation.Canary;

namespace Nibras.TenantIsolation.Tests.Targets;

/// <summary>
/// The canary service from the Testing block, attacked on every build beside the real services, so a run that attacks
/// nothing is impossible and the report always shows the engine at work. A service's own target sits beside it.
/// </summary>
public sealed class Canary : CanaryTarget;
