namespace Library.Application.Models.Results;

/// <summary>
/// Book data returned immediately after a write command commits.
/// </summary>
public sealed record BookResult
{
    #region Constructors

    public BookResult(Guid id, string title, string author, int publicationYear, int availableQuantity, long version)
    {
        Id = id;
        Title = title;
        Author = author;
        PublicationYear = publicationYear;
        AvailableQuantity = availableQuantity;
        Version = version;
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
    /// Number of copies currently available for lending.
    /// </summary>
    public int AvailableQuantity { get; }

    /// <summary>
    /// Optimistic concurrency version committed in MySQL.
    /// </summary>
    public long Version { get; }

    #endregion
}
