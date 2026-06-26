
namespace Library.Api.Requests.Book;

/// <summary>
/// Request body for creating a book.
/// </summary>
/// <param name="Title">Book title.</param>
/// <param name="Author">Book author.</param>
/// <param name="PublicationYear">Original publication year.</param>
/// <param name="AvailableQuantity">Number of copies available for lending.</param>
public sealed record CreateBookRequest(string Title, string Author, int PublicationYear, int AvailableQuantity);
