using FluentValidation.Results;
using System.Security.Claims;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Entities.Shared.ResponseHandling
{
    public static class ValidationHelper
    {
        public static string FlattenErrors(IEnumerable<ValidationFailure> failures)
        {
            return string.Join("; ", failures.Select(f => f.ErrorMessage).Distinct());
        }

        //public static Guid? GetUserId(this ClaimsPrincipal userClaims)
        //{
        //    var userIdString = userClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //    return string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId);
        //}
    }
}


