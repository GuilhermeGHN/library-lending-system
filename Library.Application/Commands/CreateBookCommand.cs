namespace Library.Application.Commands;

public sealed record CreateBookCommand(string Title, string Author, int PublicationYear, int AvailableQuantity);