using BookManager.Domain.Entities;
using FluentValidation;

namespace BookManager.Application.Books.Commands.AddBook;

public class AddBookCommandValidator : AbstractValidator<AddBookCommand>
{
    public AddBookCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(Book.TitleMaxLength);

        RuleFor(command => command.Description).NotEmpty().MaximumLength(Book.DescriptionMaxLength);

        RuleFor(command => command.PublishedDate).NotEmpty();

        RuleFor(command => command.AuthorIds).NotNull();

        RuleForEach(command => command.AuthorIds).NotEmpty();
    }
}
