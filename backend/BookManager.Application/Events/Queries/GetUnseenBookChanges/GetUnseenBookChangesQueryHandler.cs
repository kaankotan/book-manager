using AutoMapper;
using BookManager.Application.Events.Dtos;
using BookManager.Application.Repositories.BookEvents;
using BookManager.Application.Repositories.BookViews;
using MediatR;

namespace BookManager.Application.Events.Queries.GetUnseenBookChanges;

public class GetUnseenBookChangesQueryHandler : IRequestHandler<GetUnseenBookChangesQuery, UnseenBookChangesDto>
{
    private readonly IBookViewRepository _bookViewRepository;
    private readonly IBookEventRepository _bookEventRepository;
    private readonly IMapper _mapper;

    public GetUnseenBookChangesQueryHandler(
        IBookViewRepository bookViewRepository,
        IBookEventRepository bookEventRepository,
        IMapper mapper
    )
    {
        _bookViewRepository = bookViewRepository;
        _bookEventRepository = bookEventRepository;
        _mapper = mapper;
    }

    public async Task<UnseenBookChangesDto> Handle(GetUnseenBookChangesQuery request, CancellationToken cancellationToken)
    {
        var view = await _bookViewRepository.GetAsync(request.BookId, cancellationToken);

        if (view is null)
        {
            var latestId = await _bookEventRepository.GetLatestIdAsync(request.BookId, cancellationToken);

            return new UnseenBookChangesDto(FirstView: true, LastSeenEventId: null, latestId, [], HasMore: false);
        }

        var events = await _bookEventRepository.GetPageAsync(
            request.BookId,
            before: null,
            since: view.LastSeenEventId,
            request.Limit,
            cancellationToken
        );

        var hasMore = events.Count > request.Limit;

        var items = _mapper.Map<List<BookEventDto>>(events.Take(request.Limit).ToList());

        var latestEventId = items.Count > 0 ? items[0].Id : view.LastSeenEventId;

        return new UnseenBookChangesDto(FirstView: false, view.LastSeenEventId, latestEventId, items, hasMore);
    }
}
