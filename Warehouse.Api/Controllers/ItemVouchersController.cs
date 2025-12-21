using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.ItemVoucherService;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemVouchersController : ControllerBase
{
    private readonly IItemVoucherService _itemVoucherService;

    public ItemVouchersController(IItemVoucherService itemVoucherService)
    {
        _itemVoucherService = itemVoucherService;
    }

}
