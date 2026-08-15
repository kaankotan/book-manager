namespace BookManager.Domain.Entities;

public record BookChange(BookChangeType ChangeType, string? NewValue);
