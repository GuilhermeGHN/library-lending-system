using Library.Application.Events;
using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Projections.Documents;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Library.Infrastructure.Projections.Handlers;

public sealed class LoanCreatedProjectionHandler(IBookProjectionWriter books, ILoanProjectionWriter loans, ILogger<LoanCreatedProjectionHandler> logger) : IProjectionHandler
{
    #region Properties

    public string EventType => nameof(LoanCreatedEvent);

    #endregion

    #region Public Methods

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<LoanCreatedEvent>(payload)!;

        var book = new BookDocument
        {
            Id = message.BookId,
            Title = message.BookTitle,
            Author = message.BookAuthor,
            PublicationYear = message.BookPublicationYear,
            AvailableQuantity = message.AvailableQuantity,
            Version = message.BookVersion,
            UpdatedAt = message.OccurredAt.UtcDateTime,
            LastEventId = message.EventId
        };

        var loan = new LoanDocument
        {
            Id = message.LoanId,
            BookId = message.BookId,
            BookTitle = message.BookTitle,
            BookAuthor = message.BookAuthor,
            LoanDate = message.LoanDate.UtcDateTime,
            Status = message.Status,
            Version = message.AggregateVersion,
            UpdatedAt = message.OccurredAt.UtcDateTime,
            LastEventId = message.EventId
        };

        await books.ReplaceIfNewerAsync(book, cancellationToken);
        await loans.ReplaceIfNewerAsync(loan, cancellationToken);

        logger.LogInformation("Projected event {EventId} for loan {LoanId} and book {BookId}", message.EventId, message.LoanId, message.BookId);
    }

    #endregion
}
