using BookManager.Application.Repositories.BookViews;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Books.Commands.MarkBookViewed;

public class MarkBookViewedCommandHandler : IRequestHandler<MarkBookViewedCommand>
{
    private readonly IBookViewRepository _bookViewRepository;
    private readonly TimeProvider _timeProvider;

    public MarkBookViewedCommandHandler(IBookViewRepository bookViewRepository, TimeProvider timeProvider)
    {
        _bookViewRepository = bookViewRepository;
        _timeProvider = timeProvider;
    }

    public async Task Handle(MarkBookViewedCommand request, CancellationToken cancellationToken)
    {
        var viewedAt = _timeProvider.GetUtcNow();

        var view = await _bookViewRepository.GetTrackedAsync(request.BookId, cancellationToken);

        if (view is null)
        {
            await _bookViewRepository.AddAsync(new BookView(request.BookId, request.LastSeenEventId, viewedAt), cancellationToken);
        }
        else
        {
            view.MarkSeen(request.LastSeenEventId, viewedAt);
        }

        await _bookViewRepository.SaveChangesAsync(cancellationToken);
    }
}
