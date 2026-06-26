using Library.Domain.Books;

namespace Library.Application.Interfaces.Repositories;

public interface IBookRepository
{
    #region Methods

    Task<Book?> GetAsync(Guid id, CancellationToken cancellationToken);
    
    Task AddAsync(Book book, CancellationToken cancellationToken);

    #endregion
}
