using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemVoucherService;
using Warehouse.Entities.DTO.ItemVoucher;
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

    public ItemVouchersController(
        ResponseHandler responseHandler,
        ILogger<ItemVouchersController> logger,
        IItemVoucherService itemVoucherService,
        IValidator<GetVouchersOfItemRequest> getVouchersOfItemValidator)
    {
        _responseHandler = responseHandler;
        _logger = logger;
        _itemVoucherService = itemVoucherService;
        _getVouchersOfItemValidator = getVouchersOfItemValidator;
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
}
