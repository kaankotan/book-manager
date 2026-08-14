using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Authors.Queries.GetAuthorById;

public record GetAuthorByIdQuery(Guid Id) : IRequest<AuthorDto?>;
