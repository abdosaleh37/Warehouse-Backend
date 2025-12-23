using FluentValidation;
using Warehouse.Entities.DTO.Items.Delete;

namespace Warehouse.Api.Validators.Item
{
    public class DeleteItemRequestValidator : AbstractValidator<DeleteItemRequest>
    {
        public DeleteItemRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Item Id is required.");
        }
    }
}
