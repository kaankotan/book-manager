using BookManager.Application.Books.Commands.AddBook;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Books.Queries.GetAllBooks;
using BookManager.Application.Books.Queries.GetBookById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.Api.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly ISender _sender;

    public BooksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _sender.Send(new GetAllBooksQuery(), cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await _sender.Send(new GetBookByIdQuery(id), cancellationToken);

        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Add(AddBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }
}
