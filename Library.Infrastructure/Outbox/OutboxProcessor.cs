using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure.Outbox;

public sealed class OutboxProcessor(LibraryDbContext dbContext, IProjectionDispatcher dispatcher, IOptions<OutboxOptions> options, ILogger<OutboxProcessor> logger)
{
    #region Public Methods

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var retryBefore = DateTimeOffset.UtcNow.AddSeconds(-settings.RetryDelaySeconds);

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.Attempts < settings.MaxAttempts && (x.LastAttemptAt == null || x.LastAttemptAt <= retryBefore))
            .OrderBy(x => x.OccurredAt).Take(settings.BatchSize).ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                logger.LogInformation("Processing outbox event {EventId} for aggregate {AggregateId}", message.Id, message.AggregateId);
                
                await dispatcher.DispatchAsync(message.EventType, message.Payload, cancellationToken);
                message.MarkProcessed(DateTimeOffset.UtcNow);
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception exception)
            {
                message.MarkFailed(DateTimeOffset.UtcNow, exception.Message);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogError(exception, "Outbox event {EventId} for aggregate {AggregateId} failed on attempt {Attempt}", message.Id, message.AggregateId, message.Attempts);
                if (message.Attempts >= settings.MaxAttempts) logger.LogCritical("Outbox event {EventId} reached the maximum attempt count", message.Id);
            }
        }
        return messages.Count;
    }

    #endregion
}
