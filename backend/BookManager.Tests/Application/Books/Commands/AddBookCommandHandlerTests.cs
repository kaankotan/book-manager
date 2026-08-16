using BookManager.Application.Books.Commands.AddBook;
using BookManager.Application.Repositories.Authors;
using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Books.Commands;

public class AddBookCommandHandlerTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
    private readonly IAuthorRepository _authorRepository = Substitute.For<IAuthorRepository>();
    private readonly AddBookCommandHandler _handler;

    public AddBookCommandHandlerTests()
    {
        _handler = new AddBookCommandHandler(_bookRepository, _authorRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_WithKnownAuthors_ReturnsTheMappedBook()
    {
        var herbert = new Author("Frank Herbert");
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([herbert]);

        var result = await _handler.Handle(
            new AddBookCommand("Dune", "A desert epic", PublishedDate, [herbert.Id]),
            CancellationToken.None
        );

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal("A desert epic", result.Description);
        Assert.Equal(PublishedDate, result.PublishedDate);
        var author = Assert.Single(result.Authors);
        Assert.Equal(herbert.Id, author.Id);
        Assert.Equal("Frank Herbert", author.Name);
    }

    [Fact]
    public async Task Handle_WithKnownAuthors_PersistsTheBook()
    {
        var herbert = new Author("Frank Herbert");
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([herbert]);

        await _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, [herbert.Id]), CancellationToken.None);

        await _bookRepository
            .Received(1)
            .AddAsync(Arg.Is<Book>(book => book.Title == "Dune" && book.Authors.Contains(herbert)), Arg.Any<CancellationToken>());
        await _bookRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        await _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, []), cts.Token);

        await _authorRepository.Received(1).GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), cts.Token);
        await _bookRepository.Received(1).AddAsync(Arg.Any<Book>(), cts.Token);
        await _bookRepository.Received(1).SaveChangesAsync(cts.Token);
    }

    [Fact]
    public async Task Handle_WithAnUnknownAuthor_ThrowsAndNamesTheMissingId()
    {
        var missingId = Guid.NewGuid();
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, [missingId]), CancellationToken.None)
        );

        Assert.Contains(missingId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_WithAnUnknownAuthor_DoesNotPersistAnything()
    {
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, [Guid.NewGuid()]), CancellationToken.None)
        );

        await _bookRepository.DidNotReceive().AddAsync(Arg.Any<Book>(), Arg.Any<CancellationToken>());
        await _bookRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSomeAuthorsMissing_NamesOnlyTheMissingOnes()
    {
        var herbert = new Author("Frank Herbert");
        var missingId = Guid.NewGuid();
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([herbert]);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, [herbert.Id, missingId]), CancellationToken.None)
        );

        Assert.Contains(missingId.ToString(), exception.Message);
        Assert.DoesNotContain(herbert.Id.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_WithNoAuthorsRequested_CreatesAnAuthorlessBook()
    {
        _authorRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new AddBookCommand("Dune", "A desert epic", PublishedDate, []), CancellationToken.None);

        Assert.Empty(result.Authors);
        await _bookRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
