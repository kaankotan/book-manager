namespace BookManager.Domain.Entities;

public class BookView
{
    public Guid BookId { get; private set; }

    public long LastSeenEventId { get; private set; }

    public DateTimeOffset LastViewedAt { get; private set; }

    private BookView() { }

    public BookView(Guid bookId, long lastSeenEventId, DateTimeOffset lastViewedAt)
    {
        BookId = bookId;
        LastSeenEventId = lastSeenEventId;
        LastViewedAt = lastViewedAt;
    }

    public void MarkSeen(long lastSeenEventId, DateTimeOffset viewedAt)
    {
        if (lastSeenEventId > LastSeenEventId)
        {
            LastSeenEventId = lastSeenEventId;
        }

        LastViewedAt = viewedAt;
    }
}
