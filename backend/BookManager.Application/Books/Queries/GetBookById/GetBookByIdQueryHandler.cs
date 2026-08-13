using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using MediatR;

namespace BookManager.Application.Books.Queries.GetBookById;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    private readonly IBookRepository _bookRepository;

    public GetBookByIdQueryHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);

        return book?.ToDto();
    }
}
