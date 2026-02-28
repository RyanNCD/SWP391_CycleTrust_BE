using FluentValidation;
using CycleTrust.Application.DTOs.Auth;

namespace CycleTrust.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x).Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Email hoặc Phone phải có ít nhất 1 trường");
        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email).EmailAddress();
        });
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrPhone).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
