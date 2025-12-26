using MapsterMapper;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ResponseHandler responseHandler;
        private readonly WarehouseDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(
            ILogger<CategoryService> logger,
            ResponseHandler responseHandler,
            WarehouseDbContext context,
            IMapper mapper) 
        {
            _logger = logger;
            this.responseHandler = responseHandler;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateCategoryResponse>> CreateAsync(
            CreateCategoryRequest request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new category with name: {CategoryName}", request.Name);

            var categoryEntity = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                WarehouseId = request.WarehouseId
            };

            try
            {
                _context.Categories.Add(categoryEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating categocategory.");
                return responseHandler.InternalServerError<CreateCategoryResponse>(
                    "An error occurred while creating the category.");
            }
            var response = _mapper.Map<CreateCategoryResponse>(categoryEntity);

            _logger.LogInformation("Category created successfully with name: {CategoryName}", request.Name);
            return responseHandler.Success<CreateCategoryResponse>(response, "Category created successfully.");

        }
    }
}
