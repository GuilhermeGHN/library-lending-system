using Library.Domain.Loans;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Library.Infrastructure.Projections.Documents;

public sealed class LoanDocument
{
    #region Properties

    [BsonId, BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid BookId { get; set; }

    public string BookTitle { get; set; } = string.Empty;

    public string BookAuthor { get; set; } = string.Empty;

    public DateTime LoanDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [BsonRepresentation(BsonType.String)]
    public LoanStatus Status { get; set; }

    public long Version { get; set; }

    public DateTime UpdatedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid LastEventId { get; set; }

    #endregion
}
