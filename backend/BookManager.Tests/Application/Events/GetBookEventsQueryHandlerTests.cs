using BookManager.Application.Events.Queries.GetBookEvents;
using BookManager.Application.Repositories.BookEvents;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Events;

public class GetBookEventsQueryHandlerTests
{
    private readonly IBookEventRepository _bookEventRepository = Substitute.For<IBookEventRepository>();
    private readonly GetBookEventsQueryHandler _handler;

    public GetBookEventsQueryHandlerTests()
    {
        _handler = new GetBookEventsQueryHandler(_bookEventRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_WhenAFullPagePlusOneComesBack_TrimsToTheLimit()
    {
        // The repository over-fetches by one so the handler can tell whether another page exists.
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(30), BookEventFactory.WithId(20), BookEventFactory.WithId(10)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 2), CancellationToken.None);

        Assert.Equal([30L, 20L], result.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Handle_WhenAnotherPageExists_ReturnsTheLastReturnedIdAsTheCursor()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(30), BookEventFactory.WithId(20), BookEventFactory.WithId(10)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 2), CancellationToken.None);

        Assert.Equal(20L, result.NextCursor);
    }

    [Fact]
    public async Task Handle_OnTheLastPage_ReturnsNoCursor()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(30), BookEventFactory.WithId(20)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 2), CancellationToken.None);

        Assert.Equal([30L, 20L], result.Items.Select(item => item.Id));
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task Handle_WithAPartialPage_ReturnsNoCursor()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(30)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 2), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task Handle_WithNoEvents_ReturnsAnEmptyPage()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 2), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task Handle_MapsTheChangeTypeToItsName()
    {
        var bookId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(7, bookId, BookChangeType.TitleChanged, "Dune Messiah", occurredAt)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 10), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(7L, item.Id);
        Assert.Equal(bookId, item.BookId);
        Assert.Equal(nameof(BookChangeType.TitleChanged), item.ChangeType);
        Assert.Equal("Dune Messiah", item.NewValue);
        Assert.Equal(occurredAt, item.OccurredAt);
    }

    [Fact]
    public async Task Handle_MapsANullNewValue()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(7, newValue: null)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, null, null, 10), CancellationToken.None);

        Assert.Null(Assert.Single(result.Items).NewValue);
    }

    [Fact]
    public async Task Handle_ForwardsTheFilterCursorLimitAndToken()
    {
        using var cts = new CancellationTokenSource();
        var bookId = Guid.NewGuid();
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(new GetBookEventsQuery(bookId, 99, null, 25), cts.Token);

        await _bookEventRepository.Received(1).GetPageAsync(bookId, 99, null, 25, cts.Token);
    }

    [Fact]
    public async Task Handle_ForwardsTheSinceCursor()
    {
        _bookEventRepository
            .GetPageAsync(Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(new GetBookEventsQuery(null, null, 42, 25), CancellationToken.None);

        await _bookEventRepository.Received(1).GetPageAsync(null, null, 42L, 25, Arg.Any<CancellationToken>());
    }
}
