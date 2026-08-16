using FluentValidation;

namespace BookManager.Application.Books.Commands.MarkBookViewed;

public class MarkBookViewedCommandValidator : AbstractValidator<MarkBookViewedCommand>
{
    public MarkBookViewedCommandValidator()
    {
        RuleFor(command => command.BookId).NotEmpty();

        RuleFor(command => command.LastSeenEventId).GreaterThan(0);
    }
}
