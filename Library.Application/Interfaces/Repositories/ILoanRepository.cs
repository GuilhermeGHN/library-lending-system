using Library.Domain.Loans;

namespace Library.Application.Interfaces.Repositories;

public interface ILoanRepository
{
    #region Methods

    Task<Loan?> GetAsync(Guid id, CancellationToken cancellationToken);
    
    Task AddAsync(Loan loan, CancellationToken cancellationToken);

    #endregion
}
