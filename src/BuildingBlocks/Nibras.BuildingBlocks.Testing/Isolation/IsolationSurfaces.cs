using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Nibras.BuildingBlocks.Testing.Isolation;

public enum SurfaceKind
{
    /// <summary>An HTTP endpoint, once per method.</summary>
    Endpoint,

    /// <summary>A message consumer.</summary>
    Consumer,

    /// <summary>The service's pooled database connection.</summary>
    Connection,
}

/// <summary>One thing the suite attacks: an endpoint of the public API, once per HTTP method, or a consumer.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="Name">How the generated test and the report name it: <c>GET /api/v1/attendance/sessions/{id:guid}</c>, <c>consumer attendance.session.closed.v1</c>.</param>
/// <param name="Method">The HTTP method, or <c>CONSUME</c>.</param>
/// <param name="Route">The route of an endpoint.</param>
/// <param name="BodyType">The request body an endpoint binds, if any.</param>
public sealed record IsolationSurface(SurfaceKind Kind, string Name, string Method, RoutePattern? Route, Type? BodyType)
{
    public const string ConsumerPrefix = "consumer ";

    /// <summary>The route parameters that carry an identifier: <c>id</c>, or a camelCase name ending in <c>Id</c> (document 22 §1.1).</summary>
    public IReadOnlyList<string> IdentifierParameters =>
        Route?.Parameters.Where(p => IsIdentifier(p.Name)).Select(p => p.Name).ToList() ?? [];

    public static bool IsIdentifier(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name == "id" || (name.Length > 2 && name.EndsWith("Id", StringComparison.Ordinal));
    }
}

public static class IsolationSurfaces
{
    private const string UnopenedConnection = "Host=127.0.0.1;Port=1;Database=unopened;Username=unopened;Password=unopened";

    /// <summary>
    /// Every endpoint of the public API shape (<c>/api/...</c>) once per method, then every consumer, ordered by name.
    /// The probes and any route outside the API are not tenant surfaces.
    /// </summary>
    public static IReadOnlyList<IsolationSurface> Of(IEnumerable<EndpointDataSource> sources, IEnumerable<IsolationConsumer> consumers)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(consumers);
        var surfaces = new List<IsolationSurface>();
        foreach (var endpoint in sources.SelectMany(s => s.Endpoints).OfType<RouteEndpoint>())
        {
            var route = endpoint.RoutePattern;
            if (route.PathSegments.Count == 0 || route.PathSegments[0].Parts is not [RoutePatternLiteralPart { Content: "api" }])
            {
                continue;
            }

            var body = endpoint.Metadata.GetMetadata<IAcceptsMetadata>()?.RequestType;
            foreach (var method in endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [])
            {
                surfaces.Add(new IsolationSurface(SurfaceKind.Endpoint, $"{method} {route.RawText}", method, route, body));
            }
        }

        surfaces.AddRange(consumers.Select(c => new IsolationSurface(SurfaceKind.Consumer, IsolationSurface.ConsumerPrefix + c.Name, "CONSUME", null, null)));
        return [.. surfaces.DistinctBy(s => s.Name).OrderBy(s => s.Name, StringComparer.Ordinal)];
    }

    /// <summary>The surfaces of a target, read from its host started over a connection it never opens.</summary>
    public static async Task<IReadOnlyList<IsolationSurface>> DiscoverAsync(IsolationTarget target, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        var host = target.BuildHost(UnopenedConnection);
        await using (host.ConfigureAwait(false))
        {
            await host.StartAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return Of(((IEndpointRouteBuilder)host).DataSources, target.Consumers);
            }
            finally
            {
                await host.StopAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
