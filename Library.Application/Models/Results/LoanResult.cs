using Library.Domain.Loans;

namespace Library.Application.Models.Results;

/// <summary>
/// Loan data returned immediately after a write command commits.
/// </summary>
public sealed record LoanResult
{
    #region Constructors

    public LoanResult(Guid id, Guid bookId, DateTimeOffset loanDate, DateTimeOffset? returnDate, LoanStatus status, long version)
    {
        Id = id;
        BookId = bookId;
        LoanDate = loanDate;
        ReturnDate = returnDate;
        Status = status;
        Version = version;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Loan identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Identifier of the borrowed book.
    /// </summary>
    public Guid BookId { get; }

    /// <summary>
    /// UTC timestamp when the loan was created.
    /// </summary>
    public DateTimeOffset LoanDate { get; }

    /// <summary>
    /// UTC timestamp when the loan was returned, when applicable.
    /// </summary>
    public DateTimeOffset? ReturnDate { get; }

    /// <summary>
    /// Current loan status.
    /// </summary>
    public LoanStatus Status { get; }

    /// <summary>
    /// Optimistic concurrency version committed in MySQL.
    /// </summary>
    public long Version { get; }

    #endregion
}
