using Library.Application.Interfaces.Events;
using Library.Application.Interfaces.Outbox;
using Library.Infrastructure.Persistence.Contexts;

namespace Library.Infrastructure.Persistence.Outbox;

public sealed class EfOutbox(LibraryDbContext dbContext) : IOutbox
{
    #region Public Methods

    public async Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages.AddAsync(OutboxMessage.From(domainEvent), cancellationToken);
    }

    #endregion
}
