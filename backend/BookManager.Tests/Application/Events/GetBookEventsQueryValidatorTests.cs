using BookManager.Application.Events;
using BookManager.Application.Events.Queries.GetBookEvents;
using BookManager.Domain.Entities;

namespace BookManager.Tests.Application.Events;

public class GetBookEventsQueryValidatorTests
{
    private readonly GetBookEventsQueryValidator _validator = new();

    private static GetBookEventsQuery Query(
        IReadOnlyList<Guid>? bookIds = null,
        IReadOnlyList<BookChangeType>? changeTypes = null,
        int page = 1,
        int pageSize = 25,
        BookEventSortField sortBy = BookEventSortField.OccurredAt,
        bool descending = true
    ) => new(bookIds ?? [], changeTypes ?? [], page, pageSize, sortBy, descending);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithAPageSizeInsideTheAllowedRange_Passes(int pageSize)
    {
        var result = _validator.Validate(Query(pageSize: pageSize));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithAPageSizeOutsideTheAllowedRange_Fails(int pageSize)
    {
        var result = _validator.Validate(Query(pageSize: pageSize));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.PageSize));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void Validate_WithAPositivePageNumber_Passes(int page)
    {
        var result = _validator.Validate(Query(page: page));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositivePageNumber_Fails(int page)
    {
        var result = _validator.Validate(Query(page: page));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.Page));
    }

    [Theory]
    [InlineData(BookEventSortField.OccurredAt)]
    [InlineData(BookEventSortField.BookTitle)]
    public void Validate_WithAKnownSortField_Passes(BookEventSortField sortBy)
    {
        var result = _validator.Validate(Query(sortBy: sortBy));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithASortFieldOutsideTheEnum_Fails()
    {
        var result = _validator.Validate(Query(sortBy: (BookEventSortField)99));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.SortBy));
    }

    [Fact]
    public void Validate_WithNoChangeTypeFilter_Passes()
    {
        var result = _validator.Validate(Query(changeTypes: []));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithKnownChangeTypes_Passes()
    {
        var result = _validator.Validate(Query(changeTypes: [BookChangeType.Created, BookChangeType.DescriptionChanged]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAChangeTypeOutsideTheEnum_Fails()
    {
        var result = _validator.Validate(Query(changeTypes: [BookChangeType.Created, (BookChangeType)99]));

        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(GetBookEventsQuery.ChangeTypes)));
    }

    [Fact]
    public void Validate_WithNoBookFilter_Passes()
    {
        var result = _validator.Validate(Query(bookIds: []));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAnEmptyBookId_Fails()
    {
        var result = _validator.Validate(Query(bookIds: [Guid.NewGuid(), Guid.Empty]));

        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(GetBookEventsQuery.BookIds)));
    }

    [Fact]
    public void Validate_WithABookIdFilter_Passes()
    {
        var result = _validator.Validate(Query(bookIds: [Guid.NewGuid(), Guid.NewGuid()]));

        Assert.True(result.IsValid);
    }
}
