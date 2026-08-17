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
        var totalCount = await _bookEventRepository.CountAsync(request.BookIds, request.ChangeTypes, cancellationToken);

        var skip = (long)(request.Page - 1) * request.PageSize;

        if (skip >= totalCount)
        {
            return new BookEventPageDto([], request.Page, request.PageSize, totalCount);
        }

        var events = await _bookEventRepository.ListAsync(
            request.BookIds,
            request.ChangeTypes,
            (int)skip,
            request.PageSize,
            request.SortBy,
            request.Descending,
            cancellationToken
        );

        var items = _mapper.Map<List<BookEventDto>>(events.ToList());

        return new BookEventPageDto(items, request.Page, request.PageSize, totalCount);
    }
}
