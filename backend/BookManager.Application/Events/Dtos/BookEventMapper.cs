using BookManager.Domain.Entities;

namespace BookManager.Application.Events.Dtos;

public static class BookEventMapper
{
    public static BookEventDto ToDto(BookEvent bookEvent)
    {
        return new BookEventDto(
            bookEvent.Id,
            bookEvent.BookId,
            bookEvent.ChangeType.ToString(),
            bookEvent.NewValue,
            bookEvent.OccurredAt,
            Describe(bookEvent)
        );
    }

    private static string Describe(BookEvent bookEvent)
    {
        return bookEvent.ChangeType switch
        {
            BookChangeType.Created => $"Book was created with title \"{bookEvent.NewValue}\"",
            BookChangeType.TitleChanged => $"Title was changed to \"{bookEvent.NewValue}\"",
            BookChangeType.DescriptionChanged => "Description was changed",
            _ => "Book was changed",
        };
    }
}
