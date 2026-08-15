namespace BookManager.Application.Events.Dtos;

public record BookEventDto(long Id, Guid BookId, string ChangeType, string? NewValue, DateTimeOffset OccurredAt, string Description);
