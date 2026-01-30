using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems;
using Warehouse.Entities.DTO.ItemVoucher.Delete;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.Update;
using Warehouse.Entities.Entities;
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

        try
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.ItemId
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item {ItemId} not found for user {UserId}", request.ItemId, userId);
                return _responseHandler.NotFound<GetVouchersOfItemResponse>("Item not found.");
            }

            var vouchers = await _context.ItemVouchers
                .AsNoTracking()
                .Where(iv => iv.ItemId == request.ItemId)
                .OrderBy(iv => iv.VoucherDate)
                    .ThenBy(iv => iv.VoucherCode)
                .ToListAsync(cancellationToken);

            if (vouchers.Count == 0)
            {
                _logger.LogInformation("No vouchers found for item {ItemId}", request.ItemId);
                return _responseHandler.Success(new GetVouchersOfItemResponse
                {
                    Vouchers = new List<GetVouchersOfItemResult>(),
                    TotalCount = 0,
                    ItemId = item.Id,
                    ItemDescription = item.Description,
                    ItemAvailableQuantity = item.OpeningQuantity,
                    ItemAvailableValue = item.OpeningUnitPrice * item.OpeningQuantity,
                    TotalInQuantity = 0,
                    TotalInValue = 0m,
                    TotalOutQuantity = 0,
                    TotalOutValue = 0m
                }, "No vouchers found.");
            }

            var voucherResults = _mapper.Map<List<GetVouchersOfItemResult>>(vouchers);

            // Ensure VoucherDate is returned as UTC
            foreach (var dto in voucherResults)
            {
                dto.VoucherDate = DateTime.SpecifyKind(dto.VoucherDate, DateTimeKind.Utc);
            }

            int runningQuantity = item.OpeningQuantity;
            decimal runningValue = item.OpeningUnitPrice * item.OpeningQuantity;

            int totalInQuantity = 0;
            decimal totalInValue = 0m;
            int totalOutQuantity = 0;
            decimal totalOutValue = 0m;

            for (int i = 0; i < voucherResults.Count; i++)
            {
                var dto = voucherResults[i];
                var entity = vouchers[i];

                var netQuantity = entity.InQuantity - entity.OutQuantity;
                var netValue = netQuantity * entity.UnitPrice;

                runningQuantity += netQuantity;
                runningValue += netValue;

                // Accumulate totals
                totalInQuantity += entity.InQuantity;
                totalInValue += entity.InQuantity * entity.UnitPrice;
                totalOutQuantity += entity.OutQuantity;
                totalOutValue += entity.OutQuantity * entity.UnitPrice;

                dto.AmountAfterVoucher = runningQuantity;
                dto.ValueAfterVoucher = runningValue;
            }

            var response = new GetVouchersOfItemResponse
            {
                Vouchers = voucherResults,
                TotalCount = voucherResults.Count,
                ItemId = item.Id,
                ItemDescription = item.Description,
                ItemAvailableQuantity = runningQuantity,
                ItemAvailableValue = runningValue,
                TotalInQuantity = totalInQuantity,
                TotalInValue = totalInValue,
                TotalOutQuantity = totalOutQuantity,
                TotalOutValue = totalOutValue
            };

            _logger.LogInformation("Retrieved {VoucherCount} vouchers for item {ItemId}", voucherResults.Count, request.ItemId);
            return _responseHandler.Success(response, "Vouchers retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetVouchersOfItemAsync cancelled for Item: {ItemId}", request.ItemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving vouchers for item {ItemId}", request.ItemId);
            return _responseHandler.InternalServerError<GetVouchersOfItemResponse>("An error occurred while retrieving vouchers.");
        }
    }

    public async Task<Response<GetVoucherByIdResponse>> GetVoucherByIdAsync(
        Guid userId,
        GetVoucherByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting voucher {VoucherId} by user {UserId}", request.Id, userId);

        try
        {
            var voucher = await _context.ItemVouchers
                .AsNoTracking()
                .Where(iv => iv.Id == request.Id
                    && iv.Item.Section.Category.Warehouse.UserId == userId)
                .Select(iv => new
                {
                    Voucher = iv,
                    ItemDescription = iv.Item.Description
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (voucher == null)
            {
                _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
                return _responseHandler.NotFound<GetVoucherByIdResponse>("Voucher not found.");
            }

            var response = _mapper.Map<GetVoucherByIdResponse>(voucher.Voucher);
            response.ItemDescription = voucher.ItemDescription ?? string.Empty;
            response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

            _logger.LogInformation("Retrieved voucher {VoucherId} for user {UserId}", request.Id, userId);
            return _responseHandler.Success(response, "Voucher retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetVoucherByIdAsync cancelled for Voucher: {VoucherId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving voucher {VoucherId}", request.Id);
            return _responseHandler.InternalServerError<GetVoucherByIdResponse>("An error occurred while retrieving the voucher.");
        }
    }

    public async Task<Response<GetMonthlyVouchersOfItemResponse>> GetMonthlyVouchersOfItemAsync(
        Guid userId,
        GetMonthlyVouchersOfItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting vouchers for item {ItemId} of month {Month}/{Year} by user {UserId}",
            request.ItemId, request.Month, request.Year, userId);

        try
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.ItemId
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item {ItemId} not found for user {UserId}", request.ItemId, userId);
                return _responseHandler.NotFound<GetMonthlyVouchersOfItemResponse>("Item not found.");
            }

            var startOfMonth = new DateTime(request.Year, request.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var preMonthSums = await _context.ItemVouchers
                .Where(iv => iv.ItemId == request.ItemId
                    && iv.VoucherDate < startOfMonth)
                .GroupBy(iv => 1)
                .Select(g => new
                {
                    NetQuantity = g.Sum(iv => iv.InQuantity - iv.OutQuantity),
                    NetValue = g.Sum(iv => (iv.InQuantity - iv.OutQuantity) * iv.UnitPrice)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var preMonthNetQuantity = preMonthSums?.NetQuantity ?? 0;
            var preMonthNetValue = preMonthSums?.NetValue ?? 0m;

            var vouchersInMonth = await _context.ItemVouchers
                .AsNoTracking()
                .Where(iv => iv.ItemId == request.ItemId
                    && iv.VoucherDate >= startOfMonth
                    && iv.VoucherDate < startOfNextMonth)
                .OrderBy(iv => iv.VoucherDate)
                    .ThenBy(iv => iv.VoucherCode)
                .ToListAsync(cancellationToken);

            if (vouchersInMonth.Count == 0)
            {
                _logger.LogInformation("No vouchers found for item {ItemId} in month {Month}/{Year}", request.ItemId, request.Month, request.Year);
                return _responseHandler.Success(new GetMonthlyVouchersOfItemResponse
                {
                    Vouchers = new List<GetMonthlyVouchersOfItemResult>(),
                    TotalCount = 0,
                    ItemId = item.Id,
                    ItemDescription = item.Description,
                    TotalInQuantity = 0,
                    TotalInValue = 0m,
                    TotalOutQuantity = 0,
                    TotalOutValue = 0m,
                    PreMonthItemAvailableQuantity = item.OpeningQuantity + preMonthNetQuantity,
                    PreMonthItemAvailableValue = (item.OpeningUnitPrice * item.OpeningQuantity) + preMonthNetValue,
                    PostMonthItemAvailableQuantity = item.OpeningQuantity + preMonthNetQuantity,
                    PostMonthItemAvailableValue = (item.OpeningUnitPrice * item.OpeningQuantity) + preMonthNetValue
                }, "No vouchers found.");
            }

            var voucherResults = _mapper.Map<List<GetMonthlyVouchersOfItemResult>>(vouchersInMonth);

            // Ensure VoucherDate is returned as UTC
            foreach (var dto in voucherResults)
            {
                dto.VoucherDate = DateTime.SpecifyKind(dto.VoucherDate, DateTimeKind.Utc);
            }

            // Calculate running totals and aggregate totals
            int runningQuantity = item.OpeningQuantity + preMonthNetQuantity;
            decimal runningValue = (item.OpeningUnitPrice * item.OpeningQuantity) + preMonthNetValue;

            int totalInQuantity = 0;
            decimal totalInValue = 0m;
            int totalOutQuantity = 0;
            decimal totalOutValue = 0m;

            for (int i = 0; i < voucherResults.Count; i++)
            {
                var dto = voucherResults[i];
                var entity = vouchersInMonth[i];

                var netQuantity = entity.InQuantity - entity.OutQuantity;
                var netValue = netQuantity * entity.UnitPrice;

                runningQuantity += netQuantity;
                runningValue += netValue;

                dto.AmountAfterVoucher = runningQuantity;
                dto.ValueAfterVoucher = runningValue;

                // Accumulate totals
                totalInQuantity += entity.InQuantity;
                totalInValue += entity.InQuantity * entity.UnitPrice;
                totalOutQuantity += entity.OutQuantity;
                totalOutValue += entity.OutQuantity * entity.UnitPrice;
            }

            var response = new GetMonthlyVouchersOfItemResponse
            {
                Vouchers = voucherResults,
                TotalCount = voucherResults.Count,
                ItemId = item.Id,
                ItemDescription = item.Description,
                TotalInQuantity = totalInQuantity,
                TotalInValue = totalInValue,
                TotalOutQuantity = totalOutQuantity,
                TotalOutValue = totalOutValue,
                PreMonthItemAvailableQuantity = item.OpeningQuantity + preMonthNetQuantity,
                PreMonthItemAvailableValue = (item.OpeningUnitPrice * item.OpeningQuantity) + preMonthNetValue,
                PostMonthItemAvailableQuantity = runningQuantity,
                PostMonthItemAvailableValue = runningValue
            };

            _logger.LogInformation("Retrieved {VoucherCount} vouchers for item {ItemId} in month {Month}/{Year} (In: {TotalIn}, Out: {TotalOut})",
                voucherResults.Count, request.ItemId, request.Month, request.Year, totalInQuantity, totalOutQuantity);
            return _responseHandler.Success(response, "Vouchers retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetMonthlyVouchersOfItemAsync cancelled for Item: {ItemId}", request.ItemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving monthly vouchers for item {ItemId}", request.ItemId);
            return _responseHandler.InternalServerError<GetMonthlyVouchersOfItemResponse>("An error occurred while retrieving monthly vouchers.");
        }
    }

    public async Task<Response<CreateVoucherResponse>> CreateVoucherAsync(
        Guid userId,
        CreateVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating voucher for item {ItemId} by user {UserId}", request.ItemId, userId);

        try
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.ItemId
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item {ItemId} not found for user {UserId}", request.ItemId, userId);
                return _responseHandler.NotFound<CreateVoucherResponse>("Item not found.");
            }

            var currentNetQuantity = await _context.ItemVouchers
                .Where(iv => iv.ItemId == item.Id)
                .Select(iv => iv.InQuantity - iv.OutQuantity)
                .SumAsync(cancellationToken);

            var newNetQuantity = request.InQuantity - request.OutQuantity;
            var projectedAvailable = item.OpeningQuantity + currentNetQuantity + newNetQuantity;

            if (projectedAvailable < 0)
            {
                _logger.LogWarning("Insufficient quantity for item {ItemId} when creating voucher by user {UserId}. Projected available: {Projected}",
                    item.Id, userId, projectedAvailable);
                return _responseHandler.BadRequest<CreateVoucherResponse>("Insufficient available quantity for this voucher.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var currentNetQuantityTx = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == item.Id)
                    .Select(iv => iv.InQuantity - iv.OutQuantity)
                    .SumAsync(cancellationToken);

                var projectedAvailableTx = item.OpeningQuantity + currentNetQuantityTx + newNetQuantity;
                if (projectedAvailableTx < 0)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId} when creating voucher (after recheck) by user {UserId}. Projected available: {Projected}",
                        item.Id, userId, projectedAvailableTx);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<CreateVoucherResponse>("Insufficient available quantity for this voucher.");
                }

                var voucherEntity = _mapper.Map<ItemVoucher>(request);

                await _context.ItemVouchers.AddAsync(voucherEntity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<CreateVoucherResponse>(voucherEntity);
                response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                _logger.LogInformation("Created voucher {VoucherId} for item {ItemId} by user {UserId}", voucherEntity.Id, request.ItemId, userId);
                return _responseHandler.Success(response, "Voucher created successfully.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CreateVoucherAsync cancelled for Item: {ItemId}", request.ItemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher for item {ItemId} by user {UserId}", request.ItemId, userId);
            return _responseHandler.InternalServerError<CreateVoucherResponse>("An error occurred while creating the voucher.");
        }
    }

    public async Task<Response<CreateVoucherWithManyItemsResponse>> CreateVoucherWithManyItemsAsync(
    Guid userId,
    CreateVoucherWithManyItemsRequest request,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating vouchers for multiple items by user {UserId}", userId);

        try
        {
            var itemIds = request.Items.Select(i => i.ItemId).Distinct().ToList();

            var items = await _context.Items
                .AsNoTracking()
                .Where(i => itemIds.Contains(i.Id)
                    && i.Section.Category.Warehouse.UserId == userId)
                .ToDictionaryAsync(i => i.Id, cancellationToken);

            // Ensure all requested item IDs exist and belong to the user
            var missingItemIds = itemIds.Except(items.Keys).ToList();
            if (missingItemIds.Any())
            {
                _logger.LogWarning("Some items were not found or do not belong to user {UserId}: {Missing}",
                    userId, string.Join(',', missingItemIds));
                return _responseHandler.NotFound<CreateVoucherWithManyItemsResponse>(
                    $"Some items were not found or do not belong to the user: {string.Join(',', missingItemIds)}");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Compute current net quantities for all items inside the transaction
                var netQuantities = await _context.ItemVouchers
                    .Where(iv => itemIds.Contains(iv.ItemId))
                    .GroupBy(iv => iv.ItemId)
                    .Select(g => new { ItemId = g.Key, Net = g.Sum(iv => iv.InQuantity - iv.OutQuantity) })
                    .ToDictionaryAsync(x => x.ItemId, x => x.Net, cancellationToken);

                var vouchersToCreate = new List<ItemVoucher>();

                foreach (var item in items)
                {
                    var itemId = item.Key;
                    var itemEntity = item.Value;
                    var currentNetQuantity = netQuantities.TryGetValue(itemId, out var net) ? net : 0;
                    var itemRequests = request.Items.Where(i => i.ItemId == itemId).ToList();

                    foreach (var itemRequest in itemRequests)
                    {
                        var newNetQuantity = itemRequest.InQuantity - itemRequest.OutQuantity;
                        var projectedAvailableTx = itemEntity.OpeningQuantity + currentNetQuantity + newNetQuantity;

                        if (projectedAvailableTx < 0)
                        {
                            _logger.LogWarning(
                                "Insufficient quantity for item {ItemId} when creating voucher (after recheck) by user {UserId}. Projected available: {Projected}",
                                itemId, userId, projectedAvailableTx);
                            await transaction.RollbackAsync(cancellationToken);
                            return _responseHandler.BadRequest<CreateVoucherWithManyItemsResponse>(
                                $"Insufficient available quantity for item {itemId}.");
                        }

                        // Add to list of vouchers to create
                        vouchersToCreate.Add(new ItemVoucher
                        {
                            VoucherCode = request.VoucherCode,
                            VoucherDate = request.VoucherDate,
                            ItemId = itemId,
                            InQuantity = itemRequest.InQuantity,
                            OutQuantity = itemRequest.OutQuantity,
                            UnitPrice = itemRequest.UnitPrice,
                            Notes = itemRequest.Notes
                        });
                    }
                }

                await _context.ItemVouchers.AddRangeAsync(vouchersToCreate, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Created and saved {Count} vouchers for multiple items by user {UserId}",
                    vouchersToCreate.Count, userId);

                var response = new CreateVoucherWithManyItemsResponse
                {
                    Id = Guid.NewGuid(),
                    VoucherCode = request.VoucherCode,
                    VoucherDate = request.VoucherDate,
                    ItemsCount = vouchersToCreate.Count
                };

                return _responseHandler.Success(response, "Vouchers created successfully.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CreateVoucherWithManyItemsAsync cancelled by user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vouchers for multiple items by user {UserId}", userId);
            return _responseHandler.InternalServerError<CreateVoucherWithManyItemsResponse>(
                "An error occurred while creating the vouchers.");
        }
    }


    public async Task<Response<UpdateVoucherResponse>> UpdateVoucherAsync(
        Guid userId,
        UpdateVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating voucher: {VoucherId} by user {UserId}", request.Id, userId);

        try
        {
            var voucher = await _context.ItemVouchers
                .AsNoTracking()
                .Where(iv => iv.Id == request.Id
                    && iv.Item.Section.Category.Warehouse.UserId == userId)
                .Select(iv => new { iv, ItemOpening = iv.Item.OpeningQuantity })
                .FirstOrDefaultAsync(cancellationToken);

            if (voucher == null)
            {
                _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
                return _responseHandler.NotFound<UpdateVoucherResponse>("Voucher not found.");
            }

            var currentNetQuantity = await _context.ItemVouchers
                .Where(iv => iv.ItemId == voucher.iv.ItemId && iv.Id != request.Id)
                .Select(iv => iv.InQuantity - iv.OutQuantity)
                .SumAsync(cancellationToken);

            var newNetQuantity = request.InQuantity - request.OutQuantity;
            var projectedAvailable = voucher.ItemOpening + currentNetQuantity + newNetQuantity;

            if (projectedAvailable < 0)
            {
                _logger.LogWarning("Insufficient quantity for item {ItemId} when updating voucher by user {UserId}. Projected available: {Projected}",
                    voucher.iv.ItemId, userId, projectedAvailable);
                return _responseHandler.BadRequest<UpdateVoucherResponse>("Insufficient available quantity for this voucher.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var currentNetQuantityTx = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == voucher.iv.ItemId && iv.Id != request.Id)
                    .Select(iv => iv.InQuantity - iv.OutQuantity)
                    .SumAsync(cancellationToken);

                var projectedAvailableTx = voucher.ItemOpening + currentNetQuantityTx + newNetQuantity;
                if (projectedAvailableTx < 0)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId} when updating voucher (after recheck) by user {UserId}. Projected available: {Projected}",
                        voucher.iv.ItemId, userId, projectedAvailableTx);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>("Insufficient available quantity for this voucher.");
                }

                var voucherEntity = await _context.ItemVouchers
                    .FirstOrDefaultAsync(iv => iv.Id == request.Id, cancellationToken);

                if (voucherEntity == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.NotFound<UpdateVoucherResponse>("Voucher not found.");
                }

                _mapper.Map(request, voucherEntity);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var response = _mapper.Map<UpdateVoucherResponse>(voucherEntity);
                response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                _logger.LogInformation("Updated voucher {VoucherId} by user {UserId}", request.Id, userId);
                return _responseHandler.Success(response, "Voucher updated successfully.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UpdateVoucherAsync cancelled for Voucher: {VoucherId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher: {VoucherId} by user {UserId}", request.Id, userId);
            return _responseHandler.InternalServerError<UpdateVoucherResponse>("An error occurred while updating the voucher.");
        }
    }

    public async Task<Response<DeleteVoucherResponse>> DeleteVoucherAsync(
        Guid userId,
        DeleteVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting voucher: {VoucherId} by user {UserId}", request.Id, userId);

        try
        {
            var voucher = await _context.ItemVouchers
                .FirstOrDefaultAsync(iv => iv.Id == request.Id
                    && iv.Item.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (voucher == null)
            {
                _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
                return _responseHandler.NotFound<DeleteVoucherResponse>("Voucher not found.");
            }

            _context.ItemVouchers.Remove(voucher);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new DeleteVoucherResponse { Id = request.Id };

            _logger.LogInformation("Deleted voucher {VoucherId} by user {UserId}", request.Id, userId);
            return _responseHandler.Success(response, "Voucher deleted successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DeleteVoucherAsync cancelled for Voucher: {VoucherId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher: {VoucherId} by user {UserId}", request.Id, userId);
            return _responseHandler.InternalServerError<DeleteVoucherResponse>("An error occurred while deleting the voucher.");
        }
    }
}
