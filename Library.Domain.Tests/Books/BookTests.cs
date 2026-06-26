using Library.Domain.Books;
using Library.Domain.Exceptions;

namespace Library.Domain.Tests.Books;

public sealed class BookTests
{
    #region Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldThrowWhenTitleIsMissing(string title)
    {
        Assert.Throws<DomainValidationException>(() => Book.Create(Guid.NewGuid(), title, "Author", 2020, 1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldThrowWhenAuthorIsMissing(string author)
    {
        Assert.Throws<DomainValidationException>(() => Book.Create(Guid.NewGuid(), "Title", author, 2020, 1));
    }

    [Fact]
    public void ShouldThrowWhenAvailableQuantityIsNegative()
    {
        Assert.Throws<DomainValidationException>(() => Book.Create(Guid.NewGuid(), "Title", "Author", 2020, -1));
    }

    [Fact]
    public void ShouldThrowWhenTakingUnavailableCopy()
    {
        var book = Book.Create(Guid.NewGuid(), "Title", "Author", 2020, 0);
        Assert.Throws<BookUnavailableException>(book.TakeAvailableCopy);
    }

    [Fact]
    public void ShouldReduceQuantityAndAdvanceVersionWhenTakingCopy()
    {
        var book = Book.Create(Guid.NewGuid(), "Title", "Author", 2020, 2);
        book.TakeAvailableCopy();
        Assert.Equal(1, book.AvailableQuantity);
        Assert.Equal(2, book.Version);
    }

    [Fact]
    public void ShouldIncreaseQuantityAndAdvanceVersionWhenReturningCopy()
    {
        var book = Book.Create(Guid.NewGuid(), "Title", "Author", 2020, 1);
        book.ReturnCopy();
        Assert.Equal(2, book.AvailableQuantity);
        Assert.Equal(2, book.Version);
    }

    #endregion
}
