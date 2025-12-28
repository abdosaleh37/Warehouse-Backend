using FluentValidation;
using Warehouse.Entities.DTO.Category.GetById;

namespace Warehouse.Api.Validators.Category
{
    public class GetCategoryByIdRequestValidator : AbstractValidator<GetCategoryByIdRequest>
    {
        public GetCategoryByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category Id is required");
        }
    }
}
