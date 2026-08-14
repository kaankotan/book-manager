using AutoMapper;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Authors;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Authors.Commands.AddAuthor;

public class AddAuthorCommandHandler : IRequestHandler<AddAuthorCommand, AuthorDto>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public AddAuthorCommandHandler(IAuthorRepository authorRepository, IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<AuthorDto> Handle(AddAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = new Author(request.Name);

        await _authorRepository.AddAsync(author, cancellationToken);
        await _authorRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AuthorDto>(author);
    }
}
