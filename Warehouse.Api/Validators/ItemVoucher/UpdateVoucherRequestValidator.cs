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

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");

            RuleFor(x => x.VoucherDate)
                .NotEmpty().WithMessage("Voucher date is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Voucher date cannot be in the future.");
        }
    }
}
