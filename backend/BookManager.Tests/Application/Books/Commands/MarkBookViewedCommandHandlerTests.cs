using BookManager.Application.Books.Commands.MarkBookViewed;
using BookManager.Application.Repositories.BookViews;
using BookManager.Domain.Entities;
using NSubstitute;

namespace BookManager.Tests.Application.Books.Commands;

public class MarkBookViewedCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IBookViewRepository _bookViewRepository = Substitute.For<IBookViewRepository>();
    private readonly MarkBookViewedCommandHandler _handler;

    public MarkBookViewedCommandHandlerTests()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(Now);

        _handler = new MarkBookViewedCommandHandler(_bookViewRepository, timeProvider);
    }

    [Fact]
    public async Task Handle_WithNoExistingView_RecordsTheWatermark()
    {
        var bookId = Guid.NewGuid();
        _bookViewRepository.GetTrackedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((BookView?)null);

        await _handler.Handle(new MarkBookViewedCommand(bookId, 30), CancellationToken.None);

        await _bookViewRepository
            .Received(1)
            .AddAsync(
                Arg.Is<BookView>(view => view.BookId == bookId && view.LastSeenEventId == 30 && view.LastViewedAt == Now),
                Arg.Any<CancellationToken>()
            );
        await _bookViewRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAnExistingView_AdvancesTheWatermark()
    {
        var bookId = Guid.NewGuid();
        var view = new BookView(bookId, 20, Now.AddDays(-1));
        _bookViewRepository.GetTrackedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(view);

        await _handler.Handle(new MarkBookViewedCommand(bookId, 30), CancellationToken.None);

        Assert.Equal(30, view.LastSeenEventId);
        Assert.Equal(Now, view.LastViewedAt);
        await _bookViewRepository.DidNotReceive().AddAsync(Arg.Any<BookView>(), Arg.Any<CancellationToken>());
        await _bookViewRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAStaleWatermark_DoesNotMoveItBackwards()
    {
        var bookId = Guid.NewGuid();
        var view = new BookView(bookId, 30, Now.AddDays(-1));
        _bookViewRepository.GetTrackedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(view);

        await _handler.Handle(new MarkBookViewedCommand(bookId, 20), CancellationToken.None);

        Assert.Equal(30, view.LastSeenEventId);
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var bookId = Guid.NewGuid();
        _bookViewRepository.GetTrackedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((BookView?)null);

        await _handler.Handle(new MarkBookViewedCommand(bookId, 30), cts.Token);

        await _bookViewRepository.Received(1).GetTrackedAsync(bookId, cts.Token);
        await _bookViewRepository.Received(1).AddAsync(Arg.Any<BookView>(), cts.Token);
        await _bookViewRepository.Received(1).SaveChangesAsync(cts.Token);
    }
}
