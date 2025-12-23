using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemService;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.Update;
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
    private readonly IValidator<GetByIdRequest> _getByIdValidator;
    private readonly IValidator<CreateItemRequest> _createItemValidator;
    private readonly IValidator<UpdateItemRequest> _updateItemValidator;

    public ItemsController(
        ResponseHandler responseHandler,
        IItemService itemService,
        ILogger<SectionsController> logger,
        IValidator<GetItemsOfSectionRequest> getItemsOfSectionValidator,
        IValidator<GetByIdRequest> getByIdValidator,
        IValidator<CreateItemRequest> createItemValidator,
        IValidator<UpdateItemRequest> updateItemValidator)
    {
        _responseHandler = responseHandler;
        _itemService = itemService;
        _logger = logger;
        _getItemsOfSectionValidator = getItemsOfSectionValidator;
        _getByIdValidator = getByIdValidator;
        _createItemValidator = createItemValidator;
        _updateItemValidator = updateItemValidator;
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

    [HttpGet("id")]
    public async Task<ActionResult<Response<GetByIdResponse>>> GetById(
        [FromQuery] GetByIdRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _getByIdValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid get items of section request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.GetByIdAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<Response<CreateItemResponse>>> CreateItem(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid create item request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.CreateItemAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPut]
    public async Task<ActionResult<Response<UpdateItemResponse>>> UpdateItem(
        [FromBody] UpdateItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid create item request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.UpdateItemAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

}
