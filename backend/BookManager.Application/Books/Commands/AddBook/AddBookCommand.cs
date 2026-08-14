using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Books.Commands.AddBook;

public record AddBookCommand(string Title, string Description, DateOnly PublishedDate, IReadOnlyList<Guid> AuthorIds) : IRequest<BookDto>;
