using Library.Application.Common.Results;
using Library.Application.Interfaces.Persistence;
using Library.Domain.Books;
using Library.Domain.Loans;
using Library.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence.Contexts;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options, ILogger<LibraryDbContext>? logger = null) : DbContext(options), IUnitOfWork
{
    #region Properties

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    #endregion

    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    }

    #endregion

    #region Explicit Interface Methods

    async Task<Result> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Ok();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            logger?.LogWarning("Optimistic concurrency conflict affecting {EntryCount} tracked entries", exception.Entries.Count);
            return Result.Fail(Error.Conflict("resource.concurrency", "The resource changed while this operation was being completed. Retry with fresh state."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion
}
