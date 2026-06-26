using Library.Application.Common.Results;
using Library.Application.Events;
using Library.Application.Handlers;
using Library.Application.Interfaces.Events;
using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.Repositories;
using Library.Domain.Books;
using Library.Domain.Loans;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Library.Application.Tests.Handlers;

public sealed class ReturnLoanHandlerTests
{
    #region Fields

    private readonly Mock<IBookRepository> _books = new();

    private readonly Mock<ILoanRepository> _loans = new();

    private readonly Mock<IOutbox> _outbox = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly List<IDomainEvent> _outboxEvents = [];

    private readonly ReturnLoanHandler _handler;

    #endregion

    #region Constructors

    public ReturnLoanHandlerTests()
    {
        _outbox.Setup(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((domainEvent, _) => _outboxEvents.Add(domainEvent))
            .Returns(Task.CompletedTask);

        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new ReturnLoanHandler(_books.Object, _loans.Object, _outbox.Object, _unitOfWork.Object, NullLogger<ReturnLoanHandler>.Instance);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task ShouldReturnLoanRestoreBookAvailabilityRecordEventAndCommit()
    {
        var book = Book.Create(Guid.NewGuid(), "DDD", "Eric Evans", 2003, 1);
        var loan = Loan.Create(Guid.NewGuid(), book.Id, DateTimeOffset.UtcNow.AddDays(-1));

        _loans.Setup(x => x.GetAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
        _books.Setup(x => x.GetAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _handler.HandleAsync(new(loan.Id), default);

        Assert.True(result.Success);
        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(2, book.AvailableQuantity);
        Assert.IsType<LoanReturnedEvent>(Assert.Single(_outboxEvents));
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldNotCommitWhenLoanIsMissing()
    {
        var loanId = Guid.NewGuid();

        _loans.Setup(x => x.GetAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var result = await _handler.HandleAsync(new(loanId), default);

        Assert.False(result.Success);
        Assert.Equal("loan.not_found", result.Error!.Code);
        _books.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotCommitWhenBookIsMissing()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1));

        _loans.Setup(x => x.GetAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
        _books.Setup(x => x.GetAsync(loan.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var result = await _handler.HandleAsync(new(loan.Id), default);

        Assert.False(result.Success);
        Assert.Equal("book.not_found", result.Error!.Code);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotCommitWhenLoanIsAlreadyReturned()
    {
        var book = Book.Create(Guid.NewGuid(), "DDD", "Eric Evans", 2003, 1);
        var loan = Loan.Create(Guid.NewGuid(), book.Id, DateTimeOffset.UtcNow.AddDays(-1));
        loan.Return(DateTimeOffset.UtcNow);

        _loans.Setup(x => x.GetAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
        _books.Setup(x => x.GetAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _handler.HandleAsync(new(loan.Id), default);

        Assert.False(result.Success);
        Assert.Equal("loan.already_returned.conflict", result.Error!.Code);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
