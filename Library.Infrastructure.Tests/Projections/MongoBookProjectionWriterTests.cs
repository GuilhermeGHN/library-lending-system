using Library.Infrastructure.Projections.Documents;
using Library.Infrastructure.Projections.Writers;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace Library.Infrastructure.Tests.Projections;

public sealed class MongoBookProjectionWriterTests
{
    #region Tests

    [MongoFact]
    public async Task ShouldIgnoreOlderBookProjection()
    {
        var client = new MongoClient(MongoFactAttribute.ConnectionString);
        var database = client.GetDatabase(MongoFactAttribute.DatabaseName);

        await client.DropDatabaseAsync(MongoFactAttribute.DatabaseName);

        var writer = new MongoBookProjectionWriter(database, NullLogger<MongoBookProjectionWriter>.Instance);
        var bookId = Guid.NewGuid();

        await writer.ReplaceIfNewerAsync(Document(bookId, "New title", 2), default);
        await writer.ReplaceIfNewerAsync(Document(bookId, "Old title", 1), default);

        var stored = await database
            .GetCollection<BookDocument>("book")
            .Find(x => x.Id == bookId)
            .SingleAsync();

        Assert.Equal("New title", stored.Title);
        Assert.Equal(2, stored.Version);
    }

    #endregion

    #region Private Methods

    private static BookDocument Document(Guid id, string title, long version)
    {
        return new BookDocument
        {
            Id = id,
            Title = title,
            Author = "Author",
            PublicationYear = 2026,
            AvailableQuantity = 1,
            Version = version,
            UpdatedAt = DateTime.UtcNow,
            LastEventId = Guid.NewGuid()
        };
    }

    #endregion
}
