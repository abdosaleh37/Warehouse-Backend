using FluentValidation;
using Warehouse.Entities.DTO.Section.GetAll;

namespace Warehouse.Api.Validators.Section
{
    public class GetAllSectionsRequestValidator : AbstractValidator<GetAllSectionsRequest>
    {
        public GetAllSectionsRequestValidator() 
        { 

        }
    }
}
