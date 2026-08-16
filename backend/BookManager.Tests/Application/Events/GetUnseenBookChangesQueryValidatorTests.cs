using BookManager.Application.Events.Queries.GetUnseenBookChanges;

namespace BookManager.Tests.Application.Events;

public class GetUnseenBookChangesQueryValidatorTests
{
    private readonly GetUnseenBookChangesQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithALimitInsideTheAllowedRange_Passes(int limit)
    {
        var result = _validator.Validate(new GetUnseenBookChangesQuery(Guid.NewGuid(), limit));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithALimitOutsideTheAllowedRange_Fails(int limit)
    {
        var result = _validator.Validate(new GetUnseenBookChangesQuery(Guid.NewGuid(), limit));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetUnseenBookChangesQuery.Limit));
    }

    [Fact]
    public void Validate_WithoutABookId_Fails()
    {
        var result = _validator.Validate(new GetUnseenBookChangesQuery(Guid.Empty, 50));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetUnseenBookChangesQuery.BookId));
    }
}
