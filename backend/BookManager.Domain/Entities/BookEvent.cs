namespace BookManager.Domain.Entities;

public class BookEvent
{
    public long Id { get; private set; }

    public Guid BookId { get; private set; }

    public BookChangeType ChangeType { get; private set; }

    public string? NewValue { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? DispatchedAt { get; private set; }

    private BookEvent() { }

    public BookEvent(Guid bookId, BookChangeType changeType, string? newValue, DateTimeOffset occurredAt)
    {
        BookId = bookId;
        ChangeType = changeType;
        NewValue = newValue;
        OccurredAt = occurredAt;
    }

    public void MarkDispatched(DateTimeOffset dispatchedAt)
    {
        DispatchedAt = dispatchedAt;
    }
}
