using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Projections.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Library.Infrastructure.Projections.Writers;

public sealed class MongoBookProjectionWriter(IMongoDatabase database, ILogger<MongoBookProjectionWriter> logger) : IBookProjectionWriter
{
    #region Fields

    private readonly IMongoCollection<BookDocument> _books = database.GetCollection<BookDocument>("book");

    #endregion

    #region Public Methods

    public async Task ReplaceIfNewerAsync(BookDocument document, CancellationToken cancellationToken)
    {
        var idFilter = Builders<BookDocument>.Filter.Eq("_id", document.Id.ToString());

        var versionFilter = Builders<BookDocument>.Filter.Or(
            Builders<BookDocument>.Filter.Exists(x => x.Version, false),
            Builders<BookDocument>.Filter.Lt(x => x.Version, document.Version));

        try
        {
            var result = await _books.ReplaceOneAsync(
                Builders<BookDocument>.Filter.And(idFilter, versionFilter),
                document,
                new ReplaceOptions { IsUpsert = true },
                cancellationToken);

            if (result.MatchedCount == 0 && result.UpsertedId is null)
                logger.LogInformation("Ignored duplicate or stale book projection event for aggregate {AggregateId} at version {Version}", document.Id, document.Version);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogInformation("Ignored duplicate or stale book projection event for aggregate {AggregateId} at version {Version}", document.Id, document.Version);
        }
    }

    #endregion
}
