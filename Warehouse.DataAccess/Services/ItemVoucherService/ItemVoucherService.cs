using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.DataAccess.Services.ExcelExportService;
using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems;
using Warehouse.Entities.DTO.ItemVoucher.Delete;
using Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.Update;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.Helpers;
using Warehouse.Entities.Shared.ResponseHandling;
using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.DataAccess.Services.ItemVoucherService;

public class ItemVoucherService : IItemVoucherService
{
    private readonly IMapper _mapper;
    private readonly ILogger<ItemVoucherService> _logger;
    private readonly WarehouseDbContext _context;
    private readonly ResponseHandler _responseHandler;
    private readonly IExcelExportService _excelExportService;

    public ItemVoucherService(
        IMapper mapper,
        ILogger<ItemVoucherService> logger,
        WarehouseDbContext context,
        ResponseHandler responseHandler,
        IExcelExportService excelExportService)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _responseHandler = responseHandler;
        _excelExportService = excelExportService;
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
                    .ThenBy(iv => iv.VoucherCode.Length)
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

            var startOfMonth = DateTime.SpecifyKind(new DateTime(request.Year, request.Month, 1), DateTimeKind.Utc);
            var startOfNextMonth = DateTime.SpecifyKind(startOfMonth.AddMonths(1), DateTimeKind.Utc);

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

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            try
            {
                var existingVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == item.Id)
                    .ToListAsync(cancellationToken);

