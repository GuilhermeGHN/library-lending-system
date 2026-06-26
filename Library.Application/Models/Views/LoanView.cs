using Library.Domain.Loans;

namespace Library.Application.Models.Views;

/// <summary>
/// Loan projection returned by public read endpoints.
/// </summary>
public sealed record LoanView
{
    #region Constructors

    public LoanView(Guid id, Guid bookId, string bookTitle, string bookAuthor, DateTimeOffset loanDate, DateTimeOffset? returnDate, LoanStatus status, long version, DateTimeOffset updatedAt, Guid lastEventId)
    {
        Id = id;
        BookId = bookId;
        BookTitle = bookTitle;
        BookAuthor = bookAuthor;
        LoanDate = loanDate;
        ReturnDate = returnDate;
        Status = status;
        Version = version;
        UpdatedAt = updatedAt;
        LastEventId = lastEventId;
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
    /// Title of the borrowed book at projection time.
    /// </summary>
    public string BookTitle { get; }

    /// <summary>
    /// Author of the borrowed book at projection time.
    /// </summary>
    public string BookAuthor { get; }

    /// <summary>
    /// UTC timestamp when the loan was created.
    /// </summary>
    public DateTimeOffset LoanDate { get; }

    /// <summary>
    /// UTC timestamp when the loan was returned, when applicable.
    /// </summary>
    public DateTimeOffset? ReturnDate { get; }

    /// <summary>
    /// Current projected loan status.
    /// </summary>
    public LoanStatus Status { get; }

    /// <summary>
    /// Latest aggregate version applied to the MongoDB projection.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// UTC timestamp of the last event applied to this projection.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; }

    /// <summary>
    /// Identifier of the last event applied to this projection.
    /// </summary>
    public Guid LastEventId { get; }

    #endregion
}
