using Library.Application.Common.Results;
using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;

namespace Library.Application.Handlers;

public sealed class GetLoanHandler(ILoanReadStore store)
{
    #region Public Methods

    public async Task<Result<LoanView>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await store.GetAsync(id, cancellationToken);

        if (loan is null)
            return Result<LoanView>.Fail(Error.NotFound("loan", $"Loan '{id}' was not found."));

        return Result<LoanView>.Ok(loan);
    }

    #endregion
}
