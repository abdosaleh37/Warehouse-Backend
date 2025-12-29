using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Category.Create;
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

            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Warehouse)
                .Where(c => c.Warehouse.UserId == userId)
                .ToListAsync(cancellationToken);

            if (categories.Count == 0)
            {
                _logger.LogInformation("No categories found for user: {UserId}", userId);
                return _responseHandler.Success(new GetAllCategoriesResponse
                {
                    Categories = new List<GetAllCategoriesResult>(),
                    TotalCount = 0
                }, "No categories found.");
            }

            var categoriesResult = _mapper.Map<List<GetAllCategoriesResult>>(categories);
            var response = new GetAllCategoriesResponse
            {
                Categories = categoriesResult,
                TotalCount = categoriesResult.Count
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

            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Warehouse)
                .Include(c => c.Sections)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.Warehouse.UserId == userId, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category: {CategoryId} not found.", request.Id);
                return _responseHandler.NotFound<GetCategoryByIdResponse>("Category not found.");
            }

            var response = _mapper.Map<GetCategoryByIdResponse>(category);

            _logger.LogInformation("Category: {CategoryId} information retreived successfully.", category.Id);
            return _responseHandler.Success(response, "Category retreived successfully.");
        }

        public async Task<Response<CreateCategoryResponse>> CreateAsync(
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

            _logger.LogInformation("Category created successfully with name: {CategoryName}", request.Name);
            return _responseHandler.Success(response, "Category created successfully.");
        }

        public async Task<Response<UpdateCategoryResponse>> UpdateAsync(
            Guid userId,
            UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category: {CategoryId} with new name: {Name}", request.Id, request.Name);

            var category = await _context.Categories
                .Include(c => c.Warehouse)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Warehouse.UserId == userId, cancellationToken);

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

            _logger.LogInformation("Category updated successfully with name: {CategoryName}", request.Name);
            return _responseHandler.Success(response, "Category updated successfully.");
        }
    }
}
