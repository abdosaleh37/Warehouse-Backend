using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Response<GetAllCategoriesResponse>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken);

        Task<Response<CreateCategoryResponse>> CreateAsync(
            Guid userId,
            CreateCategoryRequest request, 
            CancellationToken cancellationToken);
    }
}
