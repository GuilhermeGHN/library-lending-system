using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;
using Library.Infrastructure.Projections.Documents;
using MongoDB.Driver;

namespace Library.Infrastructure.Projections.ReadStores;

public sealed class MongoBookReadStore(IMongoDatabase database) : IBookReadStore
{
    #region Fields

    private readonly IMongoCollection<BookDocument> _books = database.GetCollection<BookDocument>("book");

    #endregion

    #region Public Methods

    public async Task<BookView?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await _books.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return Map(book);
    }

    public async Task<IReadOnlyList<BookView>> ListAsync(CancellationToken cancellationToken)
    {
        var books = await _books.Find(FilterDefinition<BookDocument>.Empty)
            .SortBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return books.Select(Map).ToArray()!;
    }

    #endregion

    #region Private Methods

    private static BookView? Map(BookDocument? x)
    {
        return x is null 
            ? null
            : new
            (
                x.Id,
                x.Title,
                x.Author,
                x.PublicationYear,
                x.AvailableQuantity,
                x.Version,
                new DateTimeOffset(DateTime.SpecifyKind(x.UpdatedAt, DateTimeKind.Utc)),
                x.LastEventId
            );
    }

    #endregion
}
