namespace Library.Application.Models.Views;

/// <summary>
/// Book projection returned by public read endpoints.
/// </summary>
public sealed record BookView
{
    #region Constructors

    public BookView(Guid id, string title, string author, int publicationYear, int availableQuantity, long version, DateTimeOffset updatedAt, Guid lastEventId)
    {
        Id = id;
        Title = title;
        Author = author;
        PublicationYear = publicationYear;
        AvailableQuantity = availableQuantity;
        Version = version;
        UpdatedAt = updatedAt;
        LastEventId = lastEventId;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Book identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Book title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Book author.
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// Original publication year.
    /// </summary>
    public int PublicationYear { get; }

    /// <summary>
    /// Number of copies currently projected as available.
    /// </summary>
    public int AvailableQuantity { get; }

    /// <summary>
    /// Latest aggregate version applied to the MongoDB projection.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// UTC timestamp of the last event applied to this projection.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; }

    /// <summary>
    /// Identifier of the last event applied to this projection.
    /// </summary>
    public Guid LastEventId { get; }

    #endregion
}
