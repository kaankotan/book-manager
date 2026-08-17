using BookManager.Application.Events.Dtos;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public record GetBookEventsQuery(
    IReadOnlyList<Guid> BookIds,
    IReadOnlyList<BookChangeType> ChangeTypes,
    int Page,
    int PageSize,
    BookEventSortField SortBy,
    bool Descending
) : IRequest<BookEventPageDto>;
