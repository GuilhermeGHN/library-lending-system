using Library.Application.Interfaces.Events;

namespace Library.Application.Interfaces.Outbox;

public interface IOutbox
{
    #region Methods

    Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

    #endregion
}
