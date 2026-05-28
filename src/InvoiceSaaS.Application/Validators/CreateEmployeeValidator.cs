using FluentValidation;
using InvoiceSaaS.Application.DTOs.Employee;

namespace InvoiceSaaS.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Position).MaximumLength(100).When(x => x.Position != null);
    }
}

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Status).Must(s => s is "active" or "inactive");
    }
}
