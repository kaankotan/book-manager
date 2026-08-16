using BookManager.Application.Events.Dtos;
using MediatR;

namespace BookManager.Application.Events.Queries.GetUnseenBookChanges;

public record GetUnseenBookChangesQuery(Guid BookId, int Limit) : IRequest<UnseenBookChangesDto>;
