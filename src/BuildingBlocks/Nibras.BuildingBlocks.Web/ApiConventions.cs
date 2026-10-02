using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Nibras.BuildingBlocks.Web;

public static partial class ApiConventions
{
    /// <summary>
    /// The route group <c>/api/v{version}/{service}</c> of document 22 §1.1. Every endpoint it holds carries, from the
    /// outside in: the <c>Idempotency-Key</c> filter on a <c>POST</c> (§5), the <c>If-Match</c> requirement on a
    /// <c>PUT</c>, <c>PATCH</c> or <c>DELETE</c> (§4), the validation filter and the <see cref="ETagFilter"/>.
    /// </summary>
    public static RouteGroupBuilder MapNibrasApi(this IEndpointRouteBuilder endpoints, int version)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<NibrasWebOptions>>().Value;
        var group = endpoints.MapGroup(string.Create(CultureInfo.InvariantCulture, $"/api/v{version}/{options.ApiSegment}"));
        // These two read the endpoint's own metadata (RequireIdempotencyKey, WithoutIfMatch), which is complete only
        // when the filters are built, after every convention has run.
        ((IEndpointConventionBuilder)group).Add(e => e.FilterFactories.Add((factory, next) => IdempotencyKeyFilter.Create(e, factory, next)));
        ((IEndpointConventionBuilder)group).Add(e => e.FilterFactories.Add((factory, next) => IfMatchFilter.Create(e, factory, next)));
        group.AddEndpointFilterFactory(ValidationFilter.Create);
        group.AddEndpointFilter<ETagFilter>();
        return group;
    }

    /// <summary>
    /// The violations of the URL shape among a set of routes: a version that is not a positive integer, a service
    /// segment that is not this service's, a literal that is not kebab-case, two parameters in a row, or a path
    /// deeper than one sub-resource below an item. Plurality is Spectral's to judge (SL-API-005).
    /// </summary>
    public static IReadOnlyList<string> Violations(IEnumerable<RoutePattern> routes, string apiSegment)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var violations = new List<string>();
        foreach (var route in routes)
        {
            var segments = route.PathSegments;
            if (segments.Count == 0 || !IsLiteral(segments[0], out var first) || first != "api")
            {
                continue; // probes, /bff/…, and anything else outside the public API shape
            }

            var raw = route.RawText ?? "(unnamed route)";
            if (segments.Count < 4 || !IsLiteral(segments[1], out var version) || !VersionSegment().IsMatch(version))
            {
                violations.Add($"{raw}: the prefix is /api/v{{n}}/<service>/<resource>.");
                continue;
            }

            if (!IsLiteral(segments[2], out var service) || service != apiSegment)
            {
                violations.Add($"{raw}: the service segment must be '{apiSegment}'.");
                continue;
            }

            var shape = new List<char>();
            for (var i = 3; i < segments.Count; i++)
            {
                if (IsLiteral(segments[i], out var literal))
                {
                    if (!KebabCase().IsMatch(literal))
                    {
                        violations.Add($"{raw}: '{literal}' is not kebab-case.");
                    }

                    shape.Add('L');
                }
                else if (segments[i].Parts is [RoutePatternParameterPart])
                {
                    shape.Add('P');
                }
                else
                {
                    violations.Add($"{raw}: a segment is either a literal or a single parameter.");
                    shape.Add('?');
                }
            }

            // resource, item, sub-resource or action, sub-item; or a collection action (bulk, jobs) with an optional job id.
            var text = new string([.. shape]);
            var collectionAction = segments.Count >= 5 && IsLiteral(segments[4], out var action) && action is "bulk" or "jobs";
            var allowed = text is "L" or "LP" or "LPL" or "LPLP" || (collectionAction && text is "LL" or "LLP");
            if (!allowed)
            {
                violations.Add($"{raw}: at most one sub-resource level below an item.");
            }
        }

        return violations;
    }

    private static bool IsLiteral(RoutePatternPathSegment segment, out string literal)
    {
        if (segment.Parts is [RoutePatternLiteralPart part])
        {
            literal = part.Content;
            return true;
        }

        literal = "";
        return false;
    }

    [GeneratedRegex("^v[1-9][0-9]*$", RegexOptions.CultureInvariant)]
    private static partial Regex VersionSegment();

    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    internal static partial Regex KebabCase();
}

/// <summary>
/// Fails the start of a host whose API routes break the URL shape (REQ-API-001): the host exits before its
/// readiness probe ever passes, so the mistake never reaches a client.
/// </summary>
internal sealed class ApiRouteCheck(EndpointDataSource endpoints, IOptions<NibrasWebOptions> options) : IHostedLifecycleService
{
    /// <summary>The endpoint data source is wired when the server builds its pipeline, so the check runs once that is done.</summary>
    public Task StartedAsync(CancellationToken cancellationToken)
    {
        var routes = endpoints.Endpoints.OfType<RouteEndpoint>().Select(e => e.RoutePattern);
        var violations = ApiConventions.Violations(routes, options.Value.ApiSegment);
        return violations.Count == 0
            ? Task.CompletedTask
            : throw new InvalidOperationException("API routes break document 22 §1.1:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
