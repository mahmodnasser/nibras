using System.Reflection;
using Nibras.Contracts.ServiceName.ErrorCodes;
using Nibras.ServiceName.Domain.Shared;
using Shouldly;
using Xunit;

namespace Nibras.ServiceName.UnitTests;

/// <summary>The error catalog carries the eight Appendix K.1 codes under this service's prefix.</summary>
public sealed class ErrorCatalogTests
{
    private static readonly string[] CrossCuttingSuffixes =
    [
        "VALIDATION_FAILED", "PERMISSION_DENIED", "TENANT_MISMATCH", "NOT_FOUND",
        "CONCURRENCY_CONFLICT", "IDEMPOTENCY_REPLAY", "RATE_LIMITED", "DEPENDENCY_UNAVAILABLE",
    ];

    private static List<string> Codes() =>
        typeof(ServiceNameErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.Name != nameof(ServiceNameErrorCodes.Prefix))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

    [Fact]
    public void Every_cross_cutting_code_of_appendix_K1_is_present_with_the_service_prefix()
    {
        var codes = Codes();

        foreach (var suffix in CrossCuttingSuffixes)
        {
            codes.ShouldContain(ServiceNameErrorCodes.Prefix + suffix);
        }
    }

    [Fact]
    public void Every_code_starts_with_the_service_prefix_and_appears_once()
    {
        var codes = Codes();

        codes.ShouldAllBe(code => code.StartsWith(ServiceNameErrorCodes.Prefix, StringComparison.Ordinal));
        codes.Distinct(StringComparer.Ordinal).Count().ShouldBe(codes.Count);
    }

    [Fact]
    public void The_domain_errors_use_the_contract_codes()
    {
        ServiceNameErrors.ValidationFailed("A field is missing.").Code.ShouldBe(ServiceNameErrorCodes.ValidationFailed);
        ServiceNameErrors.NotFound("No such record.").Code.ShouldBe(ServiceNameErrorCodes.NotFound);
        ServiceNameErrors.ConcurrencyConflict("The record changed.").Code.ShouldBe(ServiceNameErrorCodes.ConcurrencyConflict);
    }
}
