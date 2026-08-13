using AutoMapper;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using MediatR;

namespace BookManager.Application.Books.Queries.GetBookById;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);

        return book is null ? null : _mapper.Map<BookDto>(book);
    }
}
