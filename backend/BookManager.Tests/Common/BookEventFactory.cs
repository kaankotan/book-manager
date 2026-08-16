using BookManager.Domain.Entities;

namespace BookManager.Tests.Common;

/// <summary>
/// Builds <see cref="BookEvent"/> instances with a chosen <see cref="BookEvent.Id"/>. The identity is
/// database-generated and has a private setter, so tests that care about cursors set it by reflection.
/// </summary>
public static class BookEventFactory
{
    public static BookEvent WithId(
        long id,
        Guid? bookId = null,
        BookChangeType changeType = BookChangeType.Created,
        string? newValue = "Dune",
        DateTimeOffset? occurredAt = null
    )
    {
        var bookEvent = new BookEvent(
            bookId ?? Guid.NewGuid(),
            changeType,
            newValue,
            occurredAt ?? new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero)
        );

        typeof(BookEvent).GetProperty(nameof(BookEvent.Id))!.SetValue(bookEvent, id);

        return bookEvent;
    }
}
