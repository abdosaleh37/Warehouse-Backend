using FluentValidation;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;

namespace Warehouse.Api.Validators.Item
{
    public class GetItemsWithVouchersOfMonthRequestValidator : AbstractValidator<GetItemsWithVouchersOfMonthRequest>
    {
        public GetItemsWithVouchersOfMonthRequestValidator()
        {
            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12)
                .WithMessage("Month must be between 1 and 12.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100)
                .WithMessage("Year must be between 2000 and 2100.");
        }
    }
}
