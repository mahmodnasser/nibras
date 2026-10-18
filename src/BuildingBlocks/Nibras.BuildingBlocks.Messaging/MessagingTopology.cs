using System.Globalization;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace Nibras.BuildingBlocks.Messaging;

/// <summary>
/// The publishing topology of document 11 part 1.1: one topic exchange per service, <c>nibras.&lt;service&gt;</c>, whose
/// alternate exchange catches a message no binding matches (REQ-MSG-021), and routing keys of the shape
/// <c>&lt;service&gt;.&lt;entity&gt;.&lt;event&gt;.v&lt;n&gt;</c> under the publisher's own prefix (REQ-MSG-005).
/// </summary>
public static partial class MessagingTopology
{
    /// <summary>The cap of a service's unrouted queue; the oldest message goes first once it is full.</summary>
    public const int UnroutedQueueLength = 10_000;

    /// <summary><c>attendance</c> for <c>Attendance</c>: the routing-key prefix and the exchange suffix.</summary>
    public static string Segment(string service)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        if (!service.All(char.IsAsciiLetterOrDigit) || !char.IsAsciiLetterUpper(service[0]))
        {
            throw new ArgumentException($"'{service}' is not an Appendix L service name such as Attendance.", nameof(service));
        }

        return service.ToLowerInvariant();
    }

    public static string Exchange(string service) => "nibras." + Segment(service);

    public static string UnroutedExchange(string service) => Exchange(service) + ".unrouted";

    public static string UnroutedQueue(string service) => Segment(service) + ".unrouted";

    /// <summary>
    /// The schema version a routing key names, once the key is known to have the convention's shape and the
    /// publisher's own prefix: a service publishes only under its own name (document 11 part 1.4).
    /// </summary>
    public static int SchemaVersion(string service, string routingKey)
    {
        ArgumentNullException.ThrowIfNull(routingKey);
        var match = RoutingKeyShape().Match(routingKey);
        if (!match.Success)
        {
            throw new ArgumentException($"Routing key '{routingKey}' is not <service>.<entity>.<event>.v<n> in kebab case (REQ-MSG-005).", nameof(routingKey));
        }

        if (match.Groups["service"].Value != Segment(service))
        {
            throw new ArgumentException($"Routing key '{routingKey}' belongs to another service; {service} publishes only '{Segment(service)}.' keys.", nameof(routingKey));
        }

        return int.Parse(match.Groups["version"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    /// <summary>Declares the exchange, its alternate exchange and the unrouted queue, idempotently, before anything is published.</summary>
    public static async Task DeclareAsync(IChannel channel, string service, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(channel);
        await channel.ExchangeDeclareAsync(UnroutedExchange(service), ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await channel.QueueDeclareAsync(
            UnroutedQueue(service),
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["x-queue-type"] = "quorum",
                ["x-max-length"] = UnroutedQueueLength,
                ["x-overflow"] = "drop-head",
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(UnroutedQueue(service), UnroutedExchange(service), routingKey: "", cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.ExchangeDeclareAsync(
            Exchange(service),
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: new Dictionary<string, object?>(StringComparer.Ordinal) { ["alternate-exchange"] = UnroutedExchange(service) },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    [GeneratedRegex(@"^(?<service>[a-z][a-z0-9]*)\.[a-z][a-z0-9]*(-[a-z0-9]+)*\.[a-z][a-z0-9]*(-[a-z0-9]+)*\.v(?<version>[1-9][0-9]*)$", RegexOptions.CultureInvariant)]
    private static partial Regex RoutingKeyShape();
}
