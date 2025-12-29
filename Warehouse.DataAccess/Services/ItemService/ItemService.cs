using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
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
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Items of section: {SectionId}", request.SectionId);
        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section: {SectionId} not found.", request.SectionId);
            return _responseHandler.NotFound<GetItemsOfSectionResponse>("Section not found");
        }

        var items = await _context.Items
            .AsNoTracking()
            .Include(i => i.ItemVouchers)
            .Where(i => i.SectionId == request.SectionId)
            .OrderBy(i => i.CreatedAt)
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

        var itemResults = _mapper.Map<List<GetItemsOfSectionResult>>(items);
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

    public async Task<Response<GetItemByIdResponse>> GetByIdAsync(
        GetItemByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting item by Id: {ItemId}", request.Id);
        var item = await _context.Items
            .Include(i => i.Section)
            .Include(i => i.ItemVouchers)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item with Id: {ItemId} not found.", request.Id);
            return _responseHandler.NotFound<GetItemByIdResponse>("Item not found");
        }

        var responseData = _mapper.Map<GetItemByIdResponse>(item);

        _logger.LogInformation("Item with Id: {ItemId} retrieved successfully.", request.Id);
        return _responseHandler.Success(responseData, "Item retrieved successfully.");
    }

    public async Task<Response<CreateItemResponse>> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new item in section: {SectionId}", request.SectionId);
        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section: {SectionId} not found. Cannot create item.", request.SectionId);
            return _responseHandler.NotFound<CreateItemResponse>("Section not found");
        }

        var existingItem = await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SectionId == request.SectionId && i.ItemCode == request.ItemCode, cancellationToken);

        if (existingItem != null)
        {
            _logger.LogWarning("Item with code: {ItemCode} already exists in section: {SectionId}",
                request.ItemCode, request.SectionId);
            return _responseHandler.BadRequest<CreateItemResponse>("An item with the same code already exists in this section.");
        }

            var newItem = _mapper.Map<Item>(request);
        newItem.Id = Guid.NewGuid();
        newItem.CreatedAt = DateTime.UtcNow;

        try
        {
            await _context.Items.AddAsync(newItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating item in section: {SectionId}", request.SectionId);
            return _responseHandler.InternalServerError<CreateItemResponse>("An error occurred while creating the item.");
        }

        var responseData = _mapper.Map<CreateItemResponse>(newItem);
        _logger.LogInformation("Item created successfully with Id: {ItemId} in section: {SectionId}",
            newItem.Id, request.SectionId);
        return _responseHandler.Success(responseData, "Item created successfully.");
    }

    public async Task<Response<UpdateItemResponse>> UpdateItemAsync(
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating item: {ItemId}", request.Id);

        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

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

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<UpdateItemResponse>("An error occurred while updating the item.");
        }

        var responseData = _mapper.Map<UpdateItemResponse>(item);
        _logger.LogInformation("Item updated successfully with Id: {ItemId}", item.Id);
        return _responseHandler.Success(responseData, "Item updated successfully.");
    }

    public async Task<Response<DeleteItemResponse>> DeleteItemAsync(
        DeleteItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting item: {ItemId}", request.Id);
        var item = await _context.Items
            .Include(i => i.ItemVouchers)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
        {             
            _logger.LogWarning("Item: {ItemId} not found. Cannot delete item.", request.Id);
            return _responseHandler.NotFound<DeleteItemResponse>("Item not found");
        }

        if (item.ItemVouchers.Any())
        {
            _logger.LogWarning("Cannot delete item with Id: {Id} because it has {VouchersCount} Vouchers",
                request.Id, item.ItemVouchers.Count);
            return _responseHandler.BadRequest<DeleteItemResponse>(
                $"Cannot delete item '{item.Id}' because it contains {item.ItemVouchers.Count} vouchers(s). Please remove or reassign the vouchers first.");
        }

        try
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<DeleteItemResponse>("An error occurred while deleting the item.");
        }

        var responseData = new DeleteItemResponse
        {
            Id = item.Id
        };

        _logger.LogInformation("Item deleted successfully with Id: {ItemId}", item.Id);
        return _responseHandler.Success(responseData, "Item deleted successfully.");
    }
}
