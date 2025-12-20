using MapsterMapper;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public class SectionService : ISectionService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;

    public SectionService(WarehouseDbContext context, IMapper mapper, ResponseHandler responseHandler)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
    }

}
