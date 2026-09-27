namespace Nibras.Contracts.Shared;

/// <summary>
/// The metadata every integration message carries (document 07 part 8, document 11). The payload is a
/// versioned record from the publishing service's contract project.
/// </summary>
/// <param name="MessageId">Unique per message; the inbox deduplicates on it.</param>
/// <param name="CorrelationId">The request or process the message belongs to.</param>
/// <param name="CausationId">The message that caused this one, if any.</param>
/// <param name="TenantId">The tenant the message belongs to; platform-scoped messages carry none.</param>
/// <param name="OccurredAt">When the fact happened, in UTC.</param>
/// <param name="SchemaVersion">The contract version, for example 1 for <c>V1</c>.</param>
/// <param name="PartitionKey">The ordering key; messages with the same key are processed in order.</param>
public sealed record MessageEnvelope(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    int SchemaVersion,
    string PartitionKey);
