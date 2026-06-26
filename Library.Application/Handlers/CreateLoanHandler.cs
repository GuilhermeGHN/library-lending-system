using Library.Application.Commands;
using Library.Application.Common.Results;
using Library.Application.Events;
using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.Repositories;
using Library.Application.Models.Results;
using Library.Domain.Loans;
using Microsoft.Extensions.Logging;

namespace Library.Application.Handlers;

public sealed class CreateLoanHandler(IBookRepository books,
    ILoanRepository loans,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    ILogger<CreateLoanHandler> logger)
{
    #region Public Methods

    public async Task<Result<LoanResult>> HandleAsync(CreateLoanCommand command, CancellationToken cancellationToken)
    {
        if (command.BookId == Guid.Empty)
            return Result<LoanResult>.Fail(Error.Validation(new Dictionary<string, string[]> { ["bookId"] = ["Book id is required."] }));

        var book = await books.GetAsync(command.BookId, cancellationToken);

        if (book is null)
            return Result<LoanResult>.Fail(Error.NotFound("book", $"Book '{command.BookId}' was not found."));

        if (book.AvailableQuantity == 0)
            return Result<LoanResult>.Fail(Error.Conflict("book.unavailable", $"Book '{book.Id}' has no available copies."));

        logger.LogInformation("Creating loan for book {BookId} at version {Version}", book.Id, book.Version);
        book.TakeAvailableCopy();

        var loan = Loan.Create(Guid.NewGuid(), book.Id, DateTimeOffset.UtcNow);

        await loans.AddAsync(loan, cancellationToken);
        await outbox.AddAsync(CreatedEvent(book, loan), cancellationToken);
        var commit = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!commit.Success)
            return Result<LoanResult>.Fail(commit.Error!);

        logger.LogInformation("Created loan {LoanId} for book {BookId}", loan.Id, book.Id);
        return Result<LoanResult>.Ok(new(loan.Id, loan.BookId, loan.LoanDate, loan.ReturnDate, loan.Status, loan.Version));
    }

    #endregion

    #region Private Methods

    private LoanCreatedEvent CreatedEvent(Domain.Books.Book book, Loan loan) => new(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        loan.Id,
        book.Id,
        book.Title,
        book.Author,
        book.PublicationYear,
        book.AvailableQuantity,
        book.Version,
        loan.LoanDate,
        loan.Status,
        loan.Version
    );

    #endregion
}
