using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentValidation;
using Nibras.BuildingBlocks.Tenancy;
using Nibras.BuildingBlocks.Web.Lists;

namespace Nibras.BuildingBlocks.Web;

/// <summary>The service a host serves, as the URL segment and the error prefix derive from it.</summary>
public sealed class NibrasWebOptions
{
    /// <summary>The Appendix L service name in PascalCase, for example <c>Attendance</c>.</summary>
    public string Service { get; set; } = "";

    /// <summary><c>attendance</c>: the segment after <c>/api/v{n}/</c>.</summary>
    public string ApiSegment => Service.ToLowerInvariant();

    /// <summary><c>ATTENDANCE_</c>: the prefix of every code in the service's catalog.</summary>
    public string ErrorPrefix => Service.ToUpperInvariant() + "_";
}

/// <summary>The output-cache policies. Only public, anonymous reads are cached here; a tenant's private data never is.</summary>
public static class NibrasOutputCache
{
    /// <summary>A public page or lookup, one entry per host (the tenant's domain), for 60 seconds.</summary>
    public const string Public = "nibras-public";
}

public static class DependencyInjection
{
    /// <summary>
    /// The API conventions of document 22 for one service: JSON, the error catalog with the Appendix K.1 codes,
    /// Problem Details, validation, compression, output caching and the startup check of the URL shape.
    /// </summary>
    public static IServiceCollection AddNibrasWeb(
        this IServiceCollection services,
        string service,
        Action<ErrorCatalog>? catalog = null,
        params IJsonTypeInfoResolver[] jsonContexts)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        if (!service.All(char.IsAsciiLetterOrDigit) || !char.IsAsciiLetterUpper(service[0]))
        {
            throw new ArgumentException($"'{service}' is not an Appendix L service name such as Attendance.", nameof(service));
        }

        services.Configure<NibrasWebOptions>(o => o.Service = service);
        var errors = new ErrorCatalog(service.ToUpperInvariant() + "_");
        catalog?.Invoke(errors);
        services.TryAddSingleton(errors);
        services.AddNibrasTenancy();

        services.ConfigureHttpJsonOptions(o => NibrasJson.Configure(o.SerializerOptions, jsonContexts));
        services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true); // so the handler can name the field

        services.AddResponseCompression(o =>
        {
            // In-cluster traffic is plain HTTP behind the Gateway; compressing HTTPS responses that echo secrets invites BREACH.
            o.EnableForHttps = false;
            o.Providers.Add<BrotliCompressionProvider>();
            o.Providers.Add<GzipCompressionProvider>();
            o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([NibrasProblemDetails.ContentType]);
        });
        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

        services.AddOutputCache(o => o.AddPolicy(NibrasOutputCache.Public, policy => policy.Expire(TimeSpan.FromSeconds(60)).SetVaryByHost(true)));
        services.TryAddSingleton<ListCursors>();
        services.AddHostedService<ApiRouteCheck>();
        return services;
    }

    /// <summary>
    /// Compression, Problem Details for exceptions and body-less error statuses, output caching, the default
    /// <c>Cache-Control: no-store</c> and a re-readable body for a <c>POST</c> with an <c>Idempotency-Key</c>. Call after <c>UseNibrasTelemetry</c>, so every body carries the correlation id.
    /// </summary>
    public static IApplicationBuilder UseNibrasWeb(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseResponseCompression();
        app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = ErrorHandling.HandleExceptionAsync });
        app.UseStatusCodePages(ErrorHandling.HandleStatusCodeAsync);
        app.Use(static (context, next) =>
        {
            context.Response.OnStarting(static state =>
            {
                var response = (HttpResponse)state;
                if (string.IsNullOrEmpty(response.Headers.CacheControl))
                {
                    response.Headers.CacheControl = "no-store";
                }

                return Task.CompletedTask;
            }, context.Response);
            return next(context);
        });
        app.UseOutputCache();
        app.Use(static (context, next) =>
        {
            // The Idempotency-Key filter fingerprints the body after the endpoint has bound it, so keep it readable.
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Headers.ContainsKey(IdempotencyKeyFilter.HeaderName))
            {
                context.Request.EnableBuffering();
            }

            return next(context);
        });
        return app;
    }

    /// <summary>Registers every concrete FluentValidation validator of an assembly, scoped, under its <c>IValidator&lt;T&gt;</c>.</summary>
    public static IServiceCollection AddNibrasValidators(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }))
        {
            foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            {
                services.TryAddEnumerable(ServiceDescriptor.Scoped(contract, type));
            }
        }

        return services;
    }
}
