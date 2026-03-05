using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;
using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Api.Validators.ItemVoucher;

public class ExportVouchersRequestValidator : AbstractValidator<ExportVouchersRequest>
{
    public ExportVouchersRequestValidator()
    {
        RuleFor(x => x.VoucherType)
            .IsInEnum()
            .WithMessage("Voucher type must be either 'In' or 'Out'");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue)
            .WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.Year)
            .GreaterThan(2000)
            .LessThanOrEqualTo(2100)
            .When(x => x.Year.HasValue)
            .WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x)
            .Must(x => (x.Month.HasValue && x.Year.HasValue) || (!x.Month.HasValue && !x.Year.HasValue))
            .WithMessage("Both Month and Year must be provided together or omitted together");
    }
}
