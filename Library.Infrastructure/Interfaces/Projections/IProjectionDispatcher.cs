namespace Library.Infrastructure.Interfaces.Projections;

public interface IProjectionDispatcher
{
    #region Methods

    Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken);

    #endregion
}
