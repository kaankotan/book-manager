using BookManager.Application.Books.Commands.UpdateBook;
using BookManager.Domain.Entities;

namespace BookManager.Tests.Application.Books.Commands;

public class UpdateBookCommandValidatorTests
{
    private readonly UpdateBookCommandValidator _validator = new();

    private static UpdateBookCommand Valid(Guid? id = null, string title = "Dune", string description = "A desert epic") =>
        new(id ?? Guid.NewGuid(), title, description);

    [Fact]
    public void Validate_WithAWellFormedCommand_Passes()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAnEmptyId_Fails()
    {
        var result = _validator.Validate(Valid(id: Guid.Empty));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithABlankTitle_Fails(string title)
    {
        var result = _validator.Validate(Valid(title: title));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Title));
    }

    [Fact]
    public void Validate_WithATitleAtTheMaximumLength_Passes()
    {
        var result = _validator.Validate(Valid(title: new string('a', Book.TitleMaxLength)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithATitleOverTheMaximumLength_Fails()
    {
        var result = _validator.Validate(Valid(title: new string('a', Book.TitleMaxLength + 1)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Title));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithABlankDescription_Fails(string description)
    {
        var result = _validator.Validate(Valid(description: description));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Description));
    }

    [Fact]
    public void Validate_WithADescriptionAtTheMaximumLength_Passes()
    {
        var result = _validator.Validate(Valid(description: new string('a', Book.DescriptionMaxLength)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithADescriptionOverTheMaximumLength_Fails()
    {
        var result = _validator.Validate(Valid(description: new string('a', Book.DescriptionMaxLength + 1)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Description));
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = _validator.Validate(new UpdateBookCommand(Guid.Empty, "", ""));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Id));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Title));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateBookCommand.Description));
    }
}
