using Library.Application.Interfaces.Events;
using System.Text.Json;

namespace Library.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    #region Constructors

    private OutboxMessage() { }

    #endregion

    #region Properties

    public Guid Id { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public Guid AggregateId { get; private set; }

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset? LastAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    #endregion

    #region Public Methods

    public static OutboxMessage From(IDomainEvent domainEvent) => new()
    {
        Id = domainEvent.EventId,
        EventType = domainEvent.EventType,
        AggregateId = domainEvent.AggregateId,
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
        OccurredAt = domainEvent.OccurredAt
    };

    public void MarkProcessed(DateTimeOffset at)
    {
        ProcessedAt = at;
        LastAttemptAt = at;
        LastError = null;
    }

    public void MarkFailed(DateTimeOffset at, string error)
    {
        Attempts++;
        LastAttemptAt = at;
        LastError = error.Length <= 2000 ? error : error[..2000];
    }

    #endregion
}
