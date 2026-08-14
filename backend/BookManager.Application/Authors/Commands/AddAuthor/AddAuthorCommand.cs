using BookManager.Application.Books.Dtos;
using MediatR;

namespace BookManager.Application.Authors.Commands.AddAuthor;

public record AddAuthorCommand(string Name) : IRequest<AuthorDto>;
