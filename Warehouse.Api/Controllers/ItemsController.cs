using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemService;
using Warehouse.DataAccess.Services.SectionService;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly IItemService _itemService;
    private readonly ILogger<SectionsController> _logger;
    private readonly IValidator<GetItemsOfSectionRequest> _getItemsOfSectionValidator;

    public ItemsController(
        ResponseHandler responseHandler,
        IItemService itemService,
        ILogger<SectionsController> logger,
        IValidator<GetItemsOfSectionRequest> getItemsOfSectionValidator)

    {
        _responseHandler = responseHandler;
        _itemService = itemService;
        _logger = logger;
        _getItemsOfSectionValidator = getItemsOfSectionValidator;
    }

    [HttpGet("section")]
    public async Task<ActionResult<Response<GetItemsOfSectionResponse>>> GetItemsOfSection(
        [FromQuery] GetItemsOfSectionRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _getItemsOfSectionValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid get items of section request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.GetItemsofSectionAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

}
