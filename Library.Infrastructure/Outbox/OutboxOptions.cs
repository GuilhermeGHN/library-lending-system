namespace Library.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    #region Constants

    public const string SectionName = "Outbox";

    #endregion

    #region Properties

    public int PollingIntervalSeconds { get; set; } = 2;

    public int BatchSize { get; set; } = 50;

    public int MaxAttempts { get; set; } = 10;

    public int RetryDelaySeconds { get; set; } = 5;

    #endregion
}
