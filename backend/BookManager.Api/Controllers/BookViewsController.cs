using BookManager.Application.Books.Commands.MarkBookViewed;
using BookManager.Application.Events.Dtos;
using BookManager.Application.Events.Queries.GetUnseenBookChanges;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.Api.Controllers;

[ApiController]
public class BookViewsController : ControllerBase
{
    private const int DefaultLimit = 50;

    private readonly ISender _sender;

    public BookViewsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("/api/books/{bookId:guid}/unseen-changes")]
    public async Task<ActionResult<UnseenBookChangesDto>> GetUnseenChanges(
        Guid bookId,
        [FromQuery] int limit = DefaultLimit,
        CancellationToken cancellationToken = default
    )
    {
        var changes = await _sender.Send(new GetUnseenBookChangesQuery(bookId, limit), cancellationToken);

        return Ok(changes);
    }

    [HttpPut("/api/books/{bookId:guid}/view")]
    public async Task<IActionResult> MarkViewed(Guid bookId, MarkBookViewedCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command with { BookId = bookId }, cancellationToken);

        return NoContent();
    }
}
