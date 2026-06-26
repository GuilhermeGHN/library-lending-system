using Library.Application.Common.Results;
using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;

namespace Library.Application.Handlers;

public sealed class ListBooksHandler(IBookReadStore store)
{
    #region Public Methods

    public async Task<Result<IReadOnlyList<BookView>>> HandleAsync(CancellationToken cancellationToken)
    {
        var books = await store.ListAsync(cancellationToken);

        return Result<IReadOnlyList<BookView>>.Ok(books);
    }

    #endregion
}
