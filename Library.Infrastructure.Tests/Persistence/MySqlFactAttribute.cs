namespace Library.Infrastructure.Tests.Persistence;

public sealed class MySqlFactAttribute : FactAttribute
{
    #region Constants

    private const string ConnectionStringVariable = "MYSQL_TEST_CONNECTION_STRING";

    #endregion

    #region Constructors

    public MySqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            Skip = $"{ConnectionStringVariable} is required for MySQL integration tests. Use a disposable database because tests recreate mapped tables.";
    }

    #endregion

    #region Properties

    public static string? ConnectionString => Environment.GetEnvironmentVariable(ConnectionStringVariable);

    #endregion
}
