using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nibras.BuildingBlocks.Web;

/// <summary>
/// The two places where the framework, not a handler, ends a request: an exception, and a status with no body
/// (an unmatched route, a refused request). Both answer with the catalog's Problem Details (document 22 §8).
/// </summary>
internal static partial class ErrorHandling
{
    /// <summary>The exception handler middleware's delegate.</summary>
    public static Task HandleExceptionAsync(HttpContext context)
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var catalog = context.RequestServices.GetRequiredService<ErrorCatalog>();

        if (feature?.Error is BadHttpRequestException bad)
        {
            if (bad.StatusCode != StatusCodes.Status400BadRequest)
            {
                context.Response.StatusCode = bad.StatusCode;
                return Task.CompletedTask;
            }

            var body = NibrasProblemDetails.For(
                context, catalog.Prefix + ErrorCatalog.ValidationFailed, "The request could not be read.", errors: [FieldErrorOf(bad)]);
            return body.WriteAsync(context);
        }

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Nibras.BuildingBlocks.Web.Errors");
        LogUnhandled(logger, feature?.Error, feature?.Endpoint?.DisplayName ?? "unknown");

        var problem = NibrasProblemDetails.For(context, catalog.Prefix + ErrorCatalog.InternalError, "An unexpected failure.");
        return (problem with { Detail = "Reference " + problem.CorrelationId + "." }).WriteAsync(context);
    }

    /// <summary>The body-less error statuses that have a cross-cutting code; any other status is left as it is.</summary>
    public static async Task HandleStatusCodeAsync(StatusCodeContext statusContext)
    {
        var context = statusContext.HttpContext;
        var suffix = context.Response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => ErrorCatalog.ValidationFailed,
            StatusCodes.Status403Forbidden => ErrorCatalog.PermissionDenied,
            StatusCodes.Status404NotFound => ErrorCatalog.NotFound,
            StatusCodes.Status409Conflict => ErrorCatalog.ConcurrencyConflict,
            StatusCodes.Status429TooManyRequests => ErrorCatalog.RateLimited,
            StatusCodes.Status500InternalServerError => ErrorCatalog.InternalError,
            StatusCodes.Status503ServiceUnavailable => ErrorCatalog.DependencyUnavailable,
            _ => null,
        };
        if (suffix is null)
        {
            return;
        }

        var catalog = context.RequestServices.GetRequiredService<ErrorCatalog>();
        IReadOnlyList<FieldError>? errors = suffix == ErrorCatalog.ValidationFailed ? [new FieldError("$", "invalid")] : null;
        await NibrasProblemDetails.For(context, catalog.Prefix + suffix, "No further detail.", errors: errors)
            .WriteAsync(context)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// The JSON path of the offending property. System.Text.Json reports an unknown property with the path of
    /// the property itself, so <c>$.studnetNumber</c> becomes the field <c>studnetNumber</c> (REQ-API-004).
    /// </summary>
    internal static FieldError FieldErrorOf(BadHttpRequestException exception)
    {
        if (exception.InnerException is not JsonException json)
        {
            return new FieldError("$", "invalid");
        }

        var path = json.Path switch
        {
            null or "" or "$" => "$",
            var p when p.StartsWith("$.", StringComparison.Ordinal) => p[2..],
            var p => p,
        };
        var code = json.Message.Contains("could not be mapped to any .NET member", StringComparison.Ordinal) ? "unknownProperty" : "invalidValue";
        return new FieldError(path, code);
    }

    [LoggerMessage(EventId = 5000, Level = LogLevel.Error, Message = "Unhandled exception in {Endpoint}")]
    private static partial void LogUnhandled(ILogger logger, Exception? exception, string endpoint);
}
