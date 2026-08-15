using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Books.Commands.UpdateBook;

public record UpdateBookCommand(Guid Id, string Title, string Description) : IRequest<BookDto>;
