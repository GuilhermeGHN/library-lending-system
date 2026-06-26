using Library.Application.Models.Views;

namespace Library.Application.Interfaces.ReadStores;

public interface ILoanReadStore
{
    #region Methods

    Task<LoanView?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<LoanView>> ListAsync(CancellationToken cancellationToken);

    #endregion
}
