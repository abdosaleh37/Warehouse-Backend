using FluentValidation;
using Warehouse.Entities.DTO.ItemVoucher.GetById;

namespace Warehouse.Api.Validators.ItemVoucher
{
    public class GetVoucherByIdRequestValidator : AbstractValidator<GetVoucherByIdRequest>
    {
        public GetVoucherByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Voucher Id is required.");
        }
    }
}
