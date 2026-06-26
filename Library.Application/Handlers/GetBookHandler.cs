using Library.Application.Common.Results;
using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;

namespace Library.Application.Handlers;

public sealed class GetBookHandler(IBookReadStore store)
{
    #region Public Methods

    public async Task<Result<BookView>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await store.GetAsync(id, cancellationToken);

        if (book is null)
            return Result<BookView>.Fail(Error.NotFound("book", $"Book '{id}' was not found."));

        return Result<BookView>.Ok(book);
    }

    #endregion
}
