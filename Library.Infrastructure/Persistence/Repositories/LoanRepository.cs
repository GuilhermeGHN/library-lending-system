using Library.Application.Interfaces.Repositories;
using Library.Domain.Loans;
using Library.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Repositories;

public sealed class LoanRepository(LibraryDbContext dbContext) : ILoanRepository
{
    #region Public Methods

    public Task<Loan?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Loans.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken)
    {
        await dbContext.Loans.AddAsync(loan, cancellationToken);
    }

    #endregion
}
