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
using Warehouse.Entities.Shared.Helpers;
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

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var existingVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == item.Id)
                    .OrderBy(iv => iv.VoucherDate)
                    .ToListAsync(cancellationToken);

                var totalOutQuantity = existingVouchers.Sum(v => v.OutQuantity);
                var availableQuantity = FifoInventoryHelper.GetAvailableQuantity(item, existingVouchers);

                if (request.InQuantity > 0)
                {
                    var voucherEntity = _mapper.Map<ItemVoucher>(request);
                    await _context.ItemVouchers.AddAsync(voucherEntity, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var response = _mapper.Map<CreateVoucherResponse>(voucherEntity);
                    response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                    _logger.LogInformation("Created IN voucher {VoucherId} for item {ItemId}", voucherEntity.Id, request.ItemId);
                    return _responseHandler.Success(response, "Voucher created successfully.");
                }

                if (request.OutQuantity > availableQuantity)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId}. Available: {Available}, Requested: {Requested}",
                        item.Id, availableQuantity, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<CreateVoucherResponse>(
                        $"Insufficient quantity. Available: {availableQuantity}, Requested: {request.OutQuantity}");
                }

                var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                    item,
                    existingVouchers,
                    totalOutQuantity,
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
                    // Add microsecond offset to maintain FIFO order when querying from database
                    var voucherDate = request.VoucherDate.AddMicroseconds(batchIndex);

                    var voucherEntity = new ItemVoucher
                    {
                        Id = Guid.NewGuid(),
                        VoucherCode = request.VoucherCode,
                        VoucherDate = voucherDate,
                        ItemId = request.ItemId,
                        InQuantity = 0,
                        OutQuantity = batch.AvailableQuantity,
                        UnitPrice = batch.UnitPrice,
                        Notes = request.Notes
                    };

                    createdVouchers.Add(voucherEntity);
                }

                await _context.ItemVouchers.AddRangeAsync(createdVouchers, cancellationToken);
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

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Get all existing vouchers for all items
                var existingVouchersDict = await _context.ItemVouchers
                    .Where(iv => itemIds.Contains(iv.ItemId))
                    .OrderBy(iv => iv.VoucherDate)
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
                        vouchersToCreate.Add(new ItemVoucher
                        {
                            Id = Guid.NewGuid(),
                            VoucherCode = request.VoucherCode,
                            VoucherDate = request.VoucherDate,
                            ItemId = itemId,
                            InQuantity = itemRequest.InQuantity,
                            OutQuantity = 0,
                            UnitPrice = itemRequest.UnitPrice,
                            Notes = itemRequest.Notes
                        });
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

                        // Get batches using FIFO
                        var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                            itemEntity,
                            existingVouchers,
                            totalOutQuantity,
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
                        for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
                        {
                            var batch = batches[batchIndex];
                            // Add microsecond offset to maintain FIFO order when querying from database
                            var voucherDate = request.VoucherDate.AddMicroseconds(vouchersToCreate.Count);

                            vouchersToCreate.Add(new ItemVoucher
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

                        if (batches.Count > 1)
                        {
                            splitInfo.Add($"Item {index} split into {batches.Count} batches");
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
                    message += $" {string.Join(", ", splitInfo)} with different FIFO prices.";
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

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Get all existing vouchers EXCEPT the one being updated
                var otherVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == item.Id && iv.Id != request.Id)
                    .OrderBy(iv => iv.VoucherDate)
                    .ToListAsync(cancellationToken);

                var totalOutQuantity = otherVouchers.Sum(v => v.OutQuantity);
                var availableQuantity = FifoInventoryHelper.GetAvailableQuantity(item, otherVouchers);

                // Validate that update won't result in negative inventory
                if (existingVoucher.InQuantity > 0)
                {
                    var quantityReduction = existingVoucher.InQuantity - request.InQuantity;
                    if (quantityReduction > 0)
                    {
                        var availableQuantityAfterUpdate = availableQuantity - quantityReduction;

                        if (availableQuantityAfterUpdate < 0)
                        {
                            _logger.LogWarning("Cannot update IN voucher {VoucherId}. Would result in negative inventory. " +
                                "Available after update: {After}, Reduction: {Reduction}",
                                request.Id, availableQuantityAfterUpdate, quantityReduction);
                            await transaction.RollbackAsync(cancellationToken);
                            return _responseHandler.BadRequest<UpdateVoucherResponse>(
                                $"Cannot update this voucher. The quantity reduction of {quantityReduction} has already been consumed by OUT vouchers. " +
                                $"Available quantity would become {availableQuantityAfterUpdate}.");
                        }
                    }
                }

                // Handle IN voucher update - use price from request
                if (request.InQuantity > 0)
                {
                    _mapper.Map(request, existingVoucher);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var response = _mapper.Map<UpdateVoucherResponse>(existingVoucher);
                    response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                    _logger.LogInformation("Updated IN voucher {VoucherId}", request.Id);
                    return _responseHandler.Success(response, "Voucher updated successfully.");
                }

                // Handle OUT voucher update - use FIFO prices and potentially split
                if (request.OutQuantity > availableQuantity)
                {
                    _logger.LogWarning("Insufficient quantity for item {ItemId}. Available: {Available}, Requested: {Requested}",
                        item.Id, availableQuantity, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>(
                        $"Insufficient quantity. Available: {availableQuantity}, Requested: {request.OutQuantity}");
                }

                // Get batches using FIFO (excluding the voucher being updated)
                var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                    item,
                    otherVouchers,
                    totalOutQuantity,
                    request.OutQuantity);

                if (batches.Count == 0)
                {
                    _logger.LogError("No batches found for OUT voucher update. Item: {ItemId}, Quantity: {Quantity}",
                        item.Id, request.OutQuantity);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<UpdateVoucherResponse>("Unable to process OUT voucher update.");
                }

                // If only one batch and matches existing voucher, just update it
                if (batches.Count == 1)
                {
                    existingVoucher.VoucherCode = request.VoucherCode;
                    existingVoucher.VoucherDate = request.VoucherDate;
                    existingVoucher.InQuantity = 0;
                    existingVoucher.OutQuantity = batches[0].AvailableQuantity;
                    existingVoucher.UnitPrice = batches[0].UnitPrice;
                    existingVoucher.Notes = request.Notes;

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var response = _mapper.Map<UpdateVoucherResponse>(existingVoucher);
                    response.VoucherDate = DateTime.SpecifyKind(response.VoucherDate, DateTimeKind.Utc);

                    _logger.LogInformation("Updated OUT voucher {VoucherId}", request.Id);
                    return _responseHandler.Success(response, "Voucher updated successfully.");
                }

                // Multiple batches needed - delete old and create new ones
                _context.ItemVouchers.Remove(existingVoucher);

                var createdVouchers = new List<ItemVoucher>();
                for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
                {
                    var batch = batches[batchIndex];
                    // Add microsecond offset to maintain FIFO order when querying from database
                    var voucherDate = request.VoucherDate.AddMicroseconds(batchIndex);

                    var voucherEntity = new ItemVoucher
                    {
                        Id = Guid.NewGuid(),
                        VoucherCode = request.VoucherCode,
                        VoucherDate = voucherDate,
                        ItemId = item.Id,
                        InQuantity = 0,
                        OutQuantity = batch.AvailableQuantity,
                        UnitPrice = batch.UnitPrice,
                        Notes = request.Notes
                    };

                    createdVouchers.Add(voucherEntity);
                }

                await _context.ItemVouchers.AddRangeAsync(createdVouchers, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Return response with first voucher info
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

                _logger.LogInformation("Updated OUT voucher {VoucherId}, split into {Count} batch(es)",
                    request.Id, createdVouchers.Count);
                return _responseHandler.Success(response2,
                    $"Voucher updated successfully. Split into {createdVouchers.Count} batches with different FIFO prices.");
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

            // Validate that deletion won't result in negative inventory
            if (voucher.InQuantity > 0)
            {
                var allVouchers = await _context.ItemVouchers
                    .Where(iv => iv.ItemId == voucher.ItemId)
                    .OrderBy(iv => iv.VoucherDate)
                    .ToListAsync(cancellationToken);

                // Calculate current available quantity
                var currentAvailableQuantity = FifoInventoryHelper.GetAvailableQuantity(voucher.Item, allVouchers);

                // Calculate what the available quantity would be after removing this IN voucher
                var availableQuantityAfterDeletion = currentAvailableQuantity - voucher.InQuantity;

                if (availableQuantityAfterDeletion < 0)
                {
                    _logger.LogWarning("Cannot delete IN voucher {VoucherId}. Would result in negative inventory. " +
                        "Current: {Current}, After deletion: {After}",
                        request.Id, currentAvailableQuantity, availableQuantityAfterDeletion);
                    return _responseHandler.BadRequest<DeleteVoucherResponse>(
                        $"Cannot delete this voucher. The quantity has already been consumed by other OUT vouchers. " +
                        $"Available quantity would become {availableQuantityAfterDeletion}.");
                }
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
