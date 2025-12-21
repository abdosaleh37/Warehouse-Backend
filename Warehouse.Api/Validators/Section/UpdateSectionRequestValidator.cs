using FluentValidation;
using Warehouse.Entities.DTO.Section.Update;

namespace Warehouse.Api.Validators.Section
{
    public class UpdateSectionRequestValidator : AbstractValidator<UpdateSectionRequest>
    {
        public UpdateSectionRequestValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Section Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Section name is required.")
                .MaximumLength(100).WithMessage("Section name must not exceed 100 characters.");
        }
    }
}
