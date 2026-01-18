using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.Delete;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.DTO.Category.GetById;
using Warehouse.Entities.DTO.Category.Update;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly IMapper _mapper;
        private readonly WarehouseDbContext _context;
        private readonly ResponseHandler _responseHandler;

        public CategoryService(
            ILogger<CategoryService> logger,
            IMapper mapper,
            WarehouseDbContext context,
            ResponseHandler responseHandler)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _responseHandler = responseHandler;
        }

        public async Task<Response<GetAllCategoriesResponse>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all categories of user: {UserId}", userId);

            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (warehouse == null)
            {
                _logger.LogWarning("User has no warehouse.");
                return _responseHandler.NotFound<GetAllCategoriesResponse>("User has no warehouse.");
            }

            var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.WarehouseId == warehouse.Id)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                Category = c,
                SectionCount = c.Sections.Count
            })
            .ToListAsync(cancellationToken);

            if (categories.Count == 0)
            {
                _logger.LogInformation("No categories found for user: {UserId}", userId);
                return _responseHandler.Success(new GetAllCategoriesResponse
                {
                    Categories = new List<GetAllCategoriesResult>(),
                    TotalCount = 0,
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCreatedAt = DateTime.SpecifyKind(warehouse.CreatedAt, DateTimeKind.Utc)
                }, "No categories found.");
            }

            var categoriesResult = categories.Select(x =>
            {
                var mapped = _mapper.Map<GetAllCategoriesResult>(x.Category);
                mapped.SectionCount = x.SectionCount;
                mapped.CreatedAt = DateTime.SpecifyKind(mapped.CreatedAt, DateTimeKind.Utc);
                return mapped;
            }).ToList();

            var response = new GetAllCategoriesResponse
            {
                Categories = categoriesResult,
                TotalCount = categoriesResult.Count,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                WarehouseCreatedAt = DateTime.SpecifyKind(warehouse.CreatedAt, DateTimeKind.Utc)
            };

            _logger.LogInformation("Categories retreived successfully.");
            return _responseHandler.Success(response, "Categories retreived successfully.");
        }

        public async Task<Response<GetCategoryByIdResponse>> GetByIdAsync(
            Guid userId,
            GetCategoryByIdRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting information of category: {CategoryId}", request.Id);

            var categoryResult = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == request.Id && c.Warehouse.UserId == userId)
                .Select(c => new
                {
                    Category = c,
                    Warehouse = c.Warehouse,
                    SectionCount = c.Sections.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (categoryResult == null)
            {
                _logger.LogWarning("Category: {CategoryId} not found.", request.Id);
                return _responseHandler.NotFound<GetCategoryByIdResponse>("Category not found.");
            }

            var mapped = _mapper.Map<GetCategoryByIdResponse>(categoryResult.Category);
            mapped.SectionCount = categoryResult.SectionCount;
            mapped.WarehouseId = categoryResult.Category.WarehouseId;
            mapped.CreatedAt = DateTime.SpecifyKind(mapped.CreatedAt, DateTimeKind.Utc);

            if (categoryResult.Warehouse != null)
            {
                mapped.WarehouseName = categoryResult.Warehouse.Name;
            }

            _logger.LogInformation("Category: {CategoryId} information retreived successfully.", categoryResult.Category.Id);
            return _responseHandler.Success(mapped, "Category retreived successfully.");
        }

        public async Task<Response<CreateCategoryResponse>> CreateCategoryAsync(
            Guid userId,
            CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new category with name: {CategoryName} for User: {UserId}", request.Name, userId);

            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (warehouse == null)
            {
                _logger.LogWarning("User has no warehouse.");
                return _responseHandler.NotFound<CreateCategoryResponse>("User has no warehouse.");
            }

            var categoryEntity = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                WarehouseId = warehouse.Id
            };

            try
            {
                _context.Categories.Add(categoryEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category.");
                return _responseHandler.InternalServerError<CreateCategoryResponse>(
                    "An error occurred while creating the category.");
            }

            var response = _mapper.Map<CreateCategoryResponse>(categoryEntity);
            response.CreatedAt = DateTime.SpecifyKind(response.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Category created successfully with name: {CategoryName}", request.Name);
            return _responseHandler.Success(response, "Category created successfully.");
        }

        public async Task<Response<UpdateCategoryResponse>> UpdateCategoryAsync(
            Guid userId,
            UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category: {CategoryId} with new name: {Name}", request.Id, request.Name);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.Warehouse.UserId == userId, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category: {CategoryId} not found.", request.Id);
                return _responseHandler.NotFound<UpdateCategoryResponse>("Category not found");
            }

            category.Name = request.Name;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the category.");
                return _responseHandler.InternalServerError<UpdateCategoryResponse>(
                    "An error occurred while updating the category.");
            }

            var response = _mapper.Map<UpdateCategoryResponse>(category);
            response.CreatedAt = DateTime.SpecifyKind(response.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Category updated successfully with name: {CategoryName}", request.Name);
            return _responseHandler.Success(response, "Category updated successfully.");
        }

        public async Task<Response<DeleteCategoryResponse>> DeleteCategoryAsync(
            Guid userId,
            DeleteCategoryRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting category: {CategoryId}", request.Id);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.Warehouse.UserId == userId, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category: {CategoryId} not found.", request.Id);
                return _responseHandler.NotFound<DeleteCategoryResponse>("Category not found.");
            }

            var hasSections = await _context.Sections
                .AsNoTracking()
                .AnyAsync(s => s.CategoryId == category.Id, cancellationToken);

            if (hasSections)
            {
                _logger.LogWarning("Category: {CategoryId} has associated sections and cannot be deleted.", request.Id);
                return _responseHandler.BadRequest<DeleteCategoryResponse>(
                    "Category cannot be deleted because it has associated sections.");
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the category.");
                return _responseHandler.InternalServerError<DeleteCategoryResponse>(
                    "An error occurred while deleting the category.");
            }

            var response = new DeleteCategoryResponse { Id = request.Id };

            _logger.LogInformation("Category: {CategoryId} deleted successfully.", request.Id);
            return _responseHandler.Success(response, "Category deleted successfully.");
        }
    }
}
