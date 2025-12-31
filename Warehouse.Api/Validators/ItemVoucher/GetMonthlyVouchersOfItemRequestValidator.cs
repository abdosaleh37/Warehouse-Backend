using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class GetMonthlyVouchersOfItemRequestValidator : AbstractValidator<GetMonthlyVouchersOfItemRequest>
    {
        public GetMonthlyVouchersOfItemRequestValidator()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("ItemId is required.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        }
    }
}
