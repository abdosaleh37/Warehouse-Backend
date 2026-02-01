using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class CreateVoucherWithManyItemsRequestValidator : AbstractValidator<CreateVoucherWithManyItemsRequest>
    {
        public CreateVoucherWithManyItemsRequestValidator()
        {
            RuleFor(x => x.VoucherCode)
                .NotEmpty().WithMessage("Voucher code is required.")
                .MaximumLength(50).WithMessage("Voucher code must not exceed 50 characters.");

            RuleFor(x => x.VoucherDate)
                .NotEmpty().WithMessage("Voucher date is required.")
                .Must(d => d.Kind == DateTimeKind.Utc).WithMessage("Voucher date must be in UTC.")
                .Must(d => d.Date <= DateTime.UtcNow.AddHours(14).Date).WithMessage("Voucher date cannot be in the future.");


            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required.")
                .Must(items => items != null && items.Count > 0).WithMessage("Items list cannot be empty.");

            // Ensure no duplicate ItemId in the request
            RuleFor(x => x.Items)
                .Must(items => items == null || items.Select(i => i.ItemId).Distinct().Count() == items.Count)
                .WithMessage("Duplicate items are not allowed in the request. Combine entries for the same item.");

            RuleForEach(x => x.Items).SetValidator(new ItemDataValidator());
        }
    }
}
