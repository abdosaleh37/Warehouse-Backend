using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.CategoryService;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.Delete;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.DTO.Category.GetById;
using Warehouse.Entities.DTO.Category.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<CategoriesController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IValidator<GetCategoryByIdRequest> _getCategoryByIdValidator;
        private readonly IValidator<CreateCategoryRequest> _createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequest> _updateCategoryValidator;
        private readonly IValidator<DeleteCategoryRequest> _deleteCategoryValidator;

        public CategoriesController(
            ResponseHandler responseHandler,
            ILogger<CategoriesController> logger,
            ICategoryService categoryService,
            IValidator<GetCategoryByIdRequest> getCategoryByIdValidator,
            IValidator<CreateCategoryRequest> createCategoryValidator,
            IValidator<UpdateCategoryRequest> updateCategoryValidator,
            IValidator<DeleteCategoryRequest> deleteCategoryValidator)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _categoryService = categoryService;
            _getCategoryByIdValidator = getCategoryByIdValidator;
            _createCategoryValidator = createCategoryValidator;
            _updateCategoryValidator = updateCategoryValidator;
            _deleteCategoryValidator = deleteCategoryValidator;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Response<GetAllCategoriesResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out Guid userId))
            {
                return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                    _responseHandler.Unauthorized<object>("Invalid user"));
            }

            var response = await _categoryService.GetAllAsync(userId, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Response<GetCategoryByIdResponse>>> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out Guid userId))
            {
                return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                    _responseHandler.Unauthorized<object>("Invalid user"));
            }

            var request = new GetCategoryByIdRequest { Id = id };
            var validationResult = await _getCategoryByIdValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = validationResult.Errors.FlattenErrors();
                _logger.LogWarning("Invalid get a category request: {Errors}", errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.GetByIdAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Response<CreateCategoryResponse>>> CreateCategory(
            [FromBody] CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out Guid userId))
            {
                return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                    _responseHandler.Unauthorized<object>("Invalid user"));
            }

            var validationResult = await _createCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = validationResult.Errors.FlattenErrors();
                _logger.LogWarning("Invalid create category request: {Errors}", errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.CreateAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<Response<UpdateCategoryResponse>>> UpdateCategory(
            [FromBody] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out Guid userId))
            {
                return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                    _responseHandler.Unauthorized<object>("Invalid user"));
            }

            var validationResult = await _updateCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = validationResult.Errors.FlattenErrors();
                _logger.LogWarning("Invalid update category request: {Errors}", errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.UpdateAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<ActionResult<Response<DeleteCategoryResponse>>> DeleteCategory(
            [FromBody] DeleteCategoryRequest request,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out Guid userId))
            {
                return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                    _responseHandler.Unauthorized<object>("Invalid user"));
            }

            var validationResult = await _deleteCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = validationResult.Errors.FlattenErrors();
                _logger.LogWarning("Invalid delete category request: {Errors}", errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.DeleteAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
