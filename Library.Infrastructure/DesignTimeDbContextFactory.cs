using Library.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library.Infrastructure;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    #region Public Methods

    public LibraryDbContext CreateDbContext(string[] args)
    {
        var connectionString = ReadConnectionString(args)
            ?? "server=localhost;port=3306;database=library;user=library;password=library";
            
        var options = new DbContextOptionsBuilder<LibraryDbContext>().UseMySQL(connectionString).Options;
        return new LibraryDbContext(options);
    }

    #endregion

    #region Private Methods

    private static string? ReadConnectionString(string[] args)
    {
        var connectionArgument = args
            .SkipWhile(x => x != "--connection")
            .Skip(1)
            .FirstOrDefault();

        return connectionArgument
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__MySql");
    }

    #endregion
}
