using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using MediatR;

namespace BookManager.Application.Books.Commands.AddBook;

public class AddBookCommandHandler : IRequestHandler<AddBookCommand, BookDto>
{
    private readonly IBookRepository _bookRepository;

    public AddBookCommandHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<BookDto> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book(request.Title, request.Author, request.Isbn, request.PublishedYear);

        await _bookRepository.AddAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return book.ToDto();
    }
}
