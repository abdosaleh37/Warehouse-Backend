using FluentValidation;
using Warehouse.Entities.DTO.Section.GetById;

namespace Warehouse.Api.Validators.Section
{
    public class GetSectionByIdRequestValidator : AbstractValidator<GetSectionByIdRequest>
    {
        public GetSectionByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Section Id must not be empty.");
        }
    }
}
