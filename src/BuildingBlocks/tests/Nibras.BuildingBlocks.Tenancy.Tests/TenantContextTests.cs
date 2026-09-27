using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Observability;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Tenancy.Tests;

public sealed class TenantContextTests
{
    private static readonly TenantId TenantA = new(Guid.Parse("018f7c2a-0b1d-7e2f-9c3b-000000000001"));
    private static readonly TenantId TenantB = new(Guid.Parse("018f7c2a-0b1d-7e2f-9c3b-000000000002"));

    [Fact]
    public void The_empty_guid_is_never_a_tenant() =>
        Should.Throw<ArgumentException>(() => new TenantId(Guid.Empty));

    [Fact]
    public void A_scope_starts_without_a_tenant_and_keeps_the_one_it_is_given()
    {
        var context = new TenantContext();
        context.Tenant.ShouldBeNull();

        context.Set(TenantA);
        context.Set(TenantA);

        context.Tenant.ShouldBe(TenantA);
    }

    [Fact]
    public void A_scope_cannot_switch_to_another_tenant()
    {
        var context = new TenantContext();
        context.Set(TenantA);

        Should.Throw<InvalidOperationException>(() => context.Set(TenantB));
        context.Tenant.ShouldBe(TenantA);
    }

    [Fact]
    [Trait("TestCase", "TC-INF-111")]
    public void Telemetry_labels_a_request_with_the_scope_tenant()
    {
        var services = new ServiceCollection();
        services.AddNibrasTenancy();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>().Set(TenantA);
        var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        provider.GetRequiredService<ITelemetryTenantSource>().CurrentTenantId(http).ShouldBe(TenantA.Value);
    }
}
