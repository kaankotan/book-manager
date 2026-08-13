using AutoMapper;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using MediatR;

namespace BookManager.Application.Books.Queries.GetAllBooks;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IReadOnlyList<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetAllBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<BookDto>>(books);
    }
}
