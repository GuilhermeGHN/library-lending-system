using Library.Infrastructure.Projections.Documents;

namespace Library.Infrastructure.Interfaces.Projections;

public interface IBookProjectionWriter
{
    #region Methods

    Task ReplaceIfNewerAsync(BookDocument document, CancellationToken cancellationToken);

    #endregion
}
