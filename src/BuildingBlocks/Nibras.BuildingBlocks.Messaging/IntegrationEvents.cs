using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Web;
using Nibras.Contracts.Shared;

namespace Nibras.BuildingBlocks.Messaging;

/// <summary>
/// Writes an integration event to the outbox of the current unit of work (document 11 part 8.2). The row is saved by
/// the handler's own <c>SaveChangesAsync</c>, in the transaction of the state change, and is published only after
/// that transaction commits (REQ-MSG-003). A rolled-back handler publishes nothing.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>Adds the event, a contract record carrying <see cref="IntegrationEventAttribute"/>, to the outbox.</summary>
    void Enqueue<TEvent>(TEvent message)
        where TEvent : class;
}

/// <summary>
/// What is being handled, for the envelope of what it publishes. The consumer host sets both from the message it
/// delivers (SL-MSG-002); in a request the correlation id comes from the correlation middleware.
/// </summary>
public sealed class MessageContext
{
    public Guid? CorrelationId { get; set; }

    public Guid? CausationId { get; set; }
}

/// <summary>What the outbox needs to know about a contract type, read once from its attributes.</summary>
internal sealed record IntegrationEventDescriptor(string RoutingKey, PropertyInfo? PartitionKey)
{
    private static readonly ConcurrentDictionary<Type, IntegrationEventDescriptor> Cache = new();

    public static IntegrationEventDescriptor For(Type type) => Cache.GetOrAdd(type, Read);

    public string? PartitionKeyOf(object message) =>
        PartitionKey?.GetValue(message) is { } value ? Convert.ToString(value, CultureInfo.InvariantCulture) : null;

    private static IntegrationEventDescriptor Read(Type type)
    {
        var declared = type.GetCustomAttribute<IntegrationEventAttribute>()
            ?? throw new ArgumentException($"{type.FullName} is not an integration event; declare its Appendix E routing key with [IntegrationEvent].", nameof(type));
        var keys = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsDefined(typeof(PartitionKeyAttribute))).ToList();
        if (keys.Count > 1)
        {
            throw new ArgumentException($"{type.FullName} marks {keys.Count} properties with [PartitionKey]; a message has one ordering key.", nameof(type));
        }

        return new IntegrationEventDescriptor(declared.RoutingKey, keys.SingleOrDefault());
    }
}

internal sealed class IntegrationEventPublisher<TContext>(
    TContext db,
    IOptions<MessagingOptions> options,
    ICurrentUser user,
    TimeProvider clock,
    MessageContext context,
    IServiceProvider services) : IIntegrationEventPublisher
    where TContext : NibrasDbContext
{
    // The wire format of document 22, so a message and a response body read the same (document 11 part 2.1).
    private static readonly JsonSerializerOptions Json = NibrasJson.Configure(new JsonSerializerOptions());

    public void Enqueue<TEvent>(TEvent message)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(message);
        if (db.Model.FindEntityType(typeof(OutboxMessage)) is null)
        {
            throw new InvalidOperationException($"{typeof(TContext).Name} maps no outbox; call modelBuilder.AddNibrasOutbox() in ConfigureModel.");
        }

        var type = message.GetType();
        var descriptor = IntegrationEventDescriptor.For(type);
        var service = options.Value.Service;
        var version = MessagingTopology.SchemaVersion(service, descriptor.RoutingKey);
        var now = clock.GetUtcNow();
        db.Set<OutboxMessage>().Add(new OutboxMessage(
            Guid.CreateVersion7(now),
            now,
            MessagingTopology.Exchange(service),
            descriptor.RoutingKey,
            type.FullName!,
            version,
            descriptor.PartitionKeyOf(message),
            context.CorrelationId ?? RequestCorrelation() ?? Guid.CreateVersion7(now),
            context.CausationId,
            user.UserId,
            now,
            Activity.Current is { IdFormat: ActivityIdFormat.W3C } activity ? activity.Id : null,
            JsonSerializer.SerializeToUtf8Bytes(message, type, Json)));
    }

    private Guid? RequestCorrelation() =>
        services.GetService<IHttpContextAccessor>()?.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] is string text && Guid.TryParse(text, out var id)
            ? id
            : null;
}
