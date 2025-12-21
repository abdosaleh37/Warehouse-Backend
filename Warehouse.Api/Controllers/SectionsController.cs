using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.SectionService;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly ISectionService _sectionService;
    private readonly ILogger<SectionsController> _logger;
    private readonly IValidator<CreateSectionRequest> _createValidator;

    public SectionsController(
        ResponseHandler responseHandler,
        ISectionService sectionService,
        ILogger<SectionsController> logger,
        IValidator<CreateSectionRequest> createValidator)
    {
        _responseHandler = responseHandler;
        _sectionService = sectionService;
        _logger = logger;
        _createValidator = createValidator;
    }

    [HttpPost]
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

}
