using BookManager.Domain.Entities;

namespace BookManager.Tests.Domain;

public class BookEventTests
{
    private static readonly DateTimeOffset OccurredAt = new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_SetsDetailsAndLeavesTheEventUndispatched()
    {
        var bookId = Guid.NewGuid();

        var bookEvent = new BookEvent(bookId, BookChangeType.TitleChanged, "Dune Messiah", OccurredAt);

        Assert.Equal(bookId, bookEvent.BookId);
        Assert.Equal(BookChangeType.TitleChanged, bookEvent.ChangeType);
        Assert.Equal("Dune Messiah", bookEvent.NewValue);
        Assert.Equal(OccurredAt, bookEvent.OccurredAt);
        Assert.Null(bookEvent.DispatchedAt);
    }

    [Fact]
    public void Constructor_AcceptsANullNewValue()
    {
        var bookEvent = new BookEvent(Guid.NewGuid(), BookChangeType.Created, null, OccurredAt);

        Assert.Null(bookEvent.NewValue);
    }

    [Fact]
    public void MarkDispatched_StampsTheDispatchTime()
    {
        var bookEvent = new BookEvent(Guid.NewGuid(), BookChangeType.Created, "Dune", OccurredAt);
        var dispatchedAt = OccurredAt.AddSeconds(5);

        bookEvent.MarkDispatched(dispatchedAt);

        Assert.Equal(dispatchedAt, bookEvent.DispatchedAt);
    }
}
