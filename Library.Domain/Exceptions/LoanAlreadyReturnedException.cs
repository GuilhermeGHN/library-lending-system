namespace Library.Domain.Exceptions;

public sealed class LoanAlreadyReturnedException(Guid loanId) : DomainException($"Loan '{loanId}' has already been returned.");
