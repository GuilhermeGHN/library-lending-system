namespace Library.Infrastructure.Tests.Projections;

public sealed class MongoFactAttribute : FactAttribute
{
    #region Constants

    private const string ConnectionStringVariable = "MONGO_TEST_CONNECTION_STRING";

    #endregion

    #region Constructors

    public MongoFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            Skip = $"{ConnectionStringVariable} is required for MongoDB projection tests. Use a disposable database because tests drop it.";
    }

    #endregion

    #region Properties

    public static string? ConnectionString => Environment.GetEnvironmentVariable(ConnectionStringVariable);

    public static string DatabaseName => Environment.GetEnvironmentVariable("MONGO_TEST_DATABASE") ?? "library_projection_test";

    #endregion
}
