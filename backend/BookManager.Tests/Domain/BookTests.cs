using BookManager.Domain.Entities;

namespace BookManager.Tests.Domain;

public class BookTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    [Fact]
    public void Constructor_SetsDetailsAndAssignsId()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);

        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.Equal("Dune", book.Title);
        Assert.Equal("A desert epic", book.Description);
        Assert.Equal(PublishedDate, book.PublishedDate);
    }

    [Fact]
    public void Constructor_RecordsCreatedChangeCarryingTheTitle()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);

        var change = Assert.Single(book.PendingChanges);
        Assert.Equal(BookChangeType.Created, change.ChangeType);
        Assert.Equal("Dune", change.NewValue);
    }

    [Fact]
    public void Constructor_WithoutAuthors_LeavesAuthorsEmpty()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);

        Assert.Empty(book.Authors);
    }

    [Fact]
    public void Constructor_WithAuthors_CopiesThemOntoTheBook()
    {
        var herbert = new Author("Frank Herbert");
        var anderson = new Author("Kevin J. Anderson");

        var book = new Book("Dune", "A desert epic", PublishedDate, [herbert, anderson]);

        Assert.Equal(2, book.Authors.Count);
        Assert.Contains(herbert, book.Authors);
        Assert.Contains(anderson, book.Authors);
    }

    [Fact]
    public void UpdateDetails_WithNewTitle_AppliesItAndRecordsTitleChanged()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();

        book.UpdateDetails("Dune Messiah", "A desert epic");

        Assert.Equal("Dune Messiah", book.Title);
        var change = Assert.Single(book.PendingChanges);
        Assert.Equal(BookChangeType.TitleChanged, change.ChangeType);
        Assert.Equal("Dune Messiah", change.NewValue);
    }

    [Fact]
    public void UpdateDetails_WithNewDescription_AppliesItAndRecordsDescriptionChanged()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();

        book.UpdateDetails("Dune", "A sandy epic");

        Assert.Equal("A sandy epic", book.Description);
        var change = Assert.Single(book.PendingChanges);
        Assert.Equal(BookChangeType.DescriptionChanged, change.ChangeType);
        Assert.Equal("A sandy epic", change.NewValue);
    }

    [Fact]
    public void UpdateDetails_WithBothChanged_RecordsBothChanges()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();

        book.UpdateDetails("Dune Messiah", "A sandy epic");

        Assert.Equal(
            [BookChangeType.TitleChanged, BookChangeType.DescriptionChanged],
            book.PendingChanges.Select(change => change.ChangeType)
        );
    }

    [Fact]
    public void UpdateDetails_WithIdenticalValues_RecordsNothing()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();

        book.UpdateDetails("Dune", "A desert epic");

        Assert.Empty(book.PendingChanges);
    }

    [Fact]
    public void UpdateDetails_IsCaseSensitive()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();

        book.UpdateDetails("DUNE", "A desert epic");

        Assert.Equal("DUNE", book.Title);
        Assert.Single(book.PendingChanges);
    }

    [Fact]
    public void ClearPendingChanges_EmptiesTheChangeLog()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);

        book.ClearPendingChanges();

        Assert.Empty(book.PendingChanges);
    }

    [Fact]
    public void AddAuthor_AppendsToAuthors()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        var herbert = new Author("Frank Herbert");

        book.AddAuthor(herbert);

        Assert.Same(herbert, Assert.Single(book.Authors));
    }
}
