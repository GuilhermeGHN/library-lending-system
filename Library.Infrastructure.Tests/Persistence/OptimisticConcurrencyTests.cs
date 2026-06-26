using Library.Application.Events;
using Library.Application.Interfaces.Persistence;
using Library.Domain.Books;
using Library.Domain.Loans;
using Library.Infrastructure.Persistence.Contexts;
using Library.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Tests.Persistence;

public sealed class OptimisticConcurrencyTests
{
    #region Tests

    [MySqlFact]
    public async Task ShouldRollbackLoanAndOutboxWhenConcurrentBorrowLoses()
    {
        var connectionString = MySqlFactAttribute.ConnectionString!;

        await using (var setupContext = CreateContext(connectionString))
        {
            await RecreateSchemaAsync(setupContext);

            setupContext.Books.Add(Book.Create(Guid.NewGuid(), "Domain-Driven Design", "Eric Evans", 2003, 1));
            await setupContext.SaveChangesAsync();
        }

        await using var firstContext = CreateContext(connectionString);
        await using var secondContext = CreateContext(connectionString);

        var firstBook = await firstContext.Books.SingleAsync();
        var secondBook = await secondContext.Books.SingleAsync();
        var firstLoan = Loan.Create(Guid.NewGuid(), firstBook.Id, DateTimeOffset.UtcNow);
        var secondLoan = Loan.Create(Guid.NewGuid(), secondBook.Id, DateTimeOffset.UtcNow);

        firstBook.TakeAvailableCopy();
        secondBook.TakeAvailableCopy();

        firstContext.Loans.Add(firstLoan);
        secondContext.Loans.Add(secondLoan);
        firstContext.OutboxMessages.Add(OutboxMessage.From(CreatedEvent(firstBook, firstLoan)));
        secondContext.OutboxMessages.Add(OutboxMessage.From(CreatedEvent(secondBook, secondLoan)));

        var firstCommit = await ((IUnitOfWork)firstContext).SaveChangesAsync(CancellationToken.None);
        var secondCommit = await ((IUnitOfWork)secondContext).SaveChangesAsync(CancellationToken.None);

        Assert.True(firstCommit.Success);
        Assert.False(secondCommit.Success);
        Assert.Equal("resource.concurrency.conflict", secondCommit.Error?.Code);

        await using var verificationContext = CreateContext(connectionString);
        var book = await verificationContext.Books.AsNoTracking().SingleAsync();

        Assert.Equal(0, book.AvailableQuantity);
        Assert.Equal(2, book.Version);
        Assert.Equal(1, await verificationContext.Loans.CountAsync());
        Assert.Equal(1, await verificationContext.OutboxMessages.CountAsync());
    }

    #endregion

    #region Private Methods

    private static LibraryDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseMySQL(NormalizeConnectionString(connectionString))
            .Options;

        return new LibraryDbContext(options);
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        var normalized = connectionString.TrimEnd(';');

        if (!connectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("ssl mode", StringComparison.OrdinalIgnoreCase))
            normalized = $"{normalized};sslmode=Disabled";

        if (!connectionString.Contains("AllowPublicKeyRetrieval", StringComparison.OrdinalIgnoreCase))
            normalized = $"{normalized};AllowPublicKeyRetrieval=True";

        return normalized;
    }

    private static async Task RecreateSchemaAsync(LibraryDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS loan;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS outbox_message;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS book;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS loans;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS outbox_messages;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS books;");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS `__EFMigrationsHistory`;");
        await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1;");

        await context.Database.MigrateAsync();
    }

    private static LoanCreatedEvent CreatedEvent(Book book, Loan loan)
    {
        return new LoanCreatedEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            loan.Id,
            book.Id,
            book.Title,
            book.Author,
            book.PublicationYear,
            book.AvailableQuantity,
            book.Version,
            loan.LoanDate,
            loan.Status,
            loan.Version);
    }

    #endregion
}
