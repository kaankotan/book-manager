using BookManager.Application.Events.Dtos;
using BookManager.Application.Events.Queries.GetBookEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.Api.Controllers;

[ApiController]
public class EventsController : ControllerBase
{
    private const int DefaultLimit = 50;

    private readonly ISender _sender;

    public EventsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("/api/events")]
    public async Task<ActionResult<BookEventPageDto>> GetAll(
        [FromQuery] long? before,
        [FromQuery] int limit = DefaultLimit,
        CancellationToken cancellationToken = default
    )
    {
        var events = await _sender.Send(new GetBookEventsQuery(null, before, limit), cancellationToken);

        return Ok(events);
    }

    [HttpGet("/api/books/{bookId:guid}/events")]
    public async Task<ActionResult<BookEventPageDto>> GetByBook(
        Guid bookId,
        [FromQuery] long? before,
        [FromQuery] int limit = DefaultLimit,
        CancellationToken cancellationToken = default
    )
    {
        var events = await _sender.Send(new GetBookEventsQuery(bookId, before, limit), cancellationToken);

        return Ok(events);
    }
}
