using BookManager.Application.Events.Queries.GetUnseenBookChanges;
using BookManager.Application.Repositories.BookEvents;
using BookManager.Application.Repositories.BookViews;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Events;

public class GetUnseenBookChangesQueryHandlerTests
{
    private static readonly DateTimeOffset ViewedAt = new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IBookViewRepository _bookViewRepository = Substitute.For<IBookViewRepository>();
    private readonly IBookEventRepository _bookEventRepository = Substitute.For<IBookEventRepository>();
    private readonly GetUnseenBookChangesQueryHandler _handler;

    public GetUnseenBookChangesQueryHandlerTests()
    {
        _handler = new GetUnseenBookChangesQueryHandler(_bookViewRepository, _bookEventRepository, TestMapper.Create());
    }

    private void GivenNoPreviousView()
    {
        _bookViewRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((BookView?)null);
    }

    private void GivenPreviousView(Guid bookId, long lastSeenEventId)
    {
        _bookViewRepository
            .GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new BookView(bookId, lastSeenEventId, ViewedAt));
    }

    private void GivenUnseenEvents(params BookEvent[] events)
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(events);
    }

    [Fact]
    public async Task Handle_OnAFirstView_ReportsAFirstViewWithNoChanges()
    {
        GivenNoPreviousView();
        _bookEventRepository.GetLatestIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(30L);

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(Guid.NewGuid(), 50), CancellationToken.None);

        Assert.True(result.FirstView);
        Assert.Null(result.LastSeenEventId);
        Assert.Equal(30L, result.LatestEventId);
        Assert.Empty(result.Items);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task Handle_OnAFirstView_DoesNotReadTheEventPage()
    {
        GivenNoPreviousView();
        _bookEventRepository.GetLatestIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(30L);

        await _handler.Handle(new GetUnseenBookChangesQuery(Guid.NewGuid(), 50), CancellationToken.None);

        await _bookEventRepository
            .DidNotReceive()
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnAFirstViewOfABookWithNoEvents_ReportsNoLatestEvent()
    {
        GivenNoPreviousView();
        _bookEventRepository.GetLatestIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((long?)null);

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(Guid.NewGuid(), 50), CancellationToken.None);

        Assert.True(result.FirstView);
        Assert.Null(result.LatestEventId);
    }

    [Fact]
    public async Task Handle_WithAPreviousView_AsksForEventsAfterTheWatermark()
    {
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 20);
        GivenUnseenEvents();

        await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), CancellationToken.None);

        await _bookEventRepository.Received(1).GetPageAsync(bookId, null, 20L, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnseenEvents_ReturnsThemNewestFirst()
    {
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 20);
        GivenUnseenEvents(BookEventFactory.WithId(30, bookId), BookEventFactory.WithId(25, bookId));

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), CancellationToken.None);

        Assert.False(result.FirstView);
        Assert.Equal(20L, result.LastSeenEventId);
        Assert.Equal([30L, 25L], result.Items.Select(item => item.Id));
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task Handle_WithUnseenEvents_ReportsTheNewestAsTheLatestEvent()
    {
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 20);
        GivenUnseenEvents(BookEventFactory.WithId(30, bookId), BookEventFactory.WithId(25, bookId));

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), CancellationToken.None);

        Assert.Equal(30L, result.LatestEventId);
    }

    [Fact]
    public async Task Handle_WithNothingUnseen_KeepsTheWatermarkAsTheLatestEvent()
    {
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 20);
        GivenUnseenEvents();

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), CancellationToken.None);

        Assert.False(result.FirstView);
        Assert.Empty(result.Items);
        Assert.Equal(20L, result.LatestEventId);
    }

    [Fact]
    public async Task Handle_WithMoreUnseenThanTheLimit_TrimsAndFlagsThatMoreExist()
    {
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 10);
        GivenUnseenEvents(BookEventFactory.WithId(30, bookId), BookEventFactory.WithId(25, bookId), BookEventFactory.WithId(20, bookId));

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 2), CancellationToken.None);

        Assert.Equal([30L, 25L], result.Items.Select(item => item.Id));
        Assert.True(result.HasMore);
        Assert.Equal(30L, result.LatestEventId);
    }

    [Fact]
    public async Task Handle_MapsTheChangeTypeToItsName()
    {
        var bookId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(2024, 5, 2, 9, 0, 0, TimeSpan.Zero);
        GivenPreviousView(bookId, 10);
        GivenUnseenEvents(BookEventFactory.WithId(11, bookId, BookChangeType.TitleChanged, "Dune Messiah", occurredAt));

        var result = await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(nameof(BookChangeType.TitleChanged), item.ChangeType);
        Assert.Equal("Dune Messiah", item.NewValue);
        Assert.Equal(occurredAt, item.OccurredAt);
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var bookId = Guid.NewGuid();
        GivenPreviousView(bookId, 20);
        GivenUnseenEvents();

        await _handler.Handle(new GetUnseenBookChangesQuery(bookId, 50), cts.Token);

        await _bookViewRepository.Received(1).GetAsync(bookId, cts.Token);
        await _bookEventRepository.Received(1).GetPageAsync(bookId, null, 20L, 50, cts.Token);
    }
}
