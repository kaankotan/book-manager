using BookManager.Application.Books.Commands.MarkBookViewed;

namespace BookManager.Tests.Application.Books.Commands;

public class MarkBookViewedCommandValidatorTests
{
    private readonly MarkBookViewedCommandValidator _validator = new();

    [Fact]
    public void Validate_WithABookIdAndAPositiveEventId_Passes()
    {
        var result = _validator.Validate(new MarkBookViewedCommand(Guid.NewGuid(), 1));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutABookId_Fails()
    {
        var result = _validator.Validate(new MarkBookViewedCommand(Guid.Empty, 1));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MarkBookViewedCommand.BookId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositiveEventId_Fails(long lastSeenEventId)
    {
        var result = _validator.Validate(new MarkBookViewedCommand(Guid.NewGuid(), lastSeenEventId));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MarkBookViewedCommand.LastSeenEventId));
    }
}
