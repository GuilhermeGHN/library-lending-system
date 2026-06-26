using Library.Infrastructure.Outbox;
using Microsoft.Extensions.Options;

namespace Library.Worker;

public sealed class OutboxWorker(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options, ILogger<OutboxWorker> logger) : BackgroundService
{
    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var count = await scope.ServiceProvider.GetRequiredService<OutboxProcessor>()
                    .ProcessBatchAsync(stoppingToken);

                if (count == 0) 
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox polling failed; processing will retry");
                
                await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds), stoppingToken);
            }
        }
    }

    #endregion
}
