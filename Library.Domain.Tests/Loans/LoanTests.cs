using Library.Domain.Exceptions;
using Library.Domain.Loans;

namespace Library.Domain.Tests.Loans;

public sealed class LoanTests
{
    #region Fields

    private static readonly DateTimeOffset LoanDate = new(2026, 6, 22, 12, 0, 0, TimeSpan.Zero);

    #endregion

    #region Tests

    [Fact]
    public void ShouldStartActiveWithoutReturnDate()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), LoanDate);
        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Null(loan.ReturnDate);
        Assert.Equal(1, loan.Version);
    }

    [Fact]
    public void ShouldChangeStatusSetUtcDateAndAdvanceVersionWhenReturningLoan()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), LoanDate);
        loan.Return(LoanDate.AddDays(1).ToOffset(TimeSpan.FromHours(-3)));
        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(LoanDate.AddDays(1), loan.ReturnDate);
        Assert.Equal(2, loan.Version);
    }

    [Fact]
    public void ShouldThrowWhenReturningSameLoanTwice()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), LoanDate);
        loan.Return(LoanDate.AddDays(1));
        Assert.Throws<LoanAlreadyReturnedException>(() => loan.Return(LoanDate.AddDays(2)));
    }

    [Fact]
    public void ShouldThrowWhenReturningBeforeLoanDate()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), LoanDate);
        Assert.Throws<DomainValidationException>(() => loan.Return(LoanDate.AddSeconds(-1)));
    }

    #endregion
}
