using Library.Application.Common.Results;
using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;

namespace Library.Application.Handlers;

public sealed class ListLoansHandler(ILoanReadStore store)
{
    #region Public Methods

    public async Task<Result<IReadOnlyList<LoanView>>> HandleAsync(CancellationToken cancellationToken)
    {
        var loans = await store.ListAsync(cancellationToken);

        return Result<IReadOnlyList<LoanView>>.Ok(loans);
    }

    #endregion
}
