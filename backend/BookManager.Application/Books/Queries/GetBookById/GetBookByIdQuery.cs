using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Books.Queries.GetBookById;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDto?>;
