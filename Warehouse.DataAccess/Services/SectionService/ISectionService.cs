using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.Delete;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.GetById;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public interface ISectionService
{
    Task<Response<GetAllSectionsResponse>> GetAllSectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Response<GetSectionByIdResponse>> GetSectionByIdAsync(
        Guid userId,
        GetSectionByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CreateSectionResponse>> CreateSectionAsync(
        Guid userId,
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default);

    Task<Response<UpdateSectionResponse>> UpdateSectionAsync(
        Guid userId,
        UpdateSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<DeleteSectionResponse>> DeleteSectionAsync(
        Guid userId,
        DeleteSectionRequest request,
        CancellationToken cancellationToken = default);
}