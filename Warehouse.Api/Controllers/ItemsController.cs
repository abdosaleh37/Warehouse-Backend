using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemService;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.Items.Search;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;
    private readonly IValidator<GetItemsOfSectionRequest> _getItemsOfSectionValidator;
    private readonly IValidator<GetItemByIdRequest> _getByIdValidator;
    private readonly IValidator<GetItemsWithVouchersOfMonthRequest> _getItemsWithVouchersOfMonthValidator;
    private readonly IValidator<SearchItemsRequest> _searchItemsValidator;
    private readonly IValidator<CreateItemRequest> _createItemValidator;
    private readonly IValidator<UpdateItemRequest> _updateItemValidator;
    private readonly IValidator<DeleteItemRequest> _deleteItemValidator;

    public ItemsController(
        ResponseHandler responseHandler,
        IItemService itemService,
        ILogger<ItemsController> logger,
        IValidator<GetItemsOfSectionRequest> getItemsOfSectionValidator,
        IValidator<GetItemByIdRequest> getByIdValidator,
        IValidator<GetItemsWithVouchersOfMonthRequest> getItemsWithVouchersOfMonthValidator,
        IValidator<SearchItemsRequest> searchItemsValidator,
        IValidator<CreateItemRequest> createItemValidator,
        IValidator<UpdateItemRequest> updateItemValidator,
        IValidator<DeleteItemRequest> deleteItemValidator)
    {
        _responseHandler = responseHandler;
        _itemService = itemService;
        _logger = logger;
        _getItemsOfSectionValidator = getItemsOfSectionValidator;
        _getByIdValidator = getByIdValidator;
        _getItemsWithVouchersOfMonthValidator = getItemsWithVouchersOfMonthValidator;
        _searchItemsValidator = searchItemsValidator;
        _createItemValidator = createItemValidator;
        _updateItemValidator = updateItemValidator;
        _deleteItemValidator = deleteItemValidator;
    }

    [HttpGet("section/{id:guid}")]
    public async Task<ActionResult<Response<GetItemsOfSectionResponse>>> GetItemsOfSection(
        [FromRoute] Guid id,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetItemsOfSectionRequest { SectionId = id, SearchString = q };
        var validationResult = await _getItemsOfSectionValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get items of section request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.GetItemsofSectionAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Response<GetItemByIdResponse>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetItemByIdRequest { Id = id };
        var validationResult = await _getByIdValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get item by id request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.GetByIdAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("vouchers/{year:int}/{month:int}")]
    public async Task<ActionResult<Response<GetItemsWithVouchersOfMonthResponse>>> GetItemsWithVouchersOfMonth(
        [FromRoute] int year,
        [FromRoute] int month,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetItemsWithVouchersOfMonthRequest { Year = year, Month = month };

        var validationResult = await _getItemsWithVouchersOfMonthValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get items with vouchers of month request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.GetItemsWithVouchersOfMonthAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("export/monthly-excel/{year:int}/{month:int}")]
    public async Task<IActionResult> ExportMonthlyItemsToExcel(
        [FromRoute] int year,
        [FromRoute] int month,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetItemsWithVouchersOfMonthRequest { Year = year, Month = month };

        var validationResult = await _getItemsWithVouchersOfMonthValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid export monthly items excel request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        try
        {
            var excelData = await _itemService.ExportMonthlyItemsToExcelAsync(userId, request, cancellationToken);

            var fileName = $"Items_{year}_{month:D2}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting monthly items to Excel for Month: {Month}, Year: {Year}", month, year);
            return StatusCode((int)_responseHandler.InternalServerError<object>("An error occurred while exporting data to Excel.").StatusCode,
                _responseHandler.InternalServerError<object>("An error occurred while exporting data to Excel."));
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<Response<SearchItemsResponse>>> SearchItems(
        [FromQuery] SearchItemsRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _searchItemsValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid search items request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemService.SearchItemsAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<Response<CreateItemResponse>>> CreateItem(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _createItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid create item request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.CreateItemAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPut]
    public async Task<ActionResult<Response<UpdateItemResponse>>> UpdateItem(
        [FromBody] UpdateItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _updateItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid update item request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.UpdateItemAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete]
    public async Task<ActionResult<Response<DeleteItemResponse>>> DeleteItem(
        [FromBody] DeleteItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _deleteItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid delete item request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }
        var response = await _itemService.DeleteItemAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

}
