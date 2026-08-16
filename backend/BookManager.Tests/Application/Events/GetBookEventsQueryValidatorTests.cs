using BookManager.Application.Events.Queries.GetBookEvents;

namespace BookManager.Tests.Application.Events;

public class GetBookEventsQueryValidatorTests
{
    private readonly GetBookEventsQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithALimitInsideTheAllowedRange_Passes(int limit)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, null, limit));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithALimitOutsideTheAllowedRange_Fails(int limit)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, null, limit));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.Limit));
    }

    [Fact]
    public void Validate_WithoutACursor_Passes()
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, null, 10));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAPositiveCursor_Passes()
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, 1, 10));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositiveCursor_Fails(long before)
    {
        var result = _validator.Validate(new GetBookEventsQuery(null, before, 10));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBookEventsQuery.Before));
    }

    [Fact]
    public void Validate_WithABookIdFilter_Passes()
    {
        var result = _validator.Validate(new GetBookEventsQuery(Guid.NewGuid(), null, 10));

        Assert.True(result.IsValid);
    }
}
