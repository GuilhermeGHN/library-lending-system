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

public sealed class CreateLoanHandlerTests
{
    #region Fields

    private readonly Mock<IBookRepository> _books = new();

    private readonly Mock<ILoanRepository> _loans = new();

    private readonly Mock<IOutbox> _outbox = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly List<IDomainEvent> _outboxEvents = [];

    private readonly CreateLoanHandler _handler;

    #endregion

    #region Constructors

    public CreateLoanHandlerTests()
    {
        _outbox.Setup(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((domainEvent, _) => _outboxEvents.Add(domainEvent))
            .Returns(Task.CompletedTask);

        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CreateLoanHandler(_books.Object, _loans.Object, _outbox.Object, _unitOfWork.Object, NullLogger<CreateLoanHandler>.Instance);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task ShouldCreateLoanChangeAvailabilityPersistOutboxEventAndCommit()
    {
        var book = Book.Create(Guid.NewGuid(), "DDD", "Eric Evans", 2003, 1);
        Loan? addedLoan = null;

        _books.Setup(x => x.GetAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loans.Setup(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((loan, _) => addedLoan = loan)
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new(book.Id), default);

        Assert.True(result.Success);
        Assert.Equal(0, book.AvailableQuantity);
        Assert.Equal(result.Value!.Id, addedLoan!.Id);
        var message = Assert.IsType<LoanCreatedEvent>(Assert.Single(_outboxEvents));
        Assert.Equal(0, message.AvailableQuantity);
        Assert.Equal(2, message.BookVersion);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldNotCommitWhenBookIsMissing()
    {
        var bookId = Guid.NewGuid();

        _books.Setup(x => x.GetAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var result = await _handler.HandleAsync(new(bookId), default);

        Assert.False(result.Success);
        Assert.Equal("book.not_found", result.Error!.Code);
        _loans.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotCommitWhenBookIsUnavailable()
    {
        var book = Book.Create(Guid.NewGuid(), "DDD", "Eric Evans", 2003, 0);

        _books.Setup(x => x.GetAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _handler.HandleAsync(new(book.Id), default);

        Assert.False(result.Success);
        Assert.Equal("book.unavailable.conflict", result.Error!.Code);
        _loans.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
