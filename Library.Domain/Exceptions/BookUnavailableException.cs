namespace Library.Domain.Exceptions;

public sealed class BookUnavailableException(Guid bookId) : DomainException($"Book '{bookId}' has no available copies.");
