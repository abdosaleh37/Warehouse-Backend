using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.Delete;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.GetById;
using Warehouse.Entities.DTO.Section.GetSectionsOfCategory;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public class SectionService : ISectionService
{
    private readonly ILogger<SectionService> _logger;
    private readonly IMapper _mapper;
    private readonly WarehouseDbContext _context;
    private readonly ResponseHandler _responseHandler;

    public SectionService(
        ILogger<SectionService> logger,
        IMapper mapper, 
        WarehouseDbContext context,
        ResponseHandler responseHandler)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _responseHandler = responseHandler;
    }

    public async Task<Response<GetAllSectionsResponse>> GetAllSectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all sections for user: {UserId}.", userId);

        var sections = await _context.Sections
            .AsNoTracking()
            .Where(s => s.Category.Warehouse.UserId == userId)
            .OrderBy(s => s.Category.CreatedAt)
                .ThenBy(s => s.CreatedAt)
            .Select(s => new
            {
                Section = s,
                CategoryName = s.Category.Name,
                ItemCount = s.Items.Count
            })
            .ToListAsync(cancellationToken);

        if (sections.Count == 0)
        {
            _logger.LogWarning("No sections found for user: {UserId}.", userId);
            var emptyResponse = new GetAllSectionsResponse
            {
                Sections = new List<GetAllSectionsResult>(),
                TotalSections = 0
            };
            return _responseHandler.Success(emptyResponse, "No sections found.");
        }

        var sectionResults = sections.Select(x => {
            var mapped = _mapper.Map<GetAllSectionsResult>(x.Section);
            mapped.CategoryName = x.CategoryName;
            mapped.ItemCount = x.ItemCount;
            return mapped;
        }).ToList();

        var response = new GetAllSectionsResponse
        {
            Sections = sectionResults,
            TotalSections = sectionResults.Count
        };
        
        _logger.LogInformation("Successfully retrieved {TotalSections} sections.", response.TotalSections);
        return _responseHandler.Success(response, "Sections retrieved successfully.");
    }

    public async Task<Response<GetSectionsOfCategoryResponse>> GetSectionsOfCategoryAsync(
        Guid userId,
        GetSectionsOfCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all sections for category: {CategoryId}.", request.CategoryId);

        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.Warehouse.UserId == userId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category with Id: {CategoryId} not found for user: {UserId}", 
                request.CategoryId, userId);
            return _responseHandler.NotFound<GetSectionsOfCategoryResponse>("Category not found.");
        }

        var sections = await _context.Sections
            .AsNoTracking()
            .Where(s => s.CategoryId == request.CategoryId)
            .OrderBy(s => s.CreatedAt)
            .Select(s => new
            {
                Section = s,
                ItemCount = s.Items.Count
            })
            .ToListAsync(cancellationToken);

        if (sections.Count == 0)
        {
            _logger.LogWarning("No sections found in category: {CategoryId}.", request.CategoryId);
            var emptyResponse = new GetSectionsOfCategoryResponse
            {
                Sections = new List<GetSectionsOfCategoryResult>(),
                TotalCount = 0,
                CategoryId = category.Id,
                CategoryName = category.Name
            };
            return _responseHandler.Success(emptyResponse, "No sections found.");
        }

        var sectionResults = sections.Select(x => {
            var mapped = _mapper.Map<GetSectionsOfCategoryResult>(x.Section);
            mapped.ItemCount = x.ItemCount;
            return mapped;
        }).ToList();

        var response = new GetSectionsOfCategoryResponse
        {
            Sections = sectionResults,
            TotalCount = sectionResults.Count,
            CategoryId = category.Id,
            CategoryName = category.Name
        };

        _logger.LogInformation("Successfully retrieved {TotalCount} sections.", response.TotalCount);
        return _responseHandler.Success(response, "Sections retrieved successfully.");
    }

    public async Task<Response<GetSectionByIdResponse>> GetSectionByIdAsync(
        Guid userId,
        GetSectionByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving section with Id: {SectionId}", request.Id);

        var sectionResult = await _context.Sections
            .AsNoTracking()
            .Where(s => s.Id == request.Id && s.Category.Warehouse.UserId == userId)
            .Select(s => new
            {
                Section = s,
                CategoryName = s.Category.Name,
                ItemCount = s.Items.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sectionResult == null)
        {
            _logger.LogWarning("Section with Id: {SectionId} not found", request.Id);
            return _responseHandler.NotFound<GetSectionByIdResponse>(
                $"Section not found.");
        }

        var response = _mapper.Map<GetSectionByIdResponse>(sectionResult.Section);
        response.CategoryName = sectionResult.CategoryName;
        response.ItemCount = sectionResult.ItemCount;

        _logger.LogInformation("Section with Id: {SectionId} retrieved successfully", request.Id);
        return _responseHandler.Success(response, "Section retrieved successfully");
    }

    public async Task<Response<CreateSectionResponse>> CreateSectionAsync(
        Guid userId,
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new section with name: {SectionName}", request.Name);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.Warehouse.UserId == userId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category with Id: {CategoryId} not found for user: {UserId}", 
                request.CategoryId, userId);
            return _responseHandler.NotFound<CreateSectionResponse>("Category not found.");
        }

        var section = new Section
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            CategoryId = category.Id
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
        Guid userId,
        UpdateSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating an existing section with Id: {SectionId}", request.Id);

        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.Category.Warehouse.UserId == userId, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section with Id: {SectionId} not found", request.Id);
            return _responseHandler.NotFound<UpdateSectionResponse>(
                $"Section not found.");
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
        Guid userId,
        DeleteSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting section with Id: {SectionId}", request.Id);

        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.Category.Warehouse.UserId == userId, cancellationToken);

        if (section == null)
        {
            _logger.LogWarning("Section with Id: {SectionId} not found", request.Id);
            return _responseHandler.NotFound<DeleteSectionResponse>(
                $"Section not found.");
        }

        var hasItems = await _context.Items
            .AsNoTracking()
            .AnyAsync(i => i.SectionId == section.Id, cancellationToken);

        if (hasItems)
        {
            _logger.LogWarning("Cannot delete section with Id: {SectionId} because it has associated items.", request.Id);
            return _responseHandler.BadRequest<DeleteSectionResponse>(
                "Cannot delete section because it has associated items.");
        }

        try
        {
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting section with Id: {SectionId}", request.Id);
            return _responseHandler.ServerError<DeleteSectionResponse>(
                "An error occurred while deleting the section.");
        }

        var response = new DeleteSectionResponse
        {
            Id = section.Id
        };

        _logger.LogInformation("Section deleted successfully with Id: {SectionId}", section.Id);
        return _responseHandler.Success(response, "Section deleted successfully");
    }
}
