using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class GetVouchersOfItemRequestValidator : AbstractValidator<GetVouchersOfItemRequest>
    {
        public GetVouchersOfItemRequestValidator()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("ItemId is required.");
        }
    }
}
