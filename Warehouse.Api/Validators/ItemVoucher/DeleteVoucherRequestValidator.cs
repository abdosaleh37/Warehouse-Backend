using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.Delete;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class DeleteVoucherRequestValidator : AbstractValidator<DeleteVoucherRequest>
    {
        public DeleteVoucherRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Voucher ID is required.");
        }
    }
}