                if (request.InQuantity > 0)
                {
                    var newVoucher = _mapper.Map<ItemVoucher>(request);
                    newVoucher.Id = Guid.NewGuid();

                    var orderedWithNew = FifoConsistencyHelper.OrderChronologically(
                        existingVouchers.Append(newVoucher));

                    await _context.ItemVouchers.AddAsync(newVoucher, cancellationToken);

                    var inCutoffIndex = FifoConsistencyHelper.FindCutoffIndex(orderedWithNew, newVoucher);
                    var (inToRemove, inToAdd) = FifoConsistencyHelper.RecostFrom(item, orderedWithNew, inCutoffIndex);

                    _context.ItemVouchers.RemoveRange(inToRemove);
                    await _context.ItemVouchers.AddRangeAsync(inToAdd, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var response = _mapper.Map<CreateVoucherResponse>(newVoucher);
                    response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                    _logger.LogInformation(
                        "Created IN voucher {VoucherId} for item {ItemId}. Recosted {RecostCount} downstream OUT voucher(s).",
                        newVoucher.Id, request.ItemId, inToRemove.Count);
                    return _responseHandler.Success(response, "Voucher created successfully.");
                }

                // OUT voucher path.
                var totalOutQuantity = existingVouchers.Sum(v => v.OutQuantity);
                var availableQuantity = FifoInventoryHelper.GetAvailableQuantity(item, existingVouchers);

                if (request.OutQuantity > availableQuantity)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId}. Available: {Available}, Requested: {Requested}",
                        item.Id, availableQuantity, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<CreateVoucherResponse>(
                        $"Insufficient quantity. Available: {availableQuantity}, Requested: {request.OutQuantity}");
                }

                var prospectiveOut = new ItemVoucher
                {
                    Id = Guid.NewGuid(),
                    VoucherCode = request.VoucherCode,
                    VoucherDate = request.VoucherDate,
                    ItemId = request.ItemId,
                    InQuantity = 0,
                    OutQuantity = request.OutQuantity,
                    UnitPrice = 0,
                    Notes = request.Notes
                };

                var orderedWithProspectiveOut = FifoConsistencyHelper.OrderChronologically(
                    existingVouchers.Append(prospectiveOut));

                if (FifoConsistencyHelper.WouldCauseNegativeBalance(item, orderedWithProspectiveOut))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<CreateVoucherResponse>(
                        "This voucher would result in a negative inventory balance at some point in the item's history. " +
                        "Check the voucher date against existing vouchers.");
                }

                var inVouchersUpToOutDate = existingVouchers
                    .Where(v => v.InQuantity > 0 && v.VoucherDate <= request.VoucherDate)
                    .ToList();

                var outQuantityConsumedBeforeThis = existingVouchers
                    .Where(v => v.OutQuantity > 0 && v.VoucherDate < request.VoucherDate)
                    .Sum(v => v.OutQuantity);

                var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                    item,
                    inVouchersUpToOutDate,
                    outQuantityConsumedBeforeThis,
                    request.OutQuantity);

                if (batches.Count == 0)
                {
                    _logger.LogError("No batches found for OUT voucher. Item: {ItemId}, Quantity: {Quantity}",
                        item.Id, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<CreateVoucherResponse>("Unable to process OUT voucher.");
                }

                var createdVouchers = new List<ItemVoucher>();
                for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
                {
                    var batch = batches[batchIndex];
                    var voucherDate = request.VoucherDate.AddMicroseconds(batchIndex);

                    createdVouchers.Add(new ItemVoucher
                    {
                        Id = Guid.NewGuid(),
                        VoucherCode = request.VoucherCode,
                        VoucherDate = voucherDate,
                        ItemId = request.ItemId,
                        InQuantity = 0,
                        OutQuantity = batch.AvailableQuantity,
                        UnitPrice = batch.UnitPrice,
                        Notes = request.Notes
                    });
                }

                await _context.ItemVouchers.AddRangeAsync(createdVouchers, cancellationToken);

                var orderedWithNewOut = FifoConsistencyHelper.OrderChronologically(
                    existingVouchers.Concat(createdVouchers));

                var outCutoffIndex = orderedWithNewOut.FindIndex(v => v.Id == createdVouchers[0].Id);
                var (outToRemove, outToAdd) = FifoConsistencyHelper.RecostFrom(
                    item, orderedWithNewOut, outCutoffIndex + createdVouchers.Count);

                _context.ItemVouchers.RemoveRange(outToRemove);
                await _context.ItemVouchers.AddRangeAsync(outToAdd, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var firstVoucher = createdVouchers.First();
                var response2 = new CreateVoucherResponse
                {
                    Id = firstVoucher.Id,
                    VoucherCode = firstVoucher.VoucherCode,
                    InQuantity = 0,
                    OutQuantity = request.OutQuantity,
                    UnitPrice = batches.First().UnitPrice,
                    VoucherDate = DateTime.SpecifyKind(firstVoucher.VoucherDate, DateTimeKind.Utc),
                    Notes = firstVoucher.Notes,
                    ItemId = firstVoucher.ItemId
                };

                _logger.LogInformation("Created {Count} OUT voucher(s) for item {ItemId} (total quantity: {Quantity})",
                    createdVouchers.Count, request.ItemId, request.OutQuantity);
                return _responseHandler.Success(response2,
                    $"Voucher created successfully.{(createdVouchers.Count > 1 ? $" Split into {createdVouchers.Count} batches with different prices." : "")}");
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
                    $"Some items were not found or do not belong to the user.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Get all existing vouchers for all items
                var existingVouchersDict = await _context.ItemVouchers
                    .Where(iv => itemIds.Contains(iv.ItemId))
                    .ToListAsync(cancellationToken);

                var vouchersByItem = existingVouchersDict
                    .GroupBy(v => v.ItemId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var vouchersToCreate = new List<ItemVoucher>();
                var splitInfo = new List<string>();

                int index = 1;
                foreach (var itemRequest in request.Items)
                {
                    var itemId = itemRequest.ItemId;
                    var itemEntity = items[itemId];

                    // Get existing vouchers for this item
                    var existingVouchers = vouchersByItem.ContainsKey(itemId)
                        ? vouchersByItem[itemId]
                        : new List<ItemVoucher>();

                    var totalOutQuantity = existingVouchers.Sum(v => v.OutQuantity);
                    var availableQuantity = FifoInventoryHelper.GetAvailableQuantity(itemEntity, existingVouchers);

                    // Handle IN voucher - use price from request
                    if (itemRequest.InQuantity > 0)
                    {
                        var newVoucher = new ItemVoucher
                        {
                            Id = Guid.NewGuid(),
                            VoucherCode = request.VoucherCode,
                            VoucherDate = request.VoucherDate,
                            ItemId = itemId,
                            InQuantity = itemRequest.InQuantity,
                            OutQuantity = 0,
                            UnitPrice = itemRequest.UnitPrice,
                            Notes = itemRequest.Notes
                        };

                        var orderedWithNew = FifoConsistencyHelper.OrderChronologically(
                            existingVouchers.Append(newVoucher));

                        vouchersToCreate.Add(newVoucher);

                        var cutoffIndex = FifoConsistencyHelper.FindCutoffIndex(orderedWithNew, newVoucher);
                        var (toRemove, toAdd) = FifoConsistencyHelper.RecostFrom(itemEntity, orderedWithNew, cutoffIndex);

                        _context.ItemVouchers.RemoveRange(toRemove);
                        vouchersToCreate.AddRange(toAdd);

                        if (toRemove.Count > 0)
                        {
                            splitInfo.Add($"Item {index}: recosted {toRemove.Count} downstream OUT voucher(s)");
                        }
                    }
                    // Handle OUT voucher - use FIFO prices and split
                    else if (itemRequest.OutQuantity > 0)
                    {
                        if (itemRequest.OutQuantity > availableQuantity)
                        {
                            _logger.LogWarning("Insufficient quantity for item {ItemId}. Available: {Available}, Requested: {Requested}",
                                itemId, availableQuantity, itemRequest.OutQuantity);
                            await transaction.RollbackAsync(cancellationToken);
                            return _responseHandler.BadRequest<CreateVoucherWithManyItemsResponse>(
                                $"Insufficient quantity for item number {index}. Available: {availableQuantity}, Requested: {itemRequest.OutQuantity}");
                        }

                        var prospectiveOut = new ItemVoucher
                        {
                            Id = Guid.NewGuid(),
                            VoucherCode = request.VoucherCode,
                            VoucherDate = request.VoucherDate,
                            ItemId = itemId,
                            InQuantity = 0,
                            OutQuantity = itemRequest.OutQuantity,
                            UnitPrice = 0,
                            Notes = itemRequest.Notes
                        };

                        var orderedWithProspectiveOut = FifoConsistencyHelper.OrderChronologically(
                            existingVouchers.Append(prospectiveOut));

                        if (FifoConsistencyHelper.WouldCauseNegativeBalance(itemEntity, orderedWithProspectiveOut))
                        {
                            _logger.LogWarning(
                                "Item {ItemId} (number {Index}): voucher would cause negative balance at some point in history.",
                                itemId, index);
                            await transaction.RollbackAsync(cancellationToken);
                            return _responseHandler.BadRequest<CreateVoucherWithManyItemsResponse>(
                                $"Item number {index}: this voucher would result in a negative inventory balance " +
                                $"at some point in the item's history.");
                        }

                        // Get batches using FIFO
                        var inVouchersUpToOutDate = existingVouchers
                            .Where(v => v.InQuantity > 0 && v.VoucherDate <= request.VoucherDate)
                            .ToList();

                        var outQuantityConsumedBeforeThis = existingVouchers
                            .Where(v => v.OutQuantity > 0 && v.VoucherDate < request.VoucherDate)
                            .Sum(v => v.OutQuantity);

                        var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                            itemEntity,
                            inVouchersUpToOutDate,
                            outQuantityConsumedBeforeThis,
                            itemRequest.OutQuantity);

                        if (batches.Count == 0)
                        {
                            _logger.LogError("No batches found for OUT voucher. Item: {ItemId}, Quantity: {Quantity}",
                                itemId, itemRequest.OutQuantity);
                            await transaction.RollbackAsync(cancellationToken);
                            return _responseHandler.BadRequest<CreateVoucherWithManyItemsResponse>(
                                $"Unable to process OUT voucher for item number {index}.");
                        }

                        // Create vouchers for each batch
                        var newOutVouchers = new List<ItemVoucher>();
                        for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
                        {
                            var batch = batches[batchIndex];
                            // Add microsecond offset to maintain FIFO order when querying from database
                            var voucherDate = request.VoucherDate.AddMicroseconds(vouchersToCreate.Count + newOutVouchers.Count);

                            newOutVouchers.Add(new ItemVoucher
                            {
                                Id = Guid.NewGuid(),
                                VoucherCode = request.VoucherCode,
                                VoucherDate = voucherDate,
                                ItemId = itemId,
                                InQuantity = 0,
                                OutQuantity = batch.AvailableQuantity,
                                UnitPrice = batch.UnitPrice,
                                Notes = itemRequest.Notes
                            });
                        }

                        vouchersToCreate.AddRange(newOutVouchers);

                        if (batches.Count > 1)
                        {
                            splitInfo.Add($"Item {index} split into {batches.Count} batches");
                        }

                        var orderedWithNewOut = FifoConsistencyHelper.OrderChronologically(
                            existingVouchers.Concat(newOutVouchers));

                        var cutoffIndex = orderedWithNewOut.FindIndex(v => v.Id == newOutVouchers[0].Id);
                        var (toRemove, toAdd) = FifoConsistencyHelper.RecostFrom(
                            itemEntity, orderedWithNewOut, cutoffIndex + newOutVouchers.Count);

                        _context.ItemVouchers.RemoveRange(toRemove);
                        vouchersToCreate.AddRange(toAdd);

                        if (toRemove.Count > 0)
                        {
                            splitInfo.Add($"Item {index}: recosted {toRemove.Count} downstream OUT voucher(s)");
                        }
                    }

                    index++;
                }

                await _context.ItemVouchers.AddRangeAsync(vouchersToCreate, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Created and saved {Count} voucher(s) for {ItemCount} items by user {UserId}",
                    vouchersToCreate.Count, request.Items.Count, userId);

                var response = new CreateVoucherWithManyItemsResponse
                {
                    Id = Guid.NewGuid(),
                    VoucherCode = request.VoucherCode,
                    VoucherDate = DateTime.SpecifyKind(request.VoucherDate, DateTimeKind.Utc),
                    ItemsCount = request.Items.Count
                };

                var message = "Vouchers created successfully.";
                if (splitInfo.Any())
                {
                    message += $" {string.Join(", ", splitInfo)}.";
                }

                return _responseHandler.Success(response, message);
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
            var existingVoucher = await _context.ItemVouchers
                .Include(iv => iv.Item)
                .FirstOrDefaultAsync(iv => iv.Id == request.Id
                    && iv.Item.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (existingVoucher == null)
            {
                _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
                return _responseHandler.NotFound<UpdateVoucherResponse>("Voucher not found.");
            }

            var item = existingVoucher.Item;

            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            try
            {
                var otherVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == item.Id && iv.Id != request.Id)
                    .ToListAsync(cancellationToken);

                if (request.InQuantity > 0)
                {
                    var updatedVoucher = new ItemVoucher
                    {
                        Id = existingVoucher.Id,
                        VoucherCode = request.VoucherCode,
                        VoucherDate = request.VoucherDate,
                        ItemId = item.Id,
                        InQuantity = request.InQuantity,
                        OutQuantity = 0,
                        UnitPrice = request.UnitPrice,
                        Notes = request.Notes
                    };

                    var orderedWithUpdated = FifoConsistencyHelper.OrderChronologically(
                        otherVouchers.Append(updatedVoucher));

                    if (FifoConsistencyHelper.WouldCauseNegativeBalance(item, orderedWithUpdated))
                    {
                        _logger.LogWarning(
                            "Cannot update IN voucher {VoucherId}. Would cause negative balance at some point in history.",
                            request.Id);
                        await transaction.RollbackAsync(cancellationToken);
                        return _responseHandler.BadRequest<UpdateVoucherResponse>(
                            "Cannot update this voucher. The change would result in a negative inventory balance " +
                            "at some point in the item's history, due to quantity already consumed by OUT vouchers " +
                            "or the voucher's date relative to other vouchers.");
                    }

                    _mapper.Map(request, existingVoucher);
                    var cutoffIndex = FifoConsistencyHelper.FindCutoffIndex(orderedWithUpdated, updatedVoucher);
                    var (toRemove, toAdd) = FifoConsistencyHelper.RecostFrom(item, orderedWithUpdated, cutoffIndex);

                    _context.ItemVouchers.RemoveRange(toRemove);
                    await _context.ItemVouchers.AddRangeAsync(toAdd, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var response = _mapper.Map<UpdateVoucherResponse>(existingVoucher);
                    response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                    _logger.LogInformation(
                        "Updated IN voucher {VoucherId}. Recosted {RecostCount} downstream OUT voucher(s).",
                        request.Id, toRemove.Count);
                    return _responseHandler.Success(response, "Voucher updated successfully.");
                }

                var totalOutQuantity = otherVouchers.Sum(v => v.OutQuantity);
                var availableQuantity = FifoInventoryHelper.GetAvailableQuantity(item, otherVouchers);

                if (request.OutQuantity > availableQuantity)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId}. Available: {Available}, Requested: {Requested}",
                        item.Id, availableQuantity, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>(
                        $"Insufficient quantity. Available: {availableQuantity}, Requested: {request.OutQuantity}");
                }

                var prospectiveOut = new ItemVoucher
                {
                    Id = existingVoucher.Id,
                    VoucherCode = request.VoucherCode,
                    VoucherDate = request.VoucherDate,
                    ItemId = item.Id,
                    InQuantity = 0,
                    OutQuantity = request.OutQuantity,
                    UnitPrice = 0,
                    Notes = request.Notes
                };

                var orderedWithProspectiveOut = FifoConsistencyHelper.OrderChronologically(
                    otherVouchers.Append(prospectiveOut));

                if (FifoConsistencyHelper.WouldCauseNegativeBalance(item, orderedWithProspectiveOut))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>(
                        "This update would result in a negative inventory balance at some point in the item's history. " +
                        "Check the voucher date against other vouchers.");
                }

                var inVouchersUpToOutDate = otherVouchers
                    .Where(v => v.InQuantity > 0 && v.VoucherDate <= request.VoucherDate)
                    .ToList();

                var outQuantityConsumedBeforeThis = otherVouchers
                    .Where(v => v.OutQuantity > 0 && v.VoucherDate < request.VoucherDate)
                    .Sum(v => v.OutQuantity);

                var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                    item,
                    inVouchersUpToOutDate,
                    outQuantityConsumedBeforeThis,
                    request.OutQuantity);

                if (batches.Count == 0)
                {
                    _logger.LogError("No batches found for OUT voucher update. Item: {ItemId}, Quantity: {Quantity}",
                        item.Id, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>("Unable to process OUT voucher update.");
                }

                _context.ItemVouchers.Remove(existingVoucher);

                var createdVouchers = new List<ItemVoucher>();
                for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
                {
                    var batch = batches[batchIndex];
                    var voucherDate = request.VoucherDate.AddMicroseconds(batchIndex);

                    createdVouchers.Add(new ItemVoucher
                    {
                        Id = Guid.NewGuid(),
                        VoucherCode = request.VoucherCode,
                        VoucherDate = voucherDate,
                        ItemId = item.Id,
                        InQuantity = 0,
                        OutQuantity = batch.AvailableQuantity,
                        UnitPrice = batch.UnitPrice,
                        Notes = request.Notes
                    });
                }

                await _context.ItemVouchers.AddRangeAsync(createdVouchers, cancellationToken);

                var orderedWithNewOut = FifoConsistencyHelper.OrderChronologically(
                    otherVouchers.Concat(createdVouchers));

                var cutoffIndex2 = orderedWithNewOut.FindIndex(v => v.Id == createdVouchers[0].Id);
                var (toRemove2, toAdd2) = FifoConsistencyHelper.RecostFrom(
                    item, orderedWithNewOut, cutoffIndex2 + createdVouchers.Count);

                _context.ItemVouchers.RemoveRange(toRemove2);
                await _context.ItemVouchers.AddRangeAsync(toAdd2, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var firstVoucher = createdVouchers.First();
                var response2 = new UpdateVoucherResponse
                {
                    Id = firstVoucher.Id,
                    VoucherCode = firstVoucher.VoucherCode,
                    InQuantity = 0,
                    OutQuantity = request.OutQuantity,
                    UnitPrice = batches.First().UnitPrice,
                    VoucherDate = DateTime.SpecifyKind(firstVoucher.VoucherDate, DateTimeKind.Utc),
                    Notes = firstVoucher.Notes,
                    ItemId = firstVoucher.ItemId
                };

                _logger.LogInformation(
                    "Updated OUT voucher {VoucherId}, split into {Count} batch(es). Recosted {RecostCount} downstream OUT voucher(s).",
                    request.Id, createdVouchers.Count, toRemove2.Count);
                return _responseHandler.Success(response2,
                    $"Voucher updated successfully.{(createdVouchers.Count > 1 ? $" Split into {createdVouchers.Count} batches with different FIFO prices." : "")}");
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
                .Include(iv => iv.Item)
                .FirstOrDefaultAsync(iv => iv.Id == request.Id
                    && iv.Item.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (voucher == null)
            {
                _logger.LogWarning("Voucher {VoucherId} not found for user {UserId}", request.Id, userId);
                return _responseHandler.NotFound<DeleteVoucherResponse>("Voucher not found.");
            }

            var item = voucher.Item;

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var otherVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == voucher.ItemId && iv.Id != voucher.Id)
                    .ToListAsync(cancellationToken);

                var orderedWithoutDeleted = FifoConsistencyHelper.OrderChronologically(otherVouchers);

                if (voucher.InQuantity > 0)
                {
                    if (FifoConsistencyHelper.WouldCauseNegativeBalance(item, orderedWithoutDeleted))
                    {
                        _logger.LogWarning(
                            "Cannot delete IN voucher {VoucherId}. Would cause negative balance at some point in history.",
                            request.Id);
                        await transaction.RollbackAsync(cancellationToken);
                        return _responseHandler.BadRequest<DeleteVoucherResponse>(
                            "Cannot delete this voucher. Its quantity has already been consumed by OUT vouchers " +
                            "at some point in the item's history.");
                    }
                }

                _context.ItemVouchers.Remove(voucher);

                var positionMarker = new ItemVoucher
                {
                    Id = voucher.Id,
                    VoucherCode = voucher.VoucherCode,
                    VoucherDate = voucher.VoucherDate,
                    ItemId = voucher.ItemId,
                    InQuantity = 0,
                    OutQuantity = 0,
                    UnitPrice = 0,
                    Notes = voucher.Notes
                };

                var orderedWithMarker = FifoConsistencyHelper.OrderChronologically(
                    otherVouchers.Append(positionMarker));

                var cutoffIndex = FifoConsistencyHelper.FindCutoffIndex(orderedWithMarker, positionMarker);

                var (toRemove, toAdd) = FifoConsistencyHelper.RecostFrom(item, orderedWithoutDeleted, cutoffIndex);

                _context.ItemVouchers.RemoveRange(toRemove);
                await _context.ItemVouchers.AddRangeAsync(toAdd, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var response = new DeleteVoucherResponse { Id = request.Id };

                _logger.LogInformation(
                    "Deleted voucher {VoucherId} by user {UserId}. Recosted {RecostCount} downstream OUT voucher(s).",
                    request.Id, userId, toRemove.Count);
                return _responseHandler.Success(response, "Voucher deleted successfully.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
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

    public async Task<byte[]> ExportVouchersAsync(
        Guid userId,
        ExportVouchersRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting {VoucherType} vouchers for user {UserId}", request.VoucherType, userId);

        try
        {
            var query = _context.ItemVouchers
                .AsNoTracking()
                .Include(v => v.Item)
                    .ThenInclude(i => i.Section)
                .Where(v => v.Item.Section.Category.Warehouse.UserId == userId);

            if (request.VoucherType == VoucherType.In)
            {
                query = query.Where(v => v.InQuantity > 0);
            }
            else
            {
                query = query.Where(v => v.OutQuantity > 0);
            }

            if (request.Month.HasValue && request.Year.HasValue)
            {
                var startDate = new DateTime(request.Year.Value, request.Month.Value, 1);
                var endDate = startDate.AddMonths(1);
                query = query.Where(v => v.VoucherDate >= startDate && v.VoucherDate < endDate);
            }

            var vouchers = await query
                .OrderBy(v => v.VoucherDate)
                    .ThenBy(v => v.VoucherCode)
                .ToListAsync(cancellationToken);

            if (vouchers.Count == 0)
            {
                _logger.LogInformation("No vouchers found for export");
                throw new InvalidOperationException("No vouchers found matching the criteria");
            }

            // Group vouchers by voucher code
            var voucherGroups = vouchers
                .GroupBy(v => v.VoucherCode)
                .Select(g => new VoucherExportData
                {
                    VoucherCode = g.Key,
                    VoucherDate = g.First().VoucherDate,
                    Items = g.Select(v => new VoucherItemData
                    {
                        ItemPartNo = v.Item.PartNo ?? v.Item.ItemCode,
                        Description = v.Item.Description,
                        Quantity = request.VoucherType == VoucherType.In ? v.InQuantity : v.OutQuantity,
                        Unit = v.Item.Unit,
                        SectionName = v.Item.Section.Name
                    }).ToList()
                })
                .ToList();

            var excelBytes = await _excelExportService.ExportVouchersToExcelAsync(
                voucherGroups,
                request.VoucherType,
                cancellationToken);

            _logger.LogInformation("Successfully exported {VoucherCount} vouchers to Excel", voucherGroups.Count);
            return excelBytes;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ExportVouchersAsync cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting vouchers for user {UserId}", userId);
            throw;
        }
    }
}
