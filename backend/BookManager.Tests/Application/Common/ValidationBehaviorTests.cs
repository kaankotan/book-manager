using BookManager.Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using ValidationException = BookManager.Application.Common.Exceptions.ValidationException;

namespace BookManager.Tests.Application.Common;

public class ValidationBehaviorTests
{
    public record TestRequest(string Name) : IRequest<string>;

    private const string HandlerResponse = "handled";

    private static ValidationBehavior<TestRequest, string> Behavior(params IValidator<TestRequest>[] validators) => new(validators);

    private static IValidator<TestRequest> ValidatorReturning(params ValidationFailure[] failures)
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        return validator;
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsTheNextHandler()
    {
        var called = false;

        var result = await Behavior()
            .Handle(
                new TestRequest("Dune"),
                _ =>
                {
                    called = true;
                    return Task.FromResult(HandlerResponse);
                },
                CancellationToken.None
            );

        Assert.True(called);
        Assert.Equal(HandlerResponse, result);
    }

    [Fact]
    public async Task Handle_WhenEveryValidatorPasses_CallsTheNextHandler()
    {
        var behavior = Behavior(ValidatorReturning(), ValidatorReturning());

        var result = await behavior.Handle(new TestRequest("Dune"), _ => Task.FromResult(HandlerResponse), CancellationToken.None);

        Assert.Equal(HandlerResponse, result);
    }

    [Fact]
    public async Task Handle_ForwardsTheCancellationTokenToValidatorsAndTheNextHandler()
    {
        using var cts = new CancellationTokenSource();
        var validator = ValidatorReturning();
        CancellationToken forwarded = default;

        await Behavior(validator)
            .Handle(
                new TestRequest("Dune"),
                token =>
                {
                    forwarded = token;
                    return Task.FromResult(HandlerResponse);
                },
                cts.Token
            );

        Assert.Equal(cts.Token, forwarded);
        await validator.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), cts.Token);
    }

    [Fact]
    public async Task Handle_WithAFailure_ThrowsValidationException()
    {
        var behavior = Behavior(ValidatorReturning(new ValidationFailure("Name", "'Name' must not be empty.")));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), _ => Task.FromResult(HandlerResponse), CancellationToken.None)
        );

        var messages = Assert.Contains("Name", exception.Errors);
        Assert.Equal(["'Name' must not be empty."], messages);
    }

    [Fact]
    public async Task Handle_WithAFailure_DoesNotCallTheNextHandler()
    {
        var behavior = Behavior(ValidatorReturning(new ValidationFailure("Name", "'Name' must not be empty.")));
        var called = false;

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(""),
                _ =>
                {
                    called = true;
                    return Task.FromResult(HandlerResponse);
                },
                CancellationToken.None
            )
        );

        Assert.False(called);
    }

    [Fact]
    public async Task Handle_GroupsFailuresByProperty()
    {
        var behavior = Behavior(
            ValidatorReturning(
                new ValidationFailure("Name", "must not be empty"),
                new ValidationFailure("Name", "must be shorter"),
                new ValidationFailure("Id", "must not be empty")
            )
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), _ => Task.FromResult(HandlerResponse), CancellationToken.None)
        );

        Assert.Equal(2, exception.Errors.Count);
        Assert.Equal(["must not be empty", "must be shorter"], exception.Errors["Name"]);
        Assert.Equal(["must not be empty"], exception.Errors["Id"]);
    }

    [Fact]
    public async Task Handle_CollapsesDuplicateMessagesForTheSameProperty()
    {
        var behavior = Behavior(
            ValidatorReturning(new ValidationFailure("Name", "must not be empty")),
            ValidatorReturning(new ValidationFailure("Name", "must not be empty"))
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), _ => Task.FromResult(HandlerResponse), CancellationToken.None)
        );

        Assert.Equal(["must not be empty"], exception.Errors["Name"]);
    }

    [Fact]
    public async Task Handle_CombinesFailuresFromEveryValidator()
    {
        var behavior = Behavior(
            ValidatorReturning(new ValidationFailure("Name", "must not be empty")),
            ValidatorReturning(new ValidationFailure("Id", "must not be empty"))
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), _ => Task.FromResult(HandlerResponse), CancellationToken.None)
        );

        Assert.Equal(["Id", "Name"], exception.Errors.Keys.Order());
    }

    [Fact]
    public async Task Handle_WhenOnlyOneOfSeveralValidatorsFails_StillThrows()
    {
        var behavior = Behavior(ValidatorReturning(), ValidatorReturning(new ValidationFailure("Name", "bad")));

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), _ => Task.FromResult(HandlerResponse), CancellationToken.None)
        );
    }
}
