using Library.Domain.Exceptions;

namespace Library.Domain.Loans;

public sealed class Loan
{
    #region Constructors

    private Loan() { }

    private Loan(Guid id, Guid bookId, DateTimeOffset loanDate)
    {
        if (id == Guid.Empty)
            throw new DomainValidationException("Loan id is required.");
        
        if (bookId == Guid.Empty)
            throw new DomainValidationException("Book id is required.");

        Id = id;
        BookId = bookId;
        LoanDate = loanDate.ToUniversalTime();
        Status = LoanStatus.Active;
        Version = 1;
    }

    #endregion

    #region Properties

    public Guid Id { get; private set; }

    public Guid BookId { get; private set; }

    public DateTimeOffset LoanDate { get; private set; }

    public DateTimeOffset? ReturnDate { get; private set; }

    public LoanStatus Status { get; private set; }

    public long Version { get; private set; }

    #endregion

    #region Public Methods

    public static Loan Create(Guid id, Guid bookId, DateTimeOffset loanDate) => new(
        id,
        bookId,
        loanDate);

    public void Return(DateTimeOffset returnedAt)
    {
        if (Status == LoanStatus.Returned)
            throw new LoanAlreadyReturnedException(Id);
        
        var utcReturnedAt = returnedAt.ToUniversalTime();
        
        if (utcReturnedAt < LoanDate)
            throw new DomainValidationException("Return date cannot precede loan date.");

        ReturnDate = utcReturnedAt;
        Status = LoanStatus.Returned;
        Version++;
    }

    #endregion
}
