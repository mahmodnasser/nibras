using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Web;
using Nibras.ServiceDefaults;
using Nibras.ServiceName.Application;
using Nibras.ServiceName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddNibrasServiceDefaults("ServiceName");
builder.Services.AddNibrasWeb("ServiceName");
builder.Services.AddServiceNameApplication();
builder.Services.AddServiceNameInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseNibrasTelemetry();
app.UseNibrasWeb();
app.MapNibrasDefaultEndpoints();

// The service's endpoints join this group, one feature folder each (document 22 §1.1).
app.MapNibrasApi(1);

await app.RunAsync().ConfigureAwait(false);

/// <summary>The entry point, visible to the integration tests' WebApplicationFactory.</summary>
public partial class Program;
