using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nibras.BuildingBlocks.Persistence;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Messaging.Tests;

public sealed class TopologyTests
{
    [Fact]
    [Trait("TestCase", "TC-MSG-955")]
    public void A_service_publishes_on_its_own_exchange_whose_alternate_catches_what_no_binding_matches()
    {
        MessagingTopology.Exchange("Attendance").ShouldBe("nibras.attendance");
        MessagingTopology.UnroutedExchange("Attendance").ShouldBe("nibras.attendance.unrouted");
        MessagingTopology.UnroutedQueue("Attendance").ShouldBe("attendance.unrouted");
        Should.Throw<ArgumentException>(() => MessagingTopology.Exchange("attendance-service"));
    }

    [Theory]
    [Trait("TestCase", "TC-MSG-955")]
    [InlineData("Attendance", "attendance.student.absent.v1", 1)]
    [InlineData("Platform", "platform.tenant.provisioning-requested.v1", 1)]
    [InlineData("Attendance", "attendance.attendance-session.closed.v12", 12)]
    public void A_routing_key_of_the_convention_names_its_schema_version(string service, string key, int version) =>
        MessagingTopology.SchemaVersion(service, key).ShouldBe(version);

    [Theory]
    [Trait("TestCase", "TC-MSG-955")]
    [InlineData("attendance.student.absent")]
    [InlineData("attendance.student.absent.v0")]
    [InlineData("attendance.Student.absent.v1")]
    [InlineData("attendance.student_absent.v1")]
    [InlineData("attendance.student.absent.v1.extra")]
    [InlineData("finance.invoice.issued.v1")]
    public void A_key_without_the_shape_or_under_another_service_is_refused(string key) =>
        Should.Throw<ArgumentException>(() => MessagingTopology.SchemaVersion("Attendance", key)).Message.ShouldContain(key);

    [Fact]
    public void Settings_are_refused_with_every_problem_named()
    {
        var problems = MessagingOptionsValidator.Problems(new MessagingOptions { Service = "attendance", MaxPerTenant = 0 }, production: false).ToList();

        problems.Count.ShouldBe(5);
        problems.ShouldContain(p => p.Contains("Appendix L service name", StringComparison.Ordinal));
        problems.ShouldContain(p => p.StartsWith("Broker must be an amqp", StringComparison.Ordinal));
        problems.ShouldContain(p => p.StartsWith("ApplicationConnectionString", StringComparison.Ordinal));
        problems.ShouldContain(p => p.StartsWith("SessionConnectionString", StringComparison.Ordinal));
        problems.ShouldContain(p => p.StartsWith("MaxPerTenant", StringComparison.Ordinal));
    }

    [Fact]
    public void Production_refuses_a_broker_without_TLS_and_accepts_amqps()
    {
        var options = new MessagingOptions { Service = "Attendance", Broker = new Uri("amqp://broker:5672/"), ApplicationConnectionString = "Host=a", SessionConnectionString = "Host=b" };

        MessagingOptionsValidator.Problems(options, production: true).ShouldBe(["Broker must be amqps:// in production (master brief Section 20)."]);
        MessagingOptionsValidator.Problems(options, production: false).ShouldBeEmpty();
        options.Broker = new Uri("amqps://broker:5671/");
        MessagingOptionsValidator.Problems(options, production: true).ShouldBeEmpty();
    }

    [Fact]
    public async Task A_host_with_invalid_settings_does_not_start()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = "Production", DisableDefaults = true });
        builder.Logging.ClearProviders();
        builder.Services.AddNibrasDbContext<ProbeDbContext>("Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x");
        builder.Services.AddNibrasMessaging<ProbeDbContext>(o =>
        {
            o.Service = "Probe";
            o.Broker = new Uri("amqp://broker/");
            o.ApplicationConnectionString = "Host=a";
            o.SessionConnectionString = "Host=b";
        });
        using var host = builder.Build();

        var refused = await Should.ThrowAsync<OptionsValidationException>(() => host.StartAsync(TestContext.Current.CancellationToken));

        refused.Message.ShouldContain("amqps:// in production");
    }

    [Fact]
    public void A_refused_row_holds_back_the_later_rows_of_its_key_but_not_other_keys()
    {
        static OutboxRow Row(string? key) => new(Guid.CreateVersion7(), "nibras.probe", "probe.item.created.v1", "T", 1, key, Guid.CreateVersion7(), null, null, default, null, null, null, []);
        var rows = new[] { Row("a"), Row("a"), Row("b"), Row(null), Row("a") };
        (string?, string?)[] outcomes = [(null, null), ("negative confirm", "nack"), (null, null), (null, null), (null, null)];

        var (dispatched, failed) = OutboxDispatcher.Decide(rows, outcomes);

        dispatched.ShouldBe([rows[0], rows[2], rows[3]]);
        failed.Select(f => f.Row).ShouldBe([rows[1]]);
        failed.Single().Reason.ShouldBe("nack");
    }
}
