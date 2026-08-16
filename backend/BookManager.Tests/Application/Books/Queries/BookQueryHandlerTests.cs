using BookManager.Application.Books.Queries.GetAllBooks;
using BookManager.Application.Books.Queries.GetBookById;
using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Books.Queries;

public class GetAllBooksQueryHandlerTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryHandlerTests()
    {
        _handler = new GetAllBooksQueryHandler(_bookRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_MapsEveryBookInOrder()
    {
        var dune = new Book("Dune", "A desert epic", PublishedDate);
        var messiah = new Book("Dune Messiah", "The sequel", PublishedDate);
        _bookRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([dune, messiah]);

        var result = await _handler.Handle(new GetAllBooksQuery(), CancellationToken.None);

        Assert.Equal(["Dune", "Dune Messiah"], result.Select(book => book.Title));
        Assert.Equal([dune.Id, messiah.Id], result.Select(book => book.Id));
    }

    [Fact]
    public async Task Handle_MapsNestedAuthors()
    {
        var herbert = new Author("Frank Herbert");
        var dune = new Book("Dune", "A desert epic", PublishedDate, [herbert]);
        _bookRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([dune]);

        var result = await _handler.Handle(new GetAllBooksQuery(), CancellationToken.None);

        var author = Assert.Single(Assert.Single(result).Authors);
        Assert.Equal(herbert.Id, author.Id);
        Assert.Equal("Frank Herbert", author.Name);
    }

    [Fact]
    public async Task Handle_WithNoBooks_ReturnsAnEmptyList()
    {
        _bookRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetAllBooksQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        _bookRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _handler.Handle(new GetAllBooksQuery(), cts.Token);

        await _bookRepository.Received(1).GetAllAsync(cts.Token);
    }
}

public class GetBookByIdQueryHandlerTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
    private readonly GetBookByIdQueryHandler _handler;

    public GetBookByIdQueryHandlerTests()
    {
        _handler = new GetBookByIdQueryHandler(_bookRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_WithAnExistingBook_ReturnsTheMappedBook()
    {
        var herbert = new Author("Frank Herbert");
        var book = new Book("Dune", "A desert epic", PublishedDate, [herbert]);
        _bookRepository.GetByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        var result = await _handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal("A desert epic", result.Description);
        Assert.Equal(PublishedDate, result.PublishedDate);
        Assert.Equal(herbert.Id, Assert.Single(result.Authors).Id);
    }

    [Fact]
    public async Task Handle_WithAMissingBook_ReturnsNull()
    {
        _bookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

        var result = await _handler.Handle(new GetBookByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PassesTheRequestedIdAndTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        _bookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

        await _handler.Handle(new GetBookByIdQuery(id), cts.Token);

        await _bookRepository.Received(1).GetByIdAsync(id, cts.Token);
    }
}
