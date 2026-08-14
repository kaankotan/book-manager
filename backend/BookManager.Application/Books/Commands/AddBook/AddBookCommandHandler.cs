using AutoMapper;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Authors;
using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Books.Commands.AddBook;

public class AddBookCommandHandler : IRequestHandler<AddBookCommand, BookDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public AddBookCommandHandler(IBookRepository bookRepository, IAuthorRepository authorRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<BookDto> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        var authors = await _authorRepository.GetByIdsAsync(request.AuthorIds, cancellationToken);

        var missingAuthorIds = request.AuthorIds.Except(authors.Select(author => author.Id)).ToList();
        if (missingAuthorIds.Count > 0)
        {
            throw new KeyNotFoundException($"Author(s) not found: {string.Join(", ", missingAuthorIds)}");
        }

        var book = new Book(request.Title, request.Description, request.PublishedDate, authors);

        await _bookRepository.AddAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BookDto>(book);
    }
}
