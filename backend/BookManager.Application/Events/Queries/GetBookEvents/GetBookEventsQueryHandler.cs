using AutoMapper;
using BookManager.Application.Events.Dtos;
using BookManager.Application.Repositories.BookEvents;
using MediatR;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public class GetBookEventsQueryHandler : IRequestHandler<GetBookEventsQuery, BookEventPageDto>
{
    private readonly IBookEventRepository _bookEventRepository;
    private readonly IMapper _mapper;

    public GetBookEventsQueryHandler(IBookEventRepository bookEventRepository, IMapper mapper)
    {
        _bookEventRepository = bookEventRepository;
        _mapper = mapper;
    }

    public async Task<BookEventPageDto> Handle(GetBookEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _bookEventRepository.GetPageAsync(
            request.BookId,
            request.Before,
            request.Since,
            request.Limit,
            cancellationToken
        );

        var hasMore = events.Count > request.Limit;

        var items = _mapper.Map<List<BookEventDto>>(events.Take(request.Limit).ToList());

        var nextCursor = hasMore ? items[^1].Id : (long?)null;

        return new BookEventPageDto(items, nextCursor);
    }
}
