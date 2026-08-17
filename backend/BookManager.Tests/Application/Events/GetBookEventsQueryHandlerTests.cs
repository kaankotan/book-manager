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
    public async Task Handle_ReturnsTheRequestedPageAlongsideTheTotal()
    {
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(7);
        _bookEventRepository
            .ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(30), BookEventFactory.WithId(20)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, 2, 2), CancellationToken.None);

        Assert.Equal([30L, 20L], result.Items.Select(item => item.Id));
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(7, result.TotalCount);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 25)]
    [InlineData(4, 75)]
    public async Task Handle_TranslatesThePageNumberIntoASkip(int page, int expectedSkip)
    {
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(500);
        _bookEventRepository.ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        await _handler.Handle(new GetBookEventsQuery(null, page, 25), CancellationToken.None);

        await _bookEventRepository.Received(1).ListAsync(null, expectedSkip, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForwardsTheBookFilterAndToken()
    {
        using var cts = new CancellationTokenSource();
        var bookId = Guid.NewGuid();
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(10);
        _bookEventRepository.ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        await _handler.Handle(new GetBookEventsQuery(bookId, 1, 25), cts.Token);

        await _bookEventRepository.Received(1).CountAsync(bookId, cts.Token);
        await _bookEventRepository.Received(1).ListAsync(bookId, 0, 25, cts.Token);
    }

    [Fact]
    public async Task Handle_WhenThePageStartsPastTheEnd_ReturnsAnEmptyPageWithoutQueryingRows()
    {
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(30);

        var result = await _handler.Handle(new GetBookEventsQuery(null, 4, 10), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(30, result.TotalCount);
        await _bookEventRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAPageNumberLargeEnoughToOverflowASkip_StillReturnsAnEmptyPage()
    {
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(30);

        var result = await _handler.Handle(new GetBookEventsQuery(null, int.MaxValue, 100), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(30, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithNoEvents_ReturnsAnEmptyPage()
    {
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new GetBookEventsQuery(null, 1, 25), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_MapsTheChangeTypeToItsName()
    {
        var bookId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(1);
        _bookEventRepository
            .ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(7, bookId, BookChangeType.TitleChanged, "Dune Messiah", occurredAt)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, 1, 10), CancellationToken.None);

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
        _bookEventRepository.CountAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(1);
        _bookEventRepository
            .ListAsync(Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([BookEventFactory.WithId(7, newValue: null)]);

        var result = await _handler.Handle(new GetBookEventsQuery(null, 1, 10), CancellationToken.None);

        Assert.Null(Assert.Single(result.Items).NewValue);
    }
}
