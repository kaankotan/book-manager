using FluentValidation;

namespace BookManager.Application.Events.Queries.GetUnseenBookChanges;

public class GetUnseenBookChangesQueryValidator : AbstractValidator<GetUnseenBookChangesQuery>
{
    public GetUnseenBookChangesQueryValidator()
    {
        RuleFor(query => query.BookId).NotEmpty();

        RuleFor(query => query.Limit).InclusiveBetween(1, 100);
    }
}
