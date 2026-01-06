using FluentValidation;
using Warehouse.Entities.DTO.Items.Search;

namespace Warehouse.Api.Validators.Item
{
    public class SearchItemsRequestValidator : AbstractValidator<SearchItemsRequest>
    {
        public SearchItemsRequestValidator()
        {
            RuleFor(x => x.SearchTerm)
                .NotEmpty().WithMessage("Search term must not be empty.")
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        }
    }
}
