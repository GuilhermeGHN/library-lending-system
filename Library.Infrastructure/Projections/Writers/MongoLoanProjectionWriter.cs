using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Projections.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Library.Infrastructure.Projections.Writers;

public sealed class MongoLoanProjectionWriter(IMongoDatabase database, ILogger<MongoLoanProjectionWriter> logger) : ILoanProjectionWriter
{
    #region Fields

    private readonly IMongoCollection<LoanDocument> _loans = database.GetCollection<LoanDocument>("loan");

    #endregion

    #region Public Methods

    public async Task ReplaceIfNewerAsync(LoanDocument document, CancellationToken cancellationToken)
    {
        var idFilter = Builders<LoanDocument>.Filter.Eq("_id", document.Id.ToString());

        var versionFilter = Builders<LoanDocument>.Filter.Or(
            Builders<LoanDocument>.Filter.Exists(x => x.Version, false),
            Builders<LoanDocument>.Filter.Lt(x => x.Version, document.Version));

        try
        {
            var result = await _loans.ReplaceOneAsync(
                Builders<LoanDocument>.Filter.And(idFilter, versionFilter),
                document,
                new ReplaceOptions { IsUpsert = true },
                cancellationToken);

            if (result.MatchedCount == 0 && result.UpsertedId is null)
                logger.LogInformation("Ignored duplicate or stale loan projection event for aggregate {AggregateId} at version {Version}", document.Id, document.Version);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogInformation("Ignored duplicate or stale loan projection event for aggregate {AggregateId} at version {Version}", document.Id, document.Version);
        }
    }

    #endregion
}
