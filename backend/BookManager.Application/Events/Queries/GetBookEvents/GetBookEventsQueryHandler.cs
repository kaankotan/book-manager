using BookManager.Application.Events.Dtos;
using BookManager.Application.Repositories.BookEvents;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public class GetBookEventsQueryHandler : IRequestHandler<GetBookEventsQuery, BookEventPageDto>
{
    private readonly IBookEventRepository _bookEventRepository;

    public GetBookEventsQueryHandler(IBookEventRepository bookEventRepository)
    {
        _bookEventRepository = bookEventRepository;
    }

    public async Task<BookEventPageDto> Handle(GetBookEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _bookEventRepository.GetPageAsync(request.BookId, request.Before, request.Limit, cancellationToken);

        var hasMore = events.Count > request.Limit;

        var items = events
            .Take(request.Limit)
            .Select(bookEvent => new BookEventDto(
                bookEvent.Id,
                bookEvent.BookId,
                bookEvent.ChangeType.ToString(),
                bookEvent.NewValue,
                bookEvent.OccurredAt,
                Describe(bookEvent)
            ))
            .ToList();

        var nextCursor = hasMore ? items[^1].Id : (long?)null;

        return new BookEventPageDto(items, nextCursor);
    }

    private static string Describe(BookEvent bookEvent)
    {
        return bookEvent.ChangeType switch
        {
            BookChangeType.Created => $"Book was created with title \"{bookEvent.NewValue}\"",
            BookChangeType.TitleChanged => $"Title was changed to \"{bookEvent.NewValue}\"",
            BookChangeType.DescriptionChanged => "Description was changed",
            _ => "Book was changed",
        };
    }
}
