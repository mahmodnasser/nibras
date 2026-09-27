using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Tenancy;

namespace Nibras.BuildingBlocks.Web;

/// <summary>One field of a <c>_VALIDATION_FAILED</c> body: the JSON path in the request and a code from the validation catalog.</summary>
public sealed record FieldError(string Field, string Code, JsonObject? Params = null);

/// <summary>
/// RFC 9457 Problem Details with the Nibras members of document 22 §8. Built only by this block, never by hand
/// in a handler; a handler returns a <c>Result</c> and <see cref="ResultExtensions.ToProblem"/> does the rest.
/// </summary>
public sealed record NibrasProblemDetails(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Instance,
    string Code,
    string CorrelationId,
    Guid? TenantId,
    bool ParentSafe,
    JsonObject? Params,
    IReadOnlyList<FieldError>? Errors,
    int? RetryAfterSeconds)
{
    public const string ContentType = "application/problem+json";

    public const string TypePrefix = "urn:nibras:problem:";

    /// <summary>Outside production only, for the dashboard.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>Builds the body for a catalog code in the context of the current request.</summary>
    public static NibrasProblemDetails For(
        HttpContext context,
        string code,
        string detail,
        JsonObject? parameters = null,
        IReadOnlyList<FieldError>? errors = null,
        int? retryAfterSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        var row = context.RequestServices.GetRequiredService<ErrorCatalog>().Get(code);
        var production = context.RequestServices.GetService<IHostEnvironment>()?.IsProduction() ?? true;
        return new NibrasProblemDetails(
            TypePrefix + row.Code,
            row.Title,
            row.Status,
            detail,
            (context.Request.PathBase + context.Request.Path).Value ?? "/",
            row.Code,
            CorrelationIdOf(context),
            context.RequestServices.GetService<ITenantContext>()?.Tenant?.Value,
            row.ParentSafe,
            parameters,
            errors,
            retryAfterSeconds)
        {
            TraceId = production ? null : Activity.Current?.TraceId.ToHexString(),
        };
    }

    /// <summary>
    /// The correlation id the telemetry middleware echoes on the response. A host that skipped that middleware
    /// still gets a body and a header that agree.
    /// </summary>
    private static string CorrelationIdOf(HttpContext context)
    {
        if (context.Items[CorrelationIdMiddleware.ItemKey] is string existing)
        {
            return existing;
        }

        var created = Guid.CreateVersion7().ToString("D");
        context.Items[CorrelationIdMiddleware.ItemKey] = created;
        context.Response.Headers[TelemetryConventions.CorrelationHeader] = created;
        return created;
    }

    internal async Task WriteAsync(HttpContext context)
    {
        var response = context.Response;
        response.StatusCode = Status;
        response.ContentType = ContentType;
        response.Headers.CacheControl = "no-store";
        if (RetryAfterSeconds is { } seconds)
        {
            response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        }

        await JsonSerializer.SerializeAsync(response.Body, this, ProblemJsonContext.Default.NibrasProblemDetails, context.RequestAborted)
            .ConfigureAwait(false);
    }
}

/// <summary>The Problem Details body as an endpoint result.</summary>
public sealed class NibrasProblemResult(string code, string detail, JsonObject? parameters = null, IReadOnlyList<FieldError>? errors = null)
    : IResult, IContentTypeHttpResult
{
    public string Code { get; } = code;

    public string ContentType => NibrasProblemDetails.ContentType;

    public Task ExecuteAsync(HttpContext httpContext) =>
        NibrasProblemDetails.For(httpContext, Code, detail, parameters, errors).WriteAsync(httpContext);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(NibrasProblemDetails))]
internal sealed partial class ProblemJsonContext : JsonSerializerContext;
