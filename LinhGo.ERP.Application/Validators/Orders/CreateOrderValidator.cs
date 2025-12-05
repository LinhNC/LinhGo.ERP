using FluentValidation;
using LinhGo.ERP.Application.DTOs.Orders;

namespace LinhGo.ERP.Application.Validators.Orders;

/// <summary>
/// Validator for creating a new order
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");
    }
}

