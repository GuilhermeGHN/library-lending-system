using Library.Infrastructure.Projections.Documents;

namespace Library.Infrastructure.Interfaces.Projections;

public interface ILoanProjectionWriter
{
    #region Methods

    Task ReplaceIfNewerAsync(LoanDocument document, CancellationToken cancellationToken);

    #endregion
}
