namespace Library.Application.Interfaces.Events;

public interface IDomainEvent
{
    #region Properties

    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }

    string EventType { get; }

    Guid AggregateId { get; }

    long AggregateVersion { get; }

    #endregion
}
