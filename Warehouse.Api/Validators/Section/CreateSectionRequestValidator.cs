using FluentValidation;
using Warehouse.Entities.DTO.Section.Create;

namespace Warehouse.Api.Validators.Section
{
    public class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
    {
        public CreateSectionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Section name is required.")
                .MaximumLength(100).WithMessage("Section name must not exceed 100 characters.");
        }
    }
}
