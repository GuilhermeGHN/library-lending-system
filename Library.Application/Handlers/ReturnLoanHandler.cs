using Library.Application.Commands;
using Library.Application.Common.Results;
using Library.Application.Events;
using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Library.Application.Handlers;

public sealed class ReturnLoanHandler(IBookRepository books,
    ILoanRepository loans,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    ILogger<ReturnLoanHandler> logger)
{
    #region Public Methods

    public async Task<Result> HandleAsync(ReturnLoanCommand command, CancellationToken cancellationToken)
    {
        if (command.LoanId == Guid.Empty)
            return Result.Fail(Error.Validation(new Dictionary<string, string[]> { ["loanId"] = ["Loan id is required."] }));

        var loan = await loans.GetAsync(command.LoanId, cancellationToken);

        if (loan is null)
            return Result.Fail(Error.NotFound("loan", $"Loan '{command.LoanId}' was not found."));

        var book = await books.GetAsync(loan.BookId, cancellationToken);

        if (book is null)
            return Result.Fail(Error.NotFound("book", $"Book '{loan.BookId}' was not found."));

        if (loan.Status == Domain.Loans.LoanStatus.Returned)
            return Result.Fail(Error.Conflict("loan.already_returned", $"Loan '{loan.Id}' has already been returned."));

        logger.LogInformation("Returning loan {LoanId} for book {BookId}", loan.Id, book.Id);
        loan.Return(DateTimeOffset.UtcNow);
        book.ReturnCopy();

        await outbox.AddAsync(CreatedEvent(loan, book), cancellationToken);
        var commit = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!commit.Success)
            return Result.Fail(commit.Error!);

        logger.LogInformation("Returned loan {LoanId}", loan.Id);
        return Result.Ok();
    }

    #endregion

    #region Private Methods

    private LoanReturnedEvent CreatedEvent(Domain.Loans.Loan loan, Domain.Books.Book book) => new(
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
        loan.ReturnDate!.Value,
        loan.Status,
        loan.Version);

    #endregion
}
