using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Library.Infrastructure.Projections.Documents;

public sealed class BookDocument
{
    #region Properties

    [BsonId, BsonRepresentation(BsonType.String)] 
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public int PublicationYear { get; set; }

    public int AvailableQuantity { get; set; }

    public long Version { get; set; }

    public DateTime UpdatedAt { get; set; }

    [BsonRepresentation(BsonType.String)] 
    public Guid LastEventId { get; set; }

    #endregion
}
