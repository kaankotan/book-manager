using FluentValidation;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public class GetBookEventsQueryValidator : AbstractValidator<GetBookEventsQuery>
{
    public GetBookEventsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);

        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);

        RuleFor(query => query.SortBy).IsInEnum();

        RuleFor(query => query.ChangeTypes).NotNull();

        RuleForEach(query => query.ChangeTypes).IsInEnum();

        RuleFor(query => query.BookIds).NotNull();

        RuleForEach(query => query.BookIds).NotEmpty();
    }
}
