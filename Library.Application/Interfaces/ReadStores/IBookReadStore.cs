using Library.Application.Models.Views;

namespace Library.Application.Interfaces.ReadStores;

public interface IBookReadStore
{
    #region Methods

    Task<BookView?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookView>> ListAsync(CancellationToken cancellationToken);

    #endregion
}
