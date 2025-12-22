using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
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

    public async Task<Response<CreateItemResponse>> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new item in section: {SectionId}", request.SectionId);
        var section = _context.Sections
            .FirstOrDefault(s => s.Id == request.SectionId);

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
}
