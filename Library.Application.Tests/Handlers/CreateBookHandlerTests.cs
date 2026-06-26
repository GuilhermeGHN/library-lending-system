using Library.Application.Common.Results;
using Library.Application.Events;
using Library.Application.Handlers;
using Library.Application.Interfaces.Events;
using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.Repositories;
using Library.Domain.Books;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Library.Application.Tests.Handlers;

public sealed class CreateBookHandlerTests
{
    #region Fields

    private readonly Mock<IBookRepository> _books = new();

    private readonly Mock<IOutbox> _outbox = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly List<IDomainEvent> _outboxEvents = [];

    private readonly CreateBookHandler _handler;

    #endregion

    #region Constructors

    public CreateBookHandlerTests()
    {
        _outbox.Setup(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((domainEvent, _) => _outboxEvents.Add(domainEvent))
            .Returns(Task.CompletedTask);

        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CreateBookHandler(_books.Object, _outbox.Object, _unitOfWork.Object, NullLogger<CreateBookHandler>.Instance);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task ShouldCreateBookPersistOutboxEventAndCommit()
    {
        Book? addedBook = null;

        _books.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) => addedBook = book)
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new("Domain-Driven Design", "Eric Evans", 2003, 2), default);

        Assert.True(result.Success);
        Assert.Equal(result.Value!.Id, addedBook!.Id);
        var message = Assert.IsType<BookCreatedEvent>(Assert.Single(_outboxEvents));
        Assert.Equal(2, message.AvailableQuantity);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldNotPersistOrCommitInvalidCreateBookInput()
    {
        var result = await _handler.HandleAsync(new("", "Author", 2020, 1), default);

        Assert.False(result.Success);
        Assert.Equal("validation", result.Error!.Code);
        _books.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
        _outbox.Verify(x => x.AddAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenCommitFails()
    {
        var error = Error.Conflict("resource.concurrency", "The resource changed.");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(error));

        var result = await _handler.HandleAsync(new("Domain-Driven Design", "Eric Evans", 2003, 2), default);

        Assert.False(result.Success);
        Assert.Equal(error, result.Error);
    }

    #endregion
}
