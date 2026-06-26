using Library.Application.Interfaces.ReadStores;
using Library.Application.Models.Views;
using Library.Infrastructure.Projections.Documents;
using MongoDB.Driver;

namespace Library.Infrastructure.Projections.ReadStores;

public sealed class MongoLoanReadStore(IMongoDatabase database) : ILoanReadStore
{
    #region Fields
    
    private readonly IMongoCollection<LoanDocument> _loans = database.GetCollection<LoanDocument>("loan");

    #endregion

    #region Public Methods

    public async Task<LoanView?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _loans.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

        return Map(loan);
    }

    public async Task<IReadOnlyList<LoanView>> ListAsync(CancellationToken cancellationToken)
    {
        var loans = await _loans.Find(FilterDefinition<LoanDocument>.Empty)
            .SortByDescending(x => x.LoanDate)
            .ToListAsync(cancellationToken);

        return loans.Select(Map).ToArray()!;
    }

    #endregion

    #region Private Methods

    private static LoanView? Map(LoanDocument? x)
    {
        return x is null
            ? null
            : new
            (
                x.Id,
                x.BookId,
                x.BookTitle,
                x.BookAuthor,
                new DateTimeOffset(DateTime.SpecifyKind(x.LoanDate, DateTimeKind.Utc)),
                x.ReturnDate is null
                    ? null
                    : new DateTimeOffset(DateTime.SpecifyKind(x.ReturnDate.Value, DateTimeKind.Utc)),
                x.Status,
                x.Version,
                new DateTimeOffset(DateTime.SpecifyKind(x.UpdatedAt, DateTimeKind.Utc)),
                x.LastEventId
            );
    }

    #endregion
}
