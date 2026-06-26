using Library.Domain.Exceptions;

namespace Library.Domain.Books;

public sealed class Book
{
    #region Constructors

    private Book() { }

    private Book(Guid id, string title, string author, int publicationYear, int availableQuantity)
    {
        if (id == Guid.Empty)
            throw new DomainValidationException("Book id is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Book title is required.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainValidationException("Book author is required.");

        if (availableQuantity < 0)
            throw new DomainValidationException("Available quantity cannot be negative.");

        Id = id;
        Title = title.Trim();
        Author = author.Trim();
        PublicationYear = publicationYear;
        AvailableQuantity = availableQuantity;
        Version = 1;
    }

    #endregion

    #region Properties

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    public int PublicationYear { get; private set; }

    public int AvailableQuantity { get; private set; }

    public long Version { get; private set; }

    #endregion

    #region Public Methods

    public static Book Create(Guid id, string title, string author, int publicationYear, int availableQuantity) => new(
        id,
        title,
        author,
        publicationYear,
        availableQuantity);

    public void TakeAvailableCopy()
    {
        if (AvailableQuantity == 0) throw new BookUnavailableException(Id);
        AvailableQuantity--;
        Version++;
    }

    public void ReturnCopy()
    {
        AvailableQuantity++;
        Version++;
    }

    #endregion
}
