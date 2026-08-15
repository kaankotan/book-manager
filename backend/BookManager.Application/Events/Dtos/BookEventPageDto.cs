namespace BookManager.Application.Events.Dtos;

public record BookEventPageDto(IReadOnlyList<BookEventDto> Items, long? NextCursor);
