using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Warehouse.DataAccess.Services.CategoryService;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.Shared.ResponseHandling;

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

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Response<GetAllCategoriesResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);

            var response = await categoryService.GetAllAsync(userId, cancellationToken);
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

            var validationResult = await createCategoryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
                _logger.LogWarning("Invalid create category request: {Errors}", validationResult.Errors);
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await categoryService.CreateAsync(userId, request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
