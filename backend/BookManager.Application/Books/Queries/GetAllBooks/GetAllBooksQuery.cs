using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Books.Queries.GetAllBooks;

public record GetAllBooksQuery : IRequest<IReadOnlyList<BookDto>>;
