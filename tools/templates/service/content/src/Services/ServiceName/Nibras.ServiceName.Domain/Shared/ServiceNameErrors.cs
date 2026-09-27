using Nibras.BuildingBlocks.Domain;
using Nibras.Contracts.ServiceName.ErrorCodes;

namespace Nibras.ServiceName.Domain.Shared;

/// <summary>The domain's errors, each wired to its code in the contract project so the code is defined once.</summary>
public static class ServiceNameErrors
{
    public static Error ValidationFailed(string message) => new(ServiceNameErrorCodes.ValidationFailed, message);

    public static Error NotFound(string message) => new(ServiceNameErrorCodes.NotFound, message);

    public static Error ConcurrencyConflict(string message) => new(ServiceNameErrorCodes.ConcurrencyConflict, message);
}
