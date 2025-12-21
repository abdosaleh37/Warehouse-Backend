using MapsterMapper;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public class SectionService : ISectionService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<SectionService> _logger;

    public SectionService(
        WarehouseDbContext context, 
        IMapper mapper, 
        ResponseHandler responseHandler,
        ILogger<SectionService> logger)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    public async Task<Response<CreateSectionResponse>> CreateSectionAsync(
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new section with name: {SectionName}", request.Name);

        var section = new Section
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        try
        {
            await _context.AddAsync(section, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating section with name: {SectionName}", request.Name);
            return _responseHandler.ServerError<CreateSectionResponse>(
                "An error occurred while creating the section.");
        }

        var response = _mapper.Map<CreateSectionResponse>(section);

        _logger.LogInformation("Section created successfully with ID: {SectionId}", section.Id);
        return _responseHandler.Success(response, "Section created successfully");
    }
}
