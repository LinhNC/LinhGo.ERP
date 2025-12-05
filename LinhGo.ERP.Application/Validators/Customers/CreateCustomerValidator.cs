using FluentValidation;
using LinhGo.ERP.Application.DTOs.Customers;

namespace LinhGo.ERP.Application.Validators.Customers;

/// <summary>
/// Validator for creating a new customer
/// </summary>
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Customer code is required")
            .MaximumLength(50).WithMessage("Customer code must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0");
    }
}

