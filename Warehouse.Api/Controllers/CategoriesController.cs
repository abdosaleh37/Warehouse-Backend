using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Warehouse.DataAccess.Services.CategoryService;
using Warehouse.Entities.DTO.Category.Create;
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
        private readonly ILogger<SectionsController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IValidator<GetCategoryByIdRequest> _getCategoryByIdValidator;
        private readonly IValidator<CreateCategoryRequest> _createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequest> _updateCategoryValidator;

        public CategoriesController(
            ResponseHandler responseHandler,
            ILogger<SectionsController> logger,
            ICategoryService categoryService,
            IValidator<GetCategoryByIdRequest> getCategoryByIdValidator,
            IValidator<CreateCategoryRequest> createCategoryValidator,
            IValidator<UpdateCategoryRequest> updateCategoryValidator)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _categoryService = categoryService;
            _getCategoryByIdValidator = getCategoryByIdValidator;
            _createCategoryValidator = createCategoryValidator;
            _updateCategoryValidator = updateCategoryValidator;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Response<GetAllCategoriesResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            var response = await _categoryService.GetAllAsync(userId, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("id")]
        [Authorize]
        public async Task<ActionResult<Response<GetCategoryByIdResponse>>> GetById(
            [FromQuery] GetCategoryByIdRequest request,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            var validationResult = await _getCategoryByIdValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
                _logger.LogWarning("Invalid get a category request: {Errors}", validationResult.Errors);
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            var validationResult = await _createCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
                _logger.LogWarning("Invalid create category request: {Errors}", validationResult.Errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.CreateAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<ActionResult<Response<UpdateCategoryResponse>>> UpdateCategory(
            [FromBody] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            var validationResult = await _updateCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
                _logger.LogWarning("Invalid create category request: {Errors}", validationResult.Errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _categoryService.UpdateAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
