namespace BookManager.Application.Events.Dtos;

public record UnseenBookChangesDto(
    bool FirstView,
    long? LastSeenEventId,
    long? LatestEventId,
    IReadOnlyList<BookEventDto> Items,
    bool HasMore
);
