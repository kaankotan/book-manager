using BookManager.Application.Authors.Commands.AddAuthor;
using BookManager.Application.Authors.Queries.GetAllAuthors;
using BookManager.Application.Authors.Queries.GetAuthorById;
using BookManager.Application.Books.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.Api.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ISender _sender;

    public AuthorsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuthorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await _sender.Send(new GetAllAuthorsQuery(), cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuthorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await _sender.Send(new GetAuthorByIdQuery(id), cancellationToken);

        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Add(AddAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }
}
