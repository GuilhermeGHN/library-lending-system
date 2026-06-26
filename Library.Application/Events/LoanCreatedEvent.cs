using Library.Domain.Loans;
using Library.Application.Interfaces.Events;

namespace Library.Application.Events;

public sealed record LoanCreatedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid LoanId, Guid BookId, string BookTitle, string BookAuthor, int BookPublicationYear, int AvailableQuantity, long BookVersion, DateTimeOffset LoanDate, LoanStatus Status, long AggregateVersion) : IDomainEvent
{
    #region Properties

    public string EventType => nameof(LoanCreatedEvent);

    public Guid AggregateId => LoanId;

    #endregion
}
