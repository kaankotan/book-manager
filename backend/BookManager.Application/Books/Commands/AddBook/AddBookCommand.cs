using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Books.Commands.AddBook;

public record AddBookCommand(
    string Title,
    string Author,
    string? Isbn = null,
    int? PublishedYear = null) : IRequest<BookDto>;
