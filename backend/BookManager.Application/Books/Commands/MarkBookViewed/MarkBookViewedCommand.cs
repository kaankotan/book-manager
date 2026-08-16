using MediatR;

namespace BookManager.Application.Books.Commands.MarkBookViewed;

public record MarkBookViewedCommand(Guid BookId, long LastSeenEventId) : IRequest;
