using Library.Infrastructure.Interfaces.Projections;

namespace Library.Infrastructure.Projections;

public sealed class ProjectionDispatcher(IEnumerable<IProjectionHandler> handlers) : IProjectionDispatcher
{
    #region Fields

    private readonly IReadOnlyDictionary<string, IProjectionHandler> _handlers = handlers.ToDictionary(x => x.EventType);

    #endregion

    #region Public Methods

    public Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        return _handlers.TryGetValue(eventType, out var handler)
            ? handler.HandleAsync(payload, cancellationToken)
            : throw new InvalidOperationException($"No projection handler is registered for '{eventType}'.");
    }

    #endregion
}
