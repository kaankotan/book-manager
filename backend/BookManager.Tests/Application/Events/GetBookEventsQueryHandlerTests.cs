using BookManager.Application.Events;
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

    private static GetBookEventsQuery Query(
        IReadOnlyList<Guid>? bookIds = null,
        IReadOnlyList<BookChangeType>? changeTypes = null,
        int page = 1,
        int pageSize = 25,
        BookEventSortField sortBy = BookEventSortField.OccurredAt,
        bool descending = true
    ) => new(bookIds ?? [], changeTypes ?? [], page, pageSize, sortBy, descending);

    private void CountReturns(int count)
    {
        _bookEventRepository
            .CountAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<IReadOnlyList<BookChangeType>>(), Arg.Any<CancellationToken>())
            .Returns(count);
    }

    private void ListReturns(params BookEvent[] events)
    {
        _bookEventRepository
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(events);
    }

    [Fact]
    public async Task Handle_ReturnsTheRequestedPageAlongsideTheTotal()
    {
        CountReturns(7);
        ListReturns(BookEventFactory.WithId(30), BookEventFactory.WithId(20));

        var result = await _handler.Handle(Query(page: 2, pageSize: 2), CancellationToken.None);

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
        CountReturns(500);
        ListReturns();

        await _handler.Handle(Query(page: page), CancellationToken.None);

        await _bookEventRepository
            .Received(1)
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                expectedSkip,
                25,
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Theory]
    [InlineData(BookEventSortField.OccurredAt, true)]
    [InlineData(BookEventSortField.OccurredAt, false)]
    [InlineData(BookEventSortField.BookTitle, true)]
    [InlineData(BookEventSortField.BookTitle, false)]
    public async Task Handle_ForwardsTheRequestedSort(BookEventSortField sortBy, bool descending)
    {
        CountReturns(10);
        ListReturns();

        await _handler.Handle(Query(sortBy: sortBy, descending: descending), CancellationToken.None);

        await _bookEventRepository
            .Received(1)
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                sortBy,
                descending,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_AppliesTheChangeTypeFilterToBothTheRowsAndTheTotal()
    {
        BookChangeType[] changeTypes = [BookChangeType.TitleChanged, BookChangeType.DescriptionChanged];
        CountReturns(10);
        ListReturns();

        await _handler.Handle(Query(changeTypes: changeTypes), CancellationToken.None);

        await _bookEventRepository.Received(1).CountAsync(Arg.Any<IReadOnlyList<Guid>>(), changeTypes, Arg.Any<CancellationToken>());
        await _bookEventRepository
            .Received(1)
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                changeTypes,
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_AppliesTheBookFilterToBothTheRowsAndTheTotal()
    {
        Guid[] bookIds = [Guid.NewGuid(), Guid.NewGuid()];
        CountReturns(10);
        ListReturns();

        await _handler.Handle(Query(bookIds: bookIds), CancellationToken.None);

        await _bookEventRepository.Received(1).CountAsync(bookIds, Arg.Any<IReadOnlyList<BookChangeType>>(), Arg.Any<CancellationToken>());
        await _bookEventRepository
            .Received(1)
            .ListAsync(
                bookIds,
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithNoFilters_PassesEmptyFilters()
    {
        CountReturns(10);
        ListReturns();

        await _handler.Handle(Query(), CancellationToken.None);

        await _bookEventRepository
            .Received(1)
            .CountAsync(
                Arg.Is<IReadOnlyList<Guid>>(ids => ids.Count == 0),
                Arg.Is<IReadOnlyList<BookChangeType>>(types => types.Count == 0),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_ForwardsTheCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CountReturns(10);
        ListReturns();

        await _handler.Handle(Query(), cts.Token);

        await _bookEventRepository
            .Received(1)
            .CountAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<IReadOnlyList<BookChangeType>>(), cts.Token);
        await _bookEventRepository
            .Received(1)
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                0,
                25,
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                cts.Token
            );
    }

    [Fact]
    public async Task Handle_WhenThePageStartsPastTheEnd_ReturnsAnEmptyPageWithoutQueryingRows()
    {
        CountReturns(30);

        var result = await _handler.Handle(Query(page: 4, pageSize: 10), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(30, result.TotalCount);
        await _bookEventRepository
            .DidNotReceive()
            .ListAsync(
                Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<IReadOnlyList<BookChangeType>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<BookEventSortField>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithAPageNumberLargeEnoughToOverflowASkip_StillReturnsAnEmptyPage()
    {
        CountReturns(30);

        var result = await _handler.Handle(Query(page: int.MaxValue, pageSize: 100), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(30, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithNoEvents_ReturnsAnEmptyPage()
    {
        CountReturns(0);

        var result = await _handler.Handle(Query(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_MapsTheChangeTypeToItsName()
    {
        var bookId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);
        CountReturns(1);
        ListReturns(BookEventFactory.WithId(7, bookId, BookChangeType.TitleChanged, "Dune Messiah", occurredAt));

        var result = await _handler.Handle(Query(pageSize: 10), CancellationToken.None);

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
        CountReturns(1);
        ListReturns(BookEventFactory.WithId(7, newValue: null));

        var result = await _handler.Handle(Query(pageSize: 10), CancellationToken.None);

        Assert.Null(Assert.Single(result.Items).NewValue);
    }
}
