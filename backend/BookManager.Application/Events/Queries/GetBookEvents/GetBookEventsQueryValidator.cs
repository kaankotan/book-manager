using FluentValidation;

namespace BookManager.Application.Events.Queries.GetBookEvents;

public class GetBookEventsQueryValidator : AbstractValidator<GetBookEventsQuery>
{
    public GetBookEventsQueryValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 100);

        RuleFor(query => query.Before).GreaterThan(0).When(query => query.Before is not null);

        RuleFor(query => query.Since).GreaterThan(0).When(query => query.Since is not null);
    }
}
