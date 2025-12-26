using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Response<CreateCategoryResponse>> CreateAsync(
            CreateCategoryRequest request, 
            CancellationToken cancellationToken);
    }
}
