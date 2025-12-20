using FluentValidation.Results;

namespace Warehouse.Entities.Shared.ResponseHandling
{
    public static class ValidationHelper
    {
        public static string FlattenErrors(IEnumerable<ValidationFailure> failures)
        {
            return string.Join("; ", failures.Select(f => f.ErrorMessage).Distinct());
        }
    }
}
