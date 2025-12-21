using FluentValidation;
using Warehouse.Entities.DTO.Section.Delete;

namespace Warehouse.Api.Validators.Section
{
    public class DeleteSectionRequestValidator : AbstractValidator<DeleteSectionRequest>
    {
        public DeleteSectionRequestValidator() 
        {
            RuleFor(x => x.SectionId)
                .NotEmpty().WithMessage("Section Id is required");
        }
    }
}
