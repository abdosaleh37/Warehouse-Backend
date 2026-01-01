using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemVoucherService;
using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.Delete;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemVouchersController : ControllerBase
{
    private readonly ResponseHandler _responseHandler;
    private readonly IItemVoucherService _itemVoucherService;
    private readonly ILogger<ItemVouchersController> _logger;
    private readonly IValidator<GetVouchersOfItemRequest> _getVouchersOfItemValidator;
    private readonly IValidator<GetMonthlyVouchersOfItemRequest> _getMonthlyVouchersOfItemValidator;
    private readonly IValidator<GetVoucherByIdRequest> _getVoucherByIdValidator;
    private readonly IValidator<CreateVoucherRequest> _createVoucherValidator;
    private readonly IValidator<UpdateVoucherRequest> _updateVoucherValidator;
    private readonly IValidator<DeleteVoucherRequest> _deleteteVoucherValidator;

    public ItemVouchersController(
        ResponseHandler responseHandler,
        ILogger<ItemVouchersController> logger,
        IItemVoucherService itemVoucherService,
        IValidator<GetVouchersOfItemRequest> getVouchersOfItemValidator,
        IValidator<GetMonthlyVouchersOfItemRequest> getMonthlyVouchersOfItemValidator,
        IValidator<GetVoucherByIdRequest> getVoucherByIdValidator,
        IValidator<CreateVoucherRequest> createVoucherValidator,
        IValidator<UpdateVoucherRequest> updateVoucherValidator,
        IValidator<DeleteVoucherRequest> deleteteVoucherValidator)
    {
        _responseHandler = responseHandler;
        _logger = logger;
        _itemVoucherService = itemVoucherService;
        _getVouchersOfItemValidator = getVouchersOfItemValidator;
        _getMonthlyVouchersOfItemValidator = getMonthlyVouchersOfItemValidator;
        _getVoucherByIdValidator = getVoucherByIdValidator;
        _createVoucherValidator = createVoucherValidator;
        _updateVoucherValidator = updateVoucherValidator;
        _deleteteVoucherValidator = deleteteVoucherValidator;
    }

    [HttpGet("item/{id:guid}")]
    public async Task<ActionResult<Response<GetVouchersOfItemResponse>>> GetVouchersOfItem(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetVouchersOfItemRequest { ItemId = id };
        var validationResult = await _getVouchersOfItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get vouchers of item request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.GetVouchersOfItemAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Response<GetVoucherByIdResponse>>> GetVoucherById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetVoucherByIdRequest { Id = id };
        var validationResult = await _getVoucherByIdValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get voucher by id request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.GetVoucherByIdAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("item/{id:guid}/monthly")]
    public async Task<ActionResult<Response<GetMonthlyVouchersOfItemResponse>>> GetMonthlyVouchersOfItem(
        [FromRoute] Guid id,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetMonthlyVouchersOfItemRequest { ItemId = id, Year = year, Month = month };

        var validationResult = await _getMonthlyVouchersOfItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get monthly vouchers of item request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.GetMonthlyVouchersOfItemAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<Response<CreateVoucherResponse>>> CreateVoucher(
        [FromBody] CreateVoucherRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _createVoucherValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid create voucher request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.CreateVoucherAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPut]
    public async Task<ActionResult<Response<UpdateVoucherResponse>>> UpdateVoucher(
        [FromBody] UpdateVoucherRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _updateVoucherValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid update voucher request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.UpdateVoucherAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete]
    public async Task<ActionResult<Response<DeleteVoucherResponse>>> DeleteVoucher(
        [FromBody] DeleteVoucherRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var validationResult = await _deleteteVoucherValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid delete voucher request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _itemVoucherService.DeleteVoucherAsync(userId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }
}
