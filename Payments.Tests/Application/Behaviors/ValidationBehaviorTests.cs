using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Payments.Application.Behaviors;
using AppValidationException = BuildingBlocks.Exceptions.ValidationException;

namespace Payments.Tests.Application.Behaviors;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;

    private class PassingValidator : AbstractValidator<TestRequest>
    {
        public PassingValidator() => RuleFor(x => x.Value).NotEmpty();
    }

    private class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Value)
                .Must(_ => false)
                .WithErrorCode("Payment.Test.Error")
                .WithMessage("always fails");
        }
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNextWithoutValidating()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("ok");

        await behavior.Handle(new TestRequest("value"), next, default);

        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WithPassingValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new PassingValidator()]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("ok");

        await behavior.Handle(new TestRequest("valid"), next, default);

        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WithFailingValidators_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new FailingValidator()]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = async () => await behavior.Handle(new TestRequest("value"), next, default);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task Handle_WithFailingValidators_ExceptionContainsErrorCodes()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new FailingValidator()]);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var exception = await Assert.ThrowsAsync<AppValidationException>(
            async () => await behavior.Handle(new TestRequest("value"), next, default));

        exception.Errors.Should().ContainKey("Value");
        exception.Errors["Value"].Should().Contain("Payment.Test.Error");
    }
}
