using BookManager.Application.Events.Dtos;
using MediatR;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public record GetBookEventsQuery(Guid? BookId, int Page, int PageSize) : IRequest<BookEventPageDto>;
