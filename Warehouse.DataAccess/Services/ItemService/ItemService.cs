using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.Items.Search;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemService;

public class ItemService : IItemService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ItemService> _logger;

    public ItemService(WarehouseDbContext context,
        IMapper mapper,
        ResponseHandler responseHandler,
        ILogger<ItemService> logger)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    public async Task<Response<GetItemsOfSectionResponse>> GetItemsofSectionAsync(
        Guid userId,
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Items of section: {SectionId}", request.SectionId);

        try
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.SectionId
                    && s.Category.Warehouse.UserId == userId, cancellationToken);

            if (section == null)
            {
                _logger.LogWarning("Section: {SectionId} not found.", request.SectionId);
                return _responseHandler.NotFound<GetItemsOfSectionResponse>("Section not found");
            }

            var search = request.SearchString?.Trim();

            var query = _context.Items
                .AsNoTracking()
                .Where(i => i.SectionId == request.SectionId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    (i.ItemCode != null && EF.Functions.Like(i.ItemCode, $"%{search}%")) ||
                    (i.PartNo != null && EF.Functions.Like(i.PartNo, $"%{search}%")) ||
                    (i.Description != null && EF.Functions.Like(i.Description, $"%{search}%"))
                );
            }

            var items = await query
                .OrderBy(i => i.ItemCode.Length)
                    .ThenBy(i => i.ItemCode)
                        .ThenBy(i => i.CreatedAt)
                .Select(i => new
                {
                    Item = i,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                _logger.LogInformation("No items found in section: {SectionId}", request.SectionId);
                return _responseHandler.Success(new GetItemsOfSectionResponse
                {
                    SectionId = section.Id,
                    SectionName = section.Name,
                    Items = new List<GetItemsOfSectionResult>(),
                    TotalItemsCount = 0
                }, "No items found in this section.");

            }

            var itemResults = items.Select(x =>
            {
                var mapped = _mapper.Map<GetItemsOfSectionResult>(x.Item);
                mapped.AvailableQuantity = x.Item.OpeningQuantity + x.NetQuantity;
                mapped.AvailableValue = (x.Item.OpeningUnitPrice * x.Item.OpeningQuantity) + x.NetValue;
                mapped.OpeningDate = DateTime.SpecifyKind(x.Item.OpeningDate, DateTimeKind.Utc);
                mapped.CreatedAt = DateTime.SpecifyKind(x.Item.CreatedAt, DateTimeKind.Utc);
                return mapped;
            }).ToList();

            var responseData = new GetItemsOfSectionResponse
            {
                SectionId = section.Id,
                SectionName = section.Name,
                Items = itemResults,
                TotalItemsCount = itemResults.Count
            };

            _logger.LogInformation("Retrieved {ItemCount} items from section: {SectionId}",
                itemResults.Count, request.SectionId);
            return _responseHandler.Success(responseData, "Items retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetItemsofSectionAsync cancelled for Section: {SectionId}", request.SectionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving items for Section: {SectionId}", request.SectionId);
            return _responseHandler.InternalServerError<GetItemsOfSectionResponse>("An error occurred while retrieving items.");
        }
    }

    public async Task<Response<GetItemByIdResponse>> GetByIdAsync(
        Guid userId,
        GetItemByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting item by Id: {ItemId}", request.Id);

        try
        {
            var itemResult = await _context.Items
                .AsNoTracking()
                .Where(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId)
                .Select(i => new
                {
                    Item = i,
                    SectionName = i.Section.Name,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (itemResult == null)
            {
                _logger.LogWarning("Item with Id: {ItemId} not found.", request.Id);
                return _responseHandler.NotFound<GetItemByIdResponse>("Item not found");
            }

            var response = _mapper.Map<GetItemByIdResponse>(itemResult.Item);
            response.SectionName = itemResult.SectionName;
            response.AvailableQuantity = itemResult.Item.OpeningQuantity + itemResult.NetQuantity;
            response.AvailableValue = (itemResult.Item.OpeningUnitPrice * itemResult.Item.OpeningQuantity) + itemResult.NetValue;
            response.OpeningDate = DateTime.SpecifyKind(itemResult.Item.OpeningDate, DateTimeKind.Utc);
            response.CreatedAt = DateTime.SpecifyKind(itemResult.Item.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Item with Id: {ItemId} retrieved successfully.", request.Id);
            return _responseHandler.Success(response, "Item retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetByIdAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<GetItemByIdResponse>("An error occurred while retrieving the item.");
        }
    }

    public async Task<Response<GetItemsWithVouchersOfMonthResponse>> GetItemsWithVouchersOfMonthAsync(
        Guid userId,
        GetItemsWithVouchersOfMonthRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting items with vouchers for month: {Month}, year: {Year}", request.Month, request.Year);

        try
        {
            var startOfMonth = DateTime.SpecifyKind(new DateTime(request.Year, request.Month, 1), DateTimeKind.Utc);
            var startOfNextMonth = DateTime.SpecifyKind(startOfMonth.AddMonths(1), DateTimeKind.Utc);

            var itemsWithVouchers = await _context.Items
                .Where(i => i.Section.Category.Warehouse.UserId == userId &&
                    i.ItemVouchers.Any(v => v.VoucherDate >= startOfMonth
                        && v.VoucherDate < startOfNextMonth))
                .Include(i => i.Section)
                    .ThenInclude(s => s.Category)
                .AsNoTracking()
                .Select(i => new
                {
                    Item = i,
                    MonthVouchers = i.ItemVouchers
                        .Where(v => v.VoucherDate >= startOfMonth
                            && v.VoucherDate < startOfNextMonth)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            if (itemsWithVouchers.Count == 0)
            {
                _logger.LogInformation("No items with vouchers found for month: {Month}, year: {Year}", request.Month, request.Year);
                return _responseHandler.Success(new GetItemsWithVouchersOfMonthResponse
                {
                    Items = new List<GetItemsWithVouchersOfMonthResult>(),
                    TotalCount = 0
                }, "No items with vouchers found for the specified month and year.");
            }

            var results = itemsWithVouchers.Select(x => new GetItemsWithVouchersOfMonthResult
            {
                Id = x.Item.Id,
                ItemCode = x.Item.ItemCode,
                PartNo = x.Item.PartNo,
                Description = x.Item.Description,
                SectionId = x.Item.SectionId,
                SectionName = x.Item.Section.Name,
                CategoryId = x.Item.Section.CategoryId,
                CategoryName = x.Item.Section.Category.Name,
                Unit = x.Item.Unit,
                VouchersTotalInQuantity = x.MonthVouchers.Sum(v => v.InQuantity),
                VouchersTotalOutQuantity = x.MonthVouchers.Sum(v => v.OutQuantity),
                VouchersTotalQuantity = x.MonthVouchers.Sum(v => v.InQuantity - v.OutQuantity),
                VouchersTotalInValue = x.MonthVouchers.Sum(v => v.InQuantity * v.UnitPrice),
                VouchersTotalOutValue = x.MonthVouchers.Sum(v => v.OutQuantity * v.UnitPrice),
                VouchersTotalValue = x.MonthVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice)
            })
            .OrderBy(i => i.SectionId)
                .ThenBy(i => i.ItemCode.Length)
                    .ThenBy(i => i.ItemCode)
            .ToList();

            var response = new GetItemsWithVouchersOfMonthResponse
            {
                Items = results,
                TotalCount = results.Count
            };

            _logger.LogInformation("Retrieved {ItemCount} items with vouchers for month: {Month}, year: {Year}",
                results.Count, request.Month, request.Year);
            return _responseHandler.Success(response, "Items with vouchers retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetItemsWithVouchersOfMonthAsync cancelled for Month: {Month}, Year: {Year}", request.Month, request.Year);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving items with vouchers for Month: {Month}, Year: {Year}", request.Month, request.Year);
            return _responseHandler.InternalServerError<GetItemsWithVouchersOfMonthResponse>("An error occurred while retrieving items with vouchers.");
        }
    }

    public async Task<Response<SearchItemsResponse>> SearchItemsAsync(
        Guid userId,
        SearchItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching items with term: {SearchTerm}", request.SearchTerm);

        try
        {
            var query = _context.Items
                .AsNoTracking()
                .Where(i => i.Section.Category.Warehouse.UserId == userId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(i =>
                    (i.ItemCode != null && EF.Functions.Like(i.ItemCode, searchTerm)) ||
                    (i.PartNo != null && EF.Functions.Like(i.PartNo, searchTerm)) ||
                    (i.Description != null && EF.Functions.Like(i.Description, searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(i => i.Section.CreatedAt)
                    .ThenBy(i => i.ItemCode.Length)
                        .ThenBy(i => i.ItemCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new
                {
                    Item = i,
                    SectionName = i.Section.Name,
                    CategoryId = i.Section.CategoryId,
                    CategoryName = i.Section.Category.Name,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                _logger.LogInformation("No items found with search term: {SearchTerm}", request.SearchTerm);
                return _responseHandler.Success(new SearchItemsResponse
                {
                    Items = new List<SearchItemsResult>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    HasNextPage = false,
                    HasPreviousPage = false
                }, "No items found matching the search criteria.");
            }

            var itemResults = items.Select(x => new SearchItemsResult
            {
                Id = x.Item.Id,
                ItemCode = x.Item.ItemCode,
                PartNo = x.Item.PartNo,
                Description = x.Item.Description,
                UnitOfMeasure = x.Item.Unit,
                SectionId = x.Item.SectionId,
                SectionName = x.SectionName,
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                OpeningQuantity = x.Item.OpeningQuantity,
                OpeningUnitPrice = x.Item.OpeningUnitPrice,
                OpeningDate = DateTime.SpecifyKind(x.Item.OpeningDate, DateTimeKind.Utc),
                AvailableQuantity = x.Item.OpeningQuantity + x.NetQuantity,
                AvailableValue = (x.Item.OpeningUnitPrice * x.Item.OpeningQuantity) + x.NetValue,
                CreatedAt = DateTime.SpecifyKind(x.Item.CreatedAt, DateTimeKind.Utc)
            }).ToList();

            var response = new SearchItemsResponse
            {
                Items = itemResults,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                HasNextPage = (request.PageNumber * request.PageSize) < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            _logger.LogInformation("Found {ItemCount} items matching search term: {SearchTerm}",
                itemResults.Count, request.SearchTerm);
            return _responseHandler.Success(response, "Items retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SearchItemsAsync cancelled for SearchTerm: {SearchTerm}", request.SearchTerm);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching items with term: {SearchTerm}", request.SearchTerm);
            return _responseHandler.InternalServerError<SearchItemsResponse>("An error occurred while searching items.");
        }
    }

    public async Task<Response<CreateItemResponse>> CreateItemAsync(
        Guid userId,
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new item in section: {SectionId}", request.SectionId);

        try
        {
            var section = await _context.Sections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SectionId
                    && s.Category.Warehouse.UserId == userId, cancellationToken);

            if (section == null)
            {
                _logger.LogWarning("Section: {SectionId} not found. Cannot create item.", request.SectionId);
                return _responseHandler.NotFound<CreateItemResponse>("Section not found");
            }

            var existingItem = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.SectionId == request.SectionId
                    && i.ItemCode == request.ItemCode, cancellationToken);

            if (existingItem != null)
            {
                _logger.LogWarning("Item with code: {ItemCode} already exists in section: {SectionId}",
                    request.ItemCode, request.SectionId);
                return _responseHandler.BadRequest<CreateItemResponse>("An item with the same code already exists in this section.");
            }

            var newItem = _mapper.Map<Item>(request);
            newItem.Id = Guid.NewGuid();
            newItem.CreatedAt = DateTime.UtcNow;

            await _context.Items.AddAsync(newItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var responseData = _mapper.Map<CreateItemResponse>(newItem);

            _logger.LogInformation("Item created successfully with Id: {ItemId} in section: {SectionId}",
                newItem.Id, request.SectionId);
            return _responseHandler.Success(responseData, "Item created successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CreateItemAsync cancelled for Section: {SectionId}", request.SectionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating item in section: {SectionId}", request.SectionId);
            return _responseHandler.InternalServerError<CreateItemResponse>("An error occurred while creating the item.");
        }
    }

    public async Task<Response<UpdateItemResponse>> UpdateItemAsync(
        Guid userId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating item: {ItemId}", request.Id);

        try
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item: {Id} not found. Cannot update item.", request.Id);
                return _responseHandler.NotFound<UpdateItemResponse>("Item not found");
            }

            // Check for ItemCode conflict within the same section if code changed
            if (!string.Equals(item.ItemCode, request.ItemCode, StringComparison.OrdinalIgnoreCase))
            {
                var existingWithCode = await _context.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.SectionId == item.SectionId
                            && i.ItemCode == request.ItemCode
                            && i.Id != item.Id, cancellationToken);

                if (existingWithCode != null)
                {
                    _logger.LogWarning("Item code conflict: {ItemCode} already exists in section {SectionId}", request.ItemCode, item.SectionId);
                    return _responseHandler.BadRequest<UpdateItemResponse>("An item with the same code already exists in this section.");
                }
            }

            _mapper.Map(request, item);

            await _context.SaveChangesAsync(cancellationToken);

            var responseData = _mapper.Map<UpdateItemResponse>(item);

            _logger.LogInformation("Item updated successfully with Id: {ItemId}", item.Id);
            return _responseHandler.Success(responseData, "Item updated successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UpdateItemAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<UpdateItemResponse>("An error occurred while updating the item.");
        }
    }

    public async Task<Response<DeleteItemResponse>> DeleteItemAsync(
        Guid userId,
        DeleteItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting item: {ItemId}", request.Id);

        try
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item: {ItemId} not found. Cannot delete item.", request.Id);
                return _responseHandler.NotFound<DeleteItemResponse>("Item not found");
            }

            var hasVouchers = await _context.ItemVouchers
                .AsNoTracking()
                .AnyAsync(iv => iv.ItemId == item.Id, cancellationToken);

            if (hasVouchers)
            {
                _logger.LogWarning("Item: {ItemId} has associated vouchers. Cannot delete item.", request.Id);
                return _responseHandler.BadRequest<DeleteItemResponse>("Cannot delete item with associated vouchers.");
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            var responseData = new DeleteItemResponse
            {
                Id = item.Id
            };

            _logger.LogInformation("Item deleted successfully with Id: {ItemId}", item.Id);
            return _responseHandler.Success(responseData, "Item deleted successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DeleteItemAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<DeleteItemResponse>("An error occurred while deleting the item.");
        }
    }
}