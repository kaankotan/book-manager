using BookManager.Domain.Entities;

namespace BookManager.Tests.Domain;

public class BookViewTests
{
    private static readonly DateTimeOffset ViewedAt = new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_SetsTheWatermarkAndViewTime()
    {
        var bookId = Guid.NewGuid();

        var view = new BookView(bookId, 42, ViewedAt);

        Assert.Equal(bookId, view.BookId);
        Assert.Equal(42, view.LastSeenEventId);
        Assert.Equal(ViewedAt, view.LastViewedAt);
    }

    [Fact]
    public void MarkSeen_WithANewerEvent_AdvancesTheWatermark()
    {
        var view = new BookView(Guid.NewGuid(), 42, ViewedAt);
        var later = ViewedAt.AddMinutes(5);

        view.MarkSeen(43, later);

        Assert.Equal(43, view.LastSeenEventId);
        Assert.Equal(later, view.LastViewedAt);
    }

    [Fact]
    public void MarkSeen_WithAnOlderEvent_LeavesTheWatermarkAlone()
    {
        var view = new BookView(Guid.NewGuid(), 42, ViewedAt);

        view.MarkSeen(7, ViewedAt.AddMinutes(5));

        Assert.Equal(42, view.LastSeenEventId);
    }

    [Fact]
    public void MarkSeen_WithTheSameEvent_LeavesTheWatermarkAlone()
    {
        var view = new BookView(Guid.NewGuid(), 42, ViewedAt);

        view.MarkSeen(42, ViewedAt.AddMinutes(5));

        Assert.Equal(42, view.LastSeenEventId);
    }

    [Fact]
    public void MarkSeen_WithAnOlderEvent_StillRecordsTheVisit()
    {
        var view = new BookView(Guid.NewGuid(), 42, ViewedAt);
        var later = ViewedAt.AddMinutes(5);

        view.MarkSeen(7, later);

        Assert.Equal(later, view.LastViewedAt);
    }
}
