using BookManager.Application.Events.Queries.GetBookEvents;

namespace BookManager.Tests.Application.Events;

public class GetBookEventsQueryValidatorTests
{
    private readonly GetBookEventsQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithAPageSizeInsideTheAllowedRange_Passes(int pageSize)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, 1, pageSize));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithAPageSizeOutsideTheAllowedRange_Fails(int pageSize)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, 1, pageSize));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.PageSize));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void Validate_WithAPositivePageNumber_Passes(int page)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, page, 10));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositivePageNumber_Fails(int page)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, page, 10));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.Page));
    }

    [Fact]
    public void Validate_WithABookIdFilter_Passes()
    {
        var result = _validator.Validate(new GetBookEventsQuery(Guid.NewGuid(), 1, 10));

        Assert.True(result.IsValid);
    }
}
