using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemVoucherService;
using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
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
    private readonly IValidator<GetVoucherByIdRequest> _getVoucherByIdValidator;
    private readonly IValidator<CreateVoucherRequest> _createVoucherValidator;

    public ItemVouchersController(
        ResponseHandler responseHandler,
        ILogger<ItemVouchersController> logger,
        IItemVoucherService itemVoucherService,
        IValidator<GetVouchersOfItemRequest> getVouchersOfItemValidator,
        IValidator<GetVoucherByIdRequest> getVoucherByIdValidator,
        IValidator<CreateVoucherRequest> createVoucherValidator)
    {
        _responseHandler = responseHandler;
        _logger = logger;
        _itemVoucherService = itemVoucherService;
        _getVouchersOfItemValidator = getVouchersOfItemValidator;
        _getVoucherByIdValidator = getVoucherByIdValidator;
        _createVoucherValidator = createVoucherValidator;
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
    public ActionResult<Response<GetVoucherByIdResponse>> GetVoucherById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            return StatusCode((int)_responseHandler.Unauthorized<object>("Invalid user").StatusCode,
                _responseHandler.Unauthorized<object>("Invalid user"));
        }

        var request = new GetVoucherByIdRequest { Id = id };
        var validationResult = _getVoucherByIdValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid get voucher by id request: {Errors}", errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = _itemVoucherService.GetVoucherByIdAsync(userId, request, cancellationToken).Result;
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
}
