using Nibras.BuildingBlocks.Observability;
using Nibras.ServiceDefaults;
using Nibras.ServiceName.Application;
using Nibras.ServiceName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddNibrasServiceDefaults("ServiceName");
builder.Services.AddServiceNameApplication();
builder.Services.AddServiceNameInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseNibrasTelemetry();
app.MapNibrasDefaultEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>The entry point, visible to the integration tests' WebApplicationFactory.</summary>
public partial class Program;
