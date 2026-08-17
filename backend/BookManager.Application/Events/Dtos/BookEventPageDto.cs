namespace BookManager.Application.Events.Dtos;

public record BookEventPageDto(IReadOnlyList<BookEventDto> Items, int Page, int PageSize, int TotalCount);
