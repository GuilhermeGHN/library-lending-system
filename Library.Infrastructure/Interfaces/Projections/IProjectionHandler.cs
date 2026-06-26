namespace Library.Infrastructure.Interfaces.Projections;

public interface IProjectionHandler
{
    #region Properties

    string EventType { get; }

    #endregion

    #region Methods

    Task HandleAsync(string payload, CancellationToken cancellationToken);

    #endregion
}
