using FluentValidation;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;

namespace Warehouse.Api.Validators.Item
{
    public class GetItemsOfSectionRequestValidator : AbstractValidator<GetItemsOfSectionRequest>
    {
        public GetItemsOfSectionRequestValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty().WithMessage("Section Id must not be empty.");
        }
    }
}
