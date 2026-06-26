using Library.Application.Interfaces.Repositories;
using Library.Domain.Books;
using Library.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Repositories;

public sealed class BookRepository(LibraryDbContext dbContext) : IBookRepository
{
    #region Public Methods

    public Task<Book?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Books.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        await dbContext.Books.AddAsync(book, cancellationToken);
    }

    #endregion
}
