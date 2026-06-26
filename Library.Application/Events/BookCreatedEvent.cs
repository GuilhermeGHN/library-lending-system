using Library.Application.Interfaces.Events;

namespace Library.Application.Events;

public sealed record BookCreatedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid BookId, string Title, string Author, int PublicationYear, int AvailableQuantity, long AggregateVersion) : IDomainEvent
{
    #region Properties

    public string EventType => nameof(BookCreatedEvent);

    public Guid AggregateId => BookId;

    #endregion
}
