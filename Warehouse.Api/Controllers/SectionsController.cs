using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.SectionService;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.Delete;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.GetById;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<SectionsController> _logger;
    private readonly ISectionService _sectionService;
    private readonly IValidator<GetSectionByIdRequest> _getByIdValidator;
    private readonly IValidator<CreateSectionRequest> _createValidator;
    private readonly IValidator<UpdateSectionRequest> _updateValidator;
    private readonly IValidator<DeleteSectionRequest> _deleteValidator;

    public SectionsController(
        ResponseHandler responseHandler,
        ILogger<SectionsController> logger,
        ISectionService sectionService,
        IValidator<GetSectionByIdRequest> getByIdValidator,
        IValidator<CreateSectionRequest> createValidator,
        IValidator<UpdateSectionRequest> updateValidator,
        IValidator<DeleteSectionRequest> deleteValidator)
    {
        _responseHandler = responseHandler;
        _logger = logger;
        _sectionService = sectionService;
        _getByIdValidator = getByIdValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deleteValidator = deleteValidator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<Response<GetAllSectionsResponse>>> GetAllSections(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var response = await _sectionService.GetAllSectionsAsync(userId, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<Response<GetSectionByIdResponse>>> GetSectionById(
        [FromQuery] GetSectionByIdRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _getByIdValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid section get-by-id request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.GetSectionByIdAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<ActionResult<Response<CreateSectionResponse>>> CreateSection(
        [FromBody] CreateSectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid section creation request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.CreateSectionAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<ActionResult<Response<UpdateSectionResponse>>> UpdateSection(
        [FromBody] UpdateSectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid section updating request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.UpdateSectionAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("delete")]
    [Authorize]
    public async Task<ActionResult<Response<DeleteSectionResponse>>> DeleteSection(
        [FromBody] DeleteSectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _deleteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid section deleting request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.DeleteSectionAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

}
