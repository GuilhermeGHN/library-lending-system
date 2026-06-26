using Library.Infrastructure;

namespace Library.Worker;

public class Program
{
    #region Public Methods

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLibraryInfrastructure(builder.Configuration);
        builder.Services.AddHostedService<OutboxWorker>();

        builder.Build().Run();
    }

    #endregion
}
