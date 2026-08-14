using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Authors.Queries.GetAllAuthors;

public record GetAllAuthorsQuery : IRequest<IReadOnlyList<AuthorDto>>;
