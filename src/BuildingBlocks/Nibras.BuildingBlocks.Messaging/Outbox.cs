using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Nibras.BuildingBlocks.Persistence;

namespace Nibras.BuildingBlocks.Messaging;

/// <summary>
/// One message to publish, written in the same transaction as the state change that caused it (document 11 part
/// 8.1). EF Core maps the columns written at insert time; <c>sequence</c> and <c>transaction_id</c> take their database
/// defaults, and only the dispatcher writes the dispatch columns.
/// </summary>
internal sealed class OutboxMessage : IInfrastructureRow
{
    public OutboxMessage(
        Guid id,
        DateTimeOffset createdAt,
        string exchange,
        string routingKey,
        string messageType,
        int schemaVersion,
        string? partitionKey,
        Guid correlationId,
        Guid? causationId,
        Guid? userId,
        DateTimeOffset occurredAt,
        string? traceparent,
        byte[] body)
    {
        Id = id;
        CreatedAt = createdAt;
        Exchange = exchange;
        RoutingKey = routingKey;
        MessageType = messageType;
        SchemaVersion = schemaVersion;
        PartitionKey = partitionKey;
        CorrelationId = correlationId;
        CausationId = causationId;
        UserId = userId;
        OccurredAt = occurredAt;
        Traceparent = traceparent;
        Body = body;
    }

    public Guid TenantId { get; private set; }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string Exchange { get; private set; }

    public string RoutingKey { get; private set; }

    public string MessageType { get; private set; }

    public int SchemaVersion { get; private set; }

    public string? PartitionKey { get; private set; }

    public Guid CorrelationId { get; private set; }

    public Guid? CausationId { get; private set; }

    public Guid? UserId { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string? Traceparent { get; private set; }

    public byte[] Body { get; private set; }
}

public static class OutboxModel
{
    public const string Table = "outbox_messages";

    /// <summary>
    /// Maps the outbox into a service's model; call it from <c>ConfigureModel</c>. The table itself is created by
    /// <see cref="OutboxSchema.CreateSql"/>, because EF Core cannot express its partitioning, sequence and trigger.
    /// </summary>
    public static ModelBuilder AddNibrasOutbox(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable(Table, table => table.ExcludeFromMigrations());
            outbox.HasKey(m => new { m.TenantId, m.CreatedAt, m.Id });
            outbox.Property(m => m.Id).ValueGeneratedNever();
        });
        return modelBuilder;
    }
}

/// <summary>The DDL of the outbox in a service's schema, run by the migration role, which owns it (document 11 part 8.1).</summary>
public static class OutboxSchema
{
    /// <summary>The <c>LISTEN</c> channel a commit that wrote outbox rows notifies, with the tenant as the payload.</summary>
    public const string NotifyChannel = "nibras_outbox";

    /// <summary>
    /// The table partitioned by day on <c>created_at</c>, its dispatch sequence, the transaction horizon, the pending
    /// index, a default partition so that a day without its partition never fails a business write, the row-level
    /// security policy, and the trigger that notifies the dispatcher on commit.
    /// </summary>
    public static string CreateSql(string schema)
    {
        var s = RowLevelSecurity.Identifier(schema);
        return string.Create(CultureInfo.InvariantCulture, $$"""
            CREATE SEQUENCE {{s}}.outbox_messages_sequence;
            CREATE TABLE {{s}}.outbox_messages (
                tenant_id         uuid        NOT NULL,
                id                uuid        NOT NULL,
                created_at        timestamptz NOT NULL,
                sequence          bigint      NOT NULL DEFAULT nextval('{{s}}.outbox_messages_sequence'),
                transaction_id    xid8        NOT NULL DEFAULT pg_current_xact_id(),
                exchange          text        NOT NULL,
                routing_key       text        NOT NULL,
                message_type      text        NOT NULL,
                schema_version    integer     NOT NULL,
                partition_key     text        NULL,
                correlation_id    uuid        NOT NULL,
                causation_id      uuid        NULL,
                user_id           uuid        NULL,
                occurred_at       timestamptz NOT NULL,
                traceparent       text        NULL,
                lane              text        NULL,
                channel           text        NULL,
                body              bytea       NOT NULL,
                dispatched_at     timestamptz NULL,
                dispatch_attempts integer     NOT NULL DEFAULT 0,
                last_error        text        NULL,
                PRIMARY KEY (tenant_id, created_at, id)
            ) PARTITION BY RANGE (created_at);
            CREATE INDEX ix_outbox_messages_pending ON {{s}}.outbox_messages (tenant_id, sequence) WHERE dispatched_at IS NULL;
            CREATE TABLE {{s}}.outbox_messages_default PARTITION OF {{s}}.outbox_messages DEFAULT;
            CREATE FUNCTION {{s}}.outbox_notify() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                PERFORM pg_notify('{{NotifyChannel}}', NEW.tenant_id::text);
                RETURN NULL;
            END $$;
            CREATE TRIGGER outbox_notify AFTER INSERT ON {{s}}.outbox_messages FOR EACH ROW EXECUTE FUNCTION {{s}}.outbox_notify();
            {{RowLevelSecurity.PolicySql(s, OutboxModel.Table)}}
            """);
    }

    /// <summary>The partition of one UTC day. Created ahead of the day; PartitionMaintenanceJob (SL-DATA-006) takes this over.</summary>
    public static string PartitionSql(string schema, DateOnly day)
    {
        var s = RowLevelSecurity.Identifier(schema);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"CREATE TABLE IF NOT EXISTS {s}.outbox_messages_{day:yyyyMMdd} PARTITION OF {s}.outbox_messages FOR VALUES FROM ('{day:yyyy-MM-dd} 00:00:00+00') TO ('{day.AddDays(1):yyyy-MM-dd} 00:00:00+00');");
    }
}
