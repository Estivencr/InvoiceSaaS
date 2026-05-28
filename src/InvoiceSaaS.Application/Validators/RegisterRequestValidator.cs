using FluentValidation;
using InvoiceSaaS.Application.DTOs.Auth;

namespace InvoiceSaaS.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(255);

        RuleFor(x => x.CompanyEmail)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.AdminEmail)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");

        RuleFor(x => x.AdminFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminLastName).NotEmpty().MaximumLength(100);
    }
}
