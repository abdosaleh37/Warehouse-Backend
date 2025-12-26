using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Entities.Shared.ResponseHandling;
using Warehouse.DataAccess.Services.CategoryService;
using FluentValidation;
using Warehouse.Entities.DTO.Category.Create;

namespace Warehouse.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<SectionsController> _logger;
        private readonly ICategoryService categoryService;
        private readonly IValidator<CreateCategoryRequest> createCategoryValidator;

        public CategoriesController(
            ResponseHandler responseHandler,
            ILogger<SectionsController> logger,
            ICategoryService categoryService,
            IValidator<CreateCategoryRequest> createCategoryValidator)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            this.categoryService = categoryService;
            this.createCategoryValidator = createCategoryValidator;
        }

        [HttpPost("create")]
        public async Task<ActionResult<Response<CreateCategoryResponse>>> CreateCategory(
            [FromBody] CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await createCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
                _logger.LogWarning("Invalid create category request: {Errors}", validationResult.Errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }
            var response = await categoryService.CreateAsync(request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
