using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.Delete;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.Update;
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

    public async Task<Response<GetAllSectionsResponse>> GetAllSectionsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all sections from the database.");
        var sections = await _context.Sections
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (sections.Count == 0)
        {
            _logger.LogWarning("No sections found in the database.");
            var emptyResponse = new GetAllSectionsResponse
            {
                Sections = new List<GetAllSectionsResult>(),
                TotalSections = 0
            };
            return _responseHandler.Success(emptyResponse, "No sections found.");
        }

        var sectionResults = _mapper.Map<List<GetAllSectionsResult>>(sections);
        var response = new GetAllSectionsResponse
        {
            Sections = sectionResults,
            TotalSections = sectionResults.Count
        };
        
        _logger.LogInformation("Successfully retrieved {TotalSections} sections.", response.TotalSections);
        return _responseHandler.Success(response, "Sections retrieved successfully.");
    }

    public async Task<Response<CreateSectionResponse>> CreateSectionAsync(
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new section with name: {SectionName}", request.Name);

        var section = new Section
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
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

        _logger.LogInformation("Section created successfully with Id: {SectionId}", section.Id);
        return _responseHandler.Success(response, "Section created successfully");
    }

    public async Task<Response<UpdateSectionResponse>> UpdateSectionAsync(
        UpdateSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating an existing section with Id: {SectionId}", request.Id);

        var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section with Id: {SectionId} not found", request.Id);
            return _responseHandler.NotFound<UpdateSectionResponse>(
                $"Section with Id {request.Id} not found.");
        }

        section.Name = request.Name;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while Updating section with Id: {SectionId}", request.Id);
            return _responseHandler.ServerError<UpdateSectionResponse>(
                "An error occurred while updating the section.");
        }

        var response = _mapper.Map<UpdateSectionResponse>(section);

        _logger.LogInformation("Section update successfully with name: {SectionName}", section.Name);
        return _responseHandler.Success(response, "Section updated successfully");
    }

    public async Task<Response<DeleteSectionResponse>> DeleteSectionAsync(
        DeleteSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting section with Id: {SectionId}", request.SectionId);
        var section = await _context.Sections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section with Id: {SectionId} not found", request.SectionId);
            return _responseHandler.NotFound<DeleteSectionResponse>(
                $"Section with Id {request.SectionId} not found.");
        }

        // Check if section has items
        if (section.Items.Any())
        {
            _logger.LogWarning("Cannot delete section with Id: {SectionId} because it has {ItemCount} items", 
                request.SectionId, section.Items.Count);
            return _responseHandler.BadRequest<DeleteSectionResponse>(
                $"Cannot delete section '{section.Name}' because it contains {section.Items.Count} item(s). Please remove or reassign the items first.");
        }

        try
        {
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting section with Id: {SectionId}", request.SectionId);
            return _responseHandler.ServerError<DeleteSectionResponse>(
                "An error occurred while deleting the section.");
        }

        var response = new DeleteSectionResponse
        {
            SectionId = section.Id
        };

        _logger.LogInformation("Section deleted successfully with Id: {SectionId}", section.Id);
        return _responseHandler.Success(response, "Section deleted successfully");
    }
}
