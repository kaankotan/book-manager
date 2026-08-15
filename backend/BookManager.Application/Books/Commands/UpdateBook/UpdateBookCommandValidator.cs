using BookManager.Domain.Entities;
using FluentValidation;

namespace BookManager.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();

        RuleFor(command => command.Title).NotEmpty().MaximumLength(Book.TitleMaxLength);

        RuleFor(command => command.Description).NotEmpty().MaximumLength(Book.DescriptionMaxLength);
    }
}
