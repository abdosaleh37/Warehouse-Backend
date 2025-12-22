using FluentValidation;
using Warehouse.Entities.DTO.Items.GetById;

namespace Warehouse.Api.Validators.Item
{
    public class GetByIdRequestValidator : AbstractValidator<GetByIdRequest>
    {
        public GetByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Item Id is required.");
        }
    }
}
