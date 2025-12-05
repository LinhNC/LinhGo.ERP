using FluentValidation;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Companies;

namespace LinhGo.ERP.Application.Validators.Companies;

/// <summary>
/// Validator for creating a new company
/// </summary>
public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Company code is required")
            .WithErrorCode(CompanyErrors.NameRequired)
            .MaximumLength(50).WithMessage("Company code must not exceed 50 characters")
            .WithErrorCode(CompanyErrors.NameTooLong);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .WithErrorCode(CompanyErrors.NameRequired)
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters")
            .WithErrorCode(CompanyErrors.NameTooLong);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .WithErrorCode(UserErrors.EmailInvalid)
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Website)
            .MaximumLength(255).WithMessage("Website must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.AddressLine1)
            .MaximumLength(255).WithMessage("Address must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.AddressLine1));

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code (e.g., USD, EUR)");
    }
}

