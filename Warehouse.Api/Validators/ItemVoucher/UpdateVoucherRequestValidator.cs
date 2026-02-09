using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.Update;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class UpdateVoucherRequestValidator : AbstractValidator<UpdateVoucherRequest>
    {
        public UpdateVoucherRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Voucher ID is required.");

            RuleFor(x => x.VoucherCode)
                .NotEmpty().WithMessage("Voucher code is required.")
                .MaximumLength(50).WithMessage("Voucher code must not exceed 50 characters.");

            RuleFor(x => x.InQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("In quantity must be greater than or equal to 0.");

            RuleFor(x => x.OutQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Out quantity must be greater than or equal to 0.");

            RuleFor(x => x)
                .Must(x => x.InQuantity > 0 || x.OutQuantity > 0)
                .WithMessage("Either InQuantity or OutQuantity must be greater than zero.")
                .Must(x => !(x.InQuantity > 0 && x.OutQuantity > 0))
                .WithMessage("Cannot have both InQuantity and OutQuantity. Use only one.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");

            RuleFor(x => x.VoucherDate)
                .NotEmpty().WithMessage("Voucher date is required.")
                .Must(d => d.Kind == DateTimeKind.Utc).WithMessage("Voucher date must be in UTC.")
                .Must(d => d.Date <= DateTime.UtcNow.AddHours(14).Date).WithMessage("Voucher date cannot be in the future.");
        }
    }
}
