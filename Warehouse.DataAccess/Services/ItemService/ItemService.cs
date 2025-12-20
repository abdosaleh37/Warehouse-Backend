using MapsterMapper;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemService;

public class ItemService : IItemService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;

    public ItemService(WarehouseDbContext context, IMapper mapper, ResponseHandler responseHandler)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
    }

}
