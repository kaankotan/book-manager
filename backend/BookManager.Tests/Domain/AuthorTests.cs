using BookManager.Domain.Entities;

namespace BookManager.Tests.Domain;

public class AuthorTests
{
    [Fact]
    public void Constructor_SetsNameAndAssignsId()
    {
        var author = new Author("Frank Herbert");

        Assert.NotEqual(Guid.Empty, author.Id);
        Assert.Equal("Frank Herbert", author.Name);
    }

    [Fact]
    public void Constructor_LeavesBooksEmpty()
    {
        var author = new Author("Frank Herbert");

        Assert.Empty(author.Books);
    }

    [Fact]
    public void Constructor_AssignsAUniqueIdPerAuthor()
    {
        var first = new Author("Frank Herbert");
        var second = new Author("Frank Herbert");

        Assert.NotEqual(first.Id, second.Id);
    }
}
