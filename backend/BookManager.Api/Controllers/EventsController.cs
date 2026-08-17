using BookManager.Application.Events;
using BookManager.Application.Events.Dtos;
using BookManager.Application.Events.Queries.GetBookEvents;
using BookManager.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.Api.Controllers;

[ApiController]
public class EventsController : ControllerBase
{
    private const int DefaultPageSize = 50;

    private readonly ISender _sender;

    public EventsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("/api/events")]
    public async Task<ActionResult<BookEventPageDto>> GetAll(
        [FromQuery] Guid[]? bookIds = null,
        [FromQuery] BookChangeType[]? changeTypes = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] BookEventSortField sortBy = BookEventSortField.OccurredAt,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default
    )
    {
        var events = await _sender.Send(
            new GetBookEventsQuery(bookIds ?? [], changeTypes ?? [], page, pageSize, sortBy, descending),
            cancellationToken
        );

        return Ok(events);
    }

    [HttpGet("/api/books/{bookId:guid}/events")]
    public async Task<ActionResult<BookEventPageDto>> GetByBook(
        Guid bookId,
        [FromQuery] BookChangeType[]? changeTypes = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] BookEventSortField sortBy = BookEventSortField.OccurredAt,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default
    )
    {
        var events = await _sender.Send(
            new GetBookEventsQuery([bookId], changeTypes ?? [], page, pageSize, sortBy, descending),
            cancellationToken
        );

        return Ok(events);
    }
}
