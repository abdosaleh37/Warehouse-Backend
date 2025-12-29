using FluentValidation;
using Warehouse.Entities.DTO.Section.GetSectionsOfCategory;

namespace Warehouse.Api.Validators.Section
{
    public class GetSectionsOfCategoryRequestValidator : AbstractValidator<GetSectionsOfCategoryRequest>
    {
        public GetSectionsOfCategoryRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required");
        }
    }
}
