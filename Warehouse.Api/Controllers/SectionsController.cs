using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.SectionService;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly ISectionService _sectionService;
    private readonly ILogger<SectionsController> _logger;
    private readonly IValidator<GetAllSectionsRequest> _getAllValidator;
    private readonly IValidator<CreateSectionRequest> _createValidator;
    private readonly IValidator<UpdateSectionRequest> _updateValidator;

    public SectionsController(
        ResponseHandler responseHandler,
        ISectionService sectionService,
        ILogger<SectionsController> logger,
        IValidator<GetAllSectionsRequest> getAllValidator,
        IValidator<CreateSectionRequest> createValidator,
        IValidator<UpdateSectionRequest> updateValidator)
    {
        _responseHandler = responseHandler;
        _sectionService = sectionService;
        _logger = logger;
        _getAllValidator = getAllValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<Response<GetAllSectionsResponse>>> GetAllSections(
    CancellationToken cancellationToken)
    {
        var response = await _sectionService.GetAllSectionsAsync(cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("create")]
    public async Task<ActionResult<Response<CreateSectionResponse>>> CreateSection(
        [FromBody] CreateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid section creation request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.CreateSectionAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPut("update")]
    public async Task<ActionResult<Response<UpdateSectionResponse>>> UpdateSection(
        [FromBody] UpdateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid section updating request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _sectionService.UpdateSectionAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

}
