using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemVoucherService;

public class ItemVoucherService : IItemVoucherService
{
    private readonly IMapper _mapper;
    private readonly ILogger<ItemVoucherService> _logger;
    private readonly WarehouseDbContext _context;
    private readonly ResponseHandler _responseHandler;

    public ItemVoucherService(
        IMapper mapper,
        ILogger<ItemVoucherService> logger,
        WarehouseDbContext context, 
        ResponseHandler responseHandler)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _responseHandler = responseHandler;
    }

    public async Task<Response<GetVouchersOfItemResponse>> GetVouchersOfItemAsync(
        Guid userId,
        GetVouchersOfItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting vouchers for item {ItemId} by user {UserId}", request.ItemId, userId);

        var item = await _context.Items
            .Include(i => i.Section)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item {ItemId} not found for user {UserId}", request.ItemId, userId);
            return _responseHandler.NotFound<GetVouchersOfItemResponse>("Item not found.");
        }

        var vouchers = await _context.ItemVouchers
            .Where(iv => iv.ItemId == request.ItemId)
            .OrderBy(iv => iv.VoucherDate)
                .ThenBy(iv => iv.Id)
            .ToListAsync(cancellationToken);

        if (vouchers.Count == 0)
        {
            _logger.LogInformation("No vouchers found for item {ItemId}", request.ItemId);
            return _responseHandler.Success(new GetVouchersOfItemResponse
            {
                Vouchers = new List<GetVouchersOfItemResult>(),
                TotalCount = 0,
                ItemId = item.Id,
                ItemDescription = item.Description
            }, "No vouchers found.");
        }

        var voucherResults = _mapper.Map<List<GetVouchersOfItemResult>>(vouchers);

        int runningQuantity = item.OpeningQuantity;
        decimal runningValue = item.OpeningUnitPrice * item.OpeningQuantity;

        for (int i = 0; i < voucherResults.Count; i++)
        {
            var dto = voucherResults[i];
            var entity = vouchers[i];

            var netQuantity = entity.InQuantity - entity.OutQuantity;
            var netValue = netQuantity * entity.UnitPrice;

            runningQuantity += netQuantity;
            runningValue += netValue;

            dto.AmountAfterVoucher = runningQuantity;
            dto.ValueAfterVoucher = runningValue;
        }

        var response = new GetVouchersOfItemResponse
        {
            Vouchers = voucherResults,
            TotalCount = voucherResults.Count,
            ItemId = item.Id,
            ItemDescription = item.Description
        };

        _logger.LogInformation("Retrieved {VoucherCount} vouchers for item {ItemId}", voucherResults.Count, request.ItemId);
        return _responseHandler.Success(response, "Vouchers retrieved successfully.");
    }

    public async Task<Response<GetVoucherByIdResponse>> GetVoucherByIdAsync(
        Guid userId,
        GetVoucherByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting voucher {VoucherId} by user {UserId}", request.Id, userId);
        var voucher = await _context.ItemVouchers
            .Include(iv => iv.Item)
                .ThenInclude(i => i.Section)
                    .ThenInclude(s => s.Category)
                        .ThenInclude(c => c.Warehouse)
            .FirstOrDefaultAsync(iv => iv.Id == request.Id && iv.Item.Section.Category.Warehouse.UserId == userId, cancellationToken);

        if (voucher == null)
        {
            _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
            return _responseHandler.NotFound<GetVoucherByIdResponse>("Voucher not found.");
        }

        var response = _mapper.Map<GetVoucherByIdResponse>(voucher);

        _logger.LogInformation("Retrieved voucher {VoucherId} for user {UserId}", request.Id, userId);
        return _responseHandler.Success(response, "Voucher retrieved successfully.");
    }
}
