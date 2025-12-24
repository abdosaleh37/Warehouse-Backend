using FluentValidation;
using Warehouse.Entities.DTO.Items.Create;

namespace Warehouse.Api.Validators.Item
{
    public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
    {
        public CreateItemRequestValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty().WithMessage("SectionId is required.");

            RuleFor(x => x.ItemCode)
                .NotEmpty().WithMessage("ItemCode is required.")
                .MaximumLength(50).WithMessage("ItemCode must not exceed 50 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(x => x.Unit)
                .IsInEnum().WithMessage("Unit must be a valid UnitOfMeasure.");

            RuleFor(x => x.OpeningQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("OpeningQuantity must be non-negative.");

            RuleFor(x => x.OpeningValue)
                .GreaterThanOrEqualTo(0).WithMessage("OpeningValue must be non-negative.");
        }
    }
}
