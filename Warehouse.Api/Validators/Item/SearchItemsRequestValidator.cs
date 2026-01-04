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
        }
    }
}
