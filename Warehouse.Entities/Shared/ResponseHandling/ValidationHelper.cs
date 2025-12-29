using FluentValidation.Results;
using System.Security.Claims;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Entities.Shared.ResponseHandling
{
    public static class ValidationHelper
    {
        public static string FlattenErrors(this IEnumerable<ValidationFailure> failures)
        {
            return string.Join("; ", failures.Select(f => f.ErrorMessage).Distinct());
        }

        public static bool TryGetUserId(this ClaimsPrincipal? user, out Guid userId)
        {
            userId = Guid.Empty;
            if (user == null)
                return false;

            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out userId))
            {
                userId = Guid.Empty;
                return false;
            }

            return true;
        }
    }
}