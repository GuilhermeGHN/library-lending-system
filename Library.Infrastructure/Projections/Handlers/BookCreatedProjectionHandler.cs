using Library.Application.Events;
using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Projections.Documents;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Library.Infrastructure.Projections.Handlers;

public sealed class BookCreatedProjectionHandler(IBookProjectionWriter books, ILogger<BookCreatedProjectionHandler> logger) : IProjectionHandler
{
    #region Properties

    public string EventType => nameof(BookCreatedEvent);

    #endregion

    #region Public Methods

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<BookCreatedEvent>(payload)!;

        var document = new BookDocument
        {
            Id = message.BookId,
            Title = message.Title,
            Author = message.Author,
            PublicationYear = message.PublicationYear,
            AvailableQuantity = message.AvailableQuantity,
            Version = message.AggregateVersion,
            UpdatedAt = message.OccurredAt.UtcDateTime,
            LastEventId = message.EventId
        };

        await books.ReplaceIfNewerAsync(document, cancellationToken);
        logger.LogInformation("Projected event {EventId} for book {BookId}", message.EventId, message.BookId);
    }

    #endregion
}
