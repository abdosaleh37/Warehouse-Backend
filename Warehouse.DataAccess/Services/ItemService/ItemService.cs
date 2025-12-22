using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
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
                Items = new List<Item>(),
                TotalItemsCount = 0
            }, "No items found in this section.");

        }

        var responseData = new GetItemsOfSectionResponse
        {
            SectionId = section.Id,
            SectionName = section.Name,
            Items = items,
            TotalItemsCount = items.Count
        };

        _logger.LogInformation("Retrieved {ItemCount} items from section: {SectionId}", 
            items.Count, request.SectionId);
        return _responseHandler.Success(responseData, "Items retrieved successfully.");
    }
}
