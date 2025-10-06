using FluentValidation;

namespace CoreApp.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Request.Email)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.Request.Password)
            .NotEmpty().MinimumLength(6);
    }
}
