using BookManager.Application.Books.Commands.AddBook;
using BookManager.Domain.Entities;

namespace BookManager.Tests.Application.Books.Commands;

public class AddBookCommandValidatorTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly AddBookCommandValidator _validator = new();

    private static AddBookCommand Valid(
        string title = "Dune",
        string description = "A desert epic",
        DateOnly? publishedDate = null,
        IReadOnlyList<Guid>? authorIds = null
    ) => new(title, description, publishedDate ?? PublishedDate, authorIds ?? [Guid.NewGuid()]);

    [Fact]
    public void Validate_WithAWellFormedCommand_Passes()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNoAuthors_Passes()
    {
        var result = _validator.Validate(Valid(authorIds: []));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithABlankTitle_Fails(string title)
    {
        var result = _validator.Validate(Valid(title: title));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Title));
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

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Title));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithABlankDescription_Fails(string description)
    {
        var result = _validator.Validate(Valid(description: description));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Description));
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

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Description));
    }

    [Fact]
    public void Validate_WithADefaultPublishedDate_Fails()
    {
        var result = _validator.Validate(Valid(publishedDate: default(DateOnly)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.PublishedDate));
    }

    [Fact]
    public void Validate_WithNullAuthorIds_Fails()
    {
        var result = _validator.Validate(new AddBookCommand("Dune", "A desert epic", PublishedDate, null!));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.AuthorIds));
    }

    [Fact]
    public void Validate_WithAnEmptyAuthorId_Fails()
    {
        var result = _validator.Validate(Valid(authorIds: [Guid.Empty]));

        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(AddBookCommand.AuthorIds)));
    }

    [Fact]
    public void Validate_ReportsEveryBrokenRuleAtOnce()
    {
        var result = _validator.Validate(new AddBookCommand("", "", default, [Guid.Empty]));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Title));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.Description));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AddBookCommand.PublishedDate));
    }
}
