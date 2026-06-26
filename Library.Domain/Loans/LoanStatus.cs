namespace Library.Domain.Loans;

/// <summary>
/// Current lifecycle state of a loan.
/// </summary>
public enum LoanStatus
{
    #region Values

    /// <summary>
    /// The book copy is currently borrowed.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The book copy has been returned.
    /// </summary>
    Returned = 2

    #endregion
}
