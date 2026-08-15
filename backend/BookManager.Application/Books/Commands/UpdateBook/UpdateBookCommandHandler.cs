using AutoMapper;
using BookManager.Application.Books.Dtos;
using BookManager.Application.Repositories.Books;
using MediatR;

namespace BookManager.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, BookDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public UpdateBookCommandHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (book is null)
        {
            throw new KeyNotFoundException($"Book not found: {request.Id}");
        }

        book.UpdateDetails(request.Title, request.Description);

        await _bookRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BookDto>(book);
    }
}
