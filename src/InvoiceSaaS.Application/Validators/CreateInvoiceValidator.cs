using FluentValidation;
using InvoiceSaaS.Application.DTOs.Invoice;

namespace InvoiceSaaS.Application.Validators;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.IssueDate).NotEmpty();
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Invoice must have at least one detail line.")
            .Must(d => d.Count > 0).WithMessage("Invoice must have at least one detail line.");

        RuleForEach(x => x.Details).SetValidator(new InvoiceDetailRequestValidator());
    }
}

public class InvoiceDetailRequestValidator : AbstractValidator<InvoiceDetailRequest>
{
    public InvoiceDetailRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative.");
    }
}

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.IssueDate).NotEmpty();
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("Invoice must have at least one detail line.");
        RuleForEach(x => x.Details).SetValidator(new InvoiceDetailRequestValidator());
    }
}
