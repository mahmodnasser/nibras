namespace Nibras.Contracts.Shared;

/// <summary>
/// Declares a contract record as an integration event with its Appendix E routing key,
/// <c>&lt;service&gt;.&lt;entity&gt;.&lt;event&gt;.v&lt;n&gt;</c> (REQ-MSG-005). It lives here rather than in the messaging
/// block because a contract project references this project and nothing else (document 07 part 10.2).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationEventAttribute(string routingKey) : Attribute
{
    public string RoutingKey { get; } = routingKey;
}

/// <summary>Marks the property whose value is the envelope's <c>partitionKey</c>: messages with one key keep their order.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class PartitionKeyAttribute : Attribute;
