using FluentValidation;
using Warehouse.Entities.DTO.Items.GetById;

namespace Warehouse.Api.Validators.Item
{
    public class GetItemByIdRequestValidator : AbstractValidator<GetItemByIdRequest>
    {
        public GetItemByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Item Id is required.");
        }
    }
}
