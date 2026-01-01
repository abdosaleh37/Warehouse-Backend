using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.Delete;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.DTO.Category.GetById;
using Warehouse.Entities.DTO.Category.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Response<GetAllCategoriesResponse>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken);

        Task<Response<GetCategoryByIdResponse>> GetByIdAsync(
            Guid id,
            GetCategoryByIdRequest request,
            CancellationToken cancellationToken);

        Task<Response<CreateCategoryResponse>> CreateCategoryAsync(
            Guid userId,
            CreateCategoryRequest request,
            CancellationToken cancellationToken);

        Task<Response<UpdateCategoryResponse>> UpdateCategoryAsync(
            Guid userId,
            UpdateCategoryRequest request,
            CancellationToken cancellationToken);

        Task<Response<DeleteCategoryResponse>> DeleteCategoryAsync(
            Guid userId,
            DeleteCategoryRequest request,
            CancellationToken cancellationToken);
    }
}
