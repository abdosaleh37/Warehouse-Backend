using MapsterMapper;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemVoucherService;

public class ItemVoucherService : IItemVoucherService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;

    public ItemVoucherService(WarehouseDbContext context, IMapper mapper, ResponseHandler responseHandler)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
    }

}
