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

        try
        {
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

            var sectionResults = sections.Select(x =>
            {
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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetAllSectionsAsync cancelled for user: {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving sections for user: {UserId}", userId);
            return _responseHandler.InternalServerError<GetAllSectionsResponse>("An error occurred while retrieving sections.");
        }
    }

    public async Task<Response<GetSectionsOfCategoryResponse>> GetSectionsOfCategoryAsync(
        Guid userId,
        GetSectionsOfCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all sections for category: {CategoryId}.", request.CategoryId);

        try
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId
                    && c.Warehouse.UserId == userId, cancellationToken);

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

            var sectionResults = sections.Select(x =>
            {
                var mapped = _mapper.Map<GetSectionsOfCategoryResult>(x.Section);
                mapped.ItemCount = x.ItemCount;
                mapped.CreatedAt = DateTime.SpecifyKind(mapped.CreatedAt, DateTimeKind.Utc);
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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetSectionsOfCategoryAsync cancelled for Category: {CategoryId}", request.CategoryId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving sections for Category: {CategoryId}", request.CategoryId);
            return _responseHandler.InternalServerError<GetSectionsOfCategoryResponse>("An error occurred while retrieving sections.");
        }
    }

    public async Task<Response<GetSectionByIdResponse>> GetSectionByIdAsync(
        Guid userId,
        GetSectionByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving section with Id: {SectionId}", request.Id);

        try
        {
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
            response.CreatedAt = DateTime.SpecifyKind(response.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Section with Id: {SectionId} retrieved successfully", request.Id);
            return _responseHandler.Success(response, "Section retrieved successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetSectionByIdAsync cancelled for Section: {SectionId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving section: {SectionId}", request.Id);
            return _responseHandler.InternalServerError<GetSectionByIdResponse>("An error occurred while retrieving the section.");
        }
    }

    public async Task<Response<CreateSectionResponse>> CreateSectionAsync(
        Guid userId,
        CreateSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new section with name: {SectionName}", request.Name);

        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId
                    && c.Warehouse.UserId == userId, cancellationToken);

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

            await _context.AddAsync(section, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<CreateSectionResponse>(section);
            response.CreatedAt = DateTime.SpecifyKind(response.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Section created successfully with Id: {SectionId}", section.Id);
            return _responseHandler.Success(response, "Section created successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CreateSectionAsync cancelled for User: {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating section with name: {SectionName}", request.Name);
            return _responseHandler.InternalServerError<CreateSectionResponse>(
                "An error occurred while creating the section.");
        }
    }

    public async Task<Response<UpdateSectionResponse>> UpdateSectionAsync(
        Guid userId,
        UpdateSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating an existing section with Id: {SectionId}", request.Id);

        try
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.Id
                    && s.Category.Warehouse.UserId == userId, cancellationToken);

            if (section == null)
            {
                _logger.LogWarning("Section with Id: {SectionId} not found", request.Id);
                return _responseHandler.NotFound<UpdateSectionResponse>(
                    $"Section not found.");
            }

            section.Name = request.Name;
            await _context.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<UpdateSectionResponse>(section);

            _logger.LogInformation("Section update successfully with name: {SectionName}", section.Name);
            return _responseHandler.Success(response, "Section updated successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UpdateSectionAsync cancelled for Section: {SectionId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating section: {SectionId}", request.Id);
            return _responseHandler.InternalServerError<UpdateSectionResponse>("An error occurred while updating the section.");
        }
    }

    public async Task<Response<DeleteSectionResponse>> DeleteSectionAsync(
        Guid userId,
        DeleteSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting section with Id: {SectionId}", request.Id);

        try
        {
            var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == request.Id
                && s.Category.Warehouse.UserId == userId, cancellationToken);

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

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new DeleteSectionResponse
            {
                Id = section.Id
            };

            _logger.LogInformation("Section deleted successfully with Id: {SectionId}", section.Id);
            return _responseHandler.Success(response, "Section deleted successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DeleteSectionAsync cancelled for Section: {SectionId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting section: {SectionId}", request.Id);
            return _responseHandler.InternalServerError<DeleteSectionResponse>("An error occurred while deleting the section.");
        }
    }
}