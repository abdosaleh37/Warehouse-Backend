using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems;

namespace Warehouse.Api.Validators.ItemVoucher
{
    internal class ItemDataValidator : AbstractValidator<ItemData>
    {
        public ItemDataValidator()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required.");

            RuleFor(x => x.InQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("In quantity must be greater than or equal to 0.");

            RuleFor(x => x.OutQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Out quantity must be greater than or equal to 0.");

            RuleFor(x => x)
                .Must(x => x.InQuantity > 0 || x.OutQuantity > 0)
                .WithMessage("Either InQuantity or OutQuantity must be greater than zero.")
                .Must(x => !(x.InQuantity > 0 && x.OutQuantity > 0))
                .WithMessage("Cannot have both InQuantity and OutQuantity. Use only one.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes)).WithMessage("Notes must not exceed 500 characters.");
        }
    }
}
