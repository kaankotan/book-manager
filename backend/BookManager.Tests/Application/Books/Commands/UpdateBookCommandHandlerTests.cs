using BookManager.Application.Books.Commands.UpdateBook;
using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Books.Commands;

public class UpdateBookCommandHandlerTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        _handler = new UpdateBookCommandHandler(_bookRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_WithAnExistingBook_ReturnsTheUpdatedBook()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        var result = await _handler.Handle(new UpdateBookCommand(book.Id, "Dune Messiah", "A sandy epic"), CancellationToken.None);

        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Dune Messiah", result.Title);
        Assert.Equal("A sandy epic", result.Description);
        Assert.Equal(PublishedDate, result.PublishedDate);
    }

    [Fact]
    public async Task Handle_WithAnExistingBook_AppliesTheChangesAndSaves()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        await _handler.Handle(new UpdateBookCommand(book.Id, "Dune Messiah", "A sandy epic"), CancellationToken.None);

        Assert.Equal("Dune Messiah", book.Title);
        Assert.Equal("A sandy epic", book.Description);
        await _bookRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LoadsTheBookForTrackingSoChangesArePersisted()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        await _handler.Handle(new UpdateBookCommand(book.Id, "Dune Messiah", "A sandy epic"), CancellationToken.None);

        await _bookRepository.Received(1).GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>());
        await _bookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var book = new Book("Dune", "A desert epic", PublishedDate);
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        await _handler.Handle(new UpdateBookCommand(book.Id, "Dune Messiah", "A sandy epic"), cts.Token);

        await _bookRepository.Received(1).GetTrackedByIdAsync(book.Id, cts.Token);
        await _bookRepository.Received(1).SaveChangesAsync(cts.Token);
    }

    [Fact]
    public async Task Handle_RecordsPendingChangesForTheEventOutbox()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        book.ClearPendingChanges();
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        await _handler.Handle(new UpdateBookCommand(book.Id, "Dune Messiah", "A sandy epic"), CancellationToken.None);

        Assert.Equal(
            [BookChangeType.TitleChanged, BookChangeType.DescriptionChanged],
            book.PendingChanges.Select(change => change.ChangeType)
        );
    }

    [Fact]
    public async Task Handle_WithUnchangedValues_StillSaves()
    {
        var book = new Book("Dune", "A desert epic", PublishedDate);
        _bookRepository.GetTrackedByIdAsync(book.Id, Arg.Any<CancellationToken>()).Returns(book);

        await _handler.Handle(new UpdateBookCommand(book.Id, "Dune", "A desert epic"), CancellationToken.None);

        await _bookRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAMissingBook_ThrowsAndNamesTheId()
    {
        var id = Guid.NewGuid();
        _bookRepository.GetTrackedByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Book?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new UpdateBookCommand(id, "Dune Messiah", "A sandy epic"), CancellationToken.None)
        );

        Assert.Contains(id.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_WithAMissingBook_DoesNotSave()
    {
        _bookRepository.GetTrackedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Book?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new UpdateBookCommand(Guid.NewGuid(), "Dune Messiah", "A sandy epic"), CancellationToken.None)
        );

        await _bookRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
