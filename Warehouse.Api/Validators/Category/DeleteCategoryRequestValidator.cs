using FluentValidation;
using Warehouse.Entities.DTO.Category.Delete;

namespace Warehouse.Api.Validators.Category
{
    public class DeleteCategoryRequestValidator : AbstractValidator<DeleteCategoryRequest>
    {
        public DeleteCategoryRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category Id is required.");
        }
    }
}
