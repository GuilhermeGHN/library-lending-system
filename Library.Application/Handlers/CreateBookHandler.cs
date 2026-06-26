using Library.Application.Commands;
using Library.Application.Common.Results;
using Library.Application.Events;
using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.Repositories;
using Library.Application.Models.Results;
using Library.Application.Validators;
using Library.Domain.Books;
using Microsoft.Extensions.Logging;

namespace Library.Application.Handlers;

public sealed class CreateBookHandler(IBookRepository books,
    IOutbox outbox, 
    IUnitOfWork unitOfWork, 
    ILogger<CreateBookHandler> logger)
{
    #region Public Methods

    public async Task<Result<BookResult>> HandleAsync(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var validation = CreateBookCommandValidator.Validate(command);

        if (!validation.Success)
            return Result<BookResult>.Fail(validation.Error!);

        var book = CreateNewBook(command);

        logger.LogInformation("Creating book {BookId}", book.Id);

        await books.AddAsync(book, cancellationToken);
        await outbox.AddAsync(CreatedEvent(book), cancellationToken);
        var commit = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!commit.Success)
            return Result<BookResult>.Fail(commit.Error!);

        logger.LogInformation("Created book {BookId} at version {Version}", book.Id, book.Version);
        return Result<BookResult>.Ok(new(book.Id, book.Title, book.Author, book.PublicationYear, book.AvailableQuantity, book.Version));
    }

    #endregion

    #region Private Methods

    private BookCreatedEvent CreatedEvent(Book book) => new(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        book.Id,
        book.Title,
        book.Author,
        book.PublicationYear,
        book.AvailableQuantity,
        book.Version
    );

    private static Book CreateNewBook(CreateBookCommand command)
    {
        return Book.Create(
            Guid.NewGuid(),
            command.Title,
            command.Author,
            command.PublicationYear,
            command.AvailableQuantity
        );
    }

    #endregion
}
