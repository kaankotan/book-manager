using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using MediatR;

namespace BookManager.Application.Books.Queries.GetAllBooks;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IReadOnlyList<BookDto>>
{
    private readonly IBookRepository _bookRepository;

    public GetAllBooksQueryHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IReadOnlyList<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);

        return books.Select(book => book.ToDto()).ToList();
    }
}
