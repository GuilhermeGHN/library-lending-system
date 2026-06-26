using Library.Application.Common.Results;

namespace Library.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    #region Methods

    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);

    #endregion
}
