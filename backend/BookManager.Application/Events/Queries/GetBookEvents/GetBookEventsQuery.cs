using BookManager.Application.Events.Dtos;
using MediatR;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public record GetBookEventsQuery(Guid? BookId, long? Before, int Limit) : IRequest<BookEventPageDto>;
