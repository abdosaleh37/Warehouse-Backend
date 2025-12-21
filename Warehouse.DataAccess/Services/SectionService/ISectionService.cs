using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.Delete;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public interface ISectionService
{
    Task<Response<GetAllSectionsResponse>> GetAllSectionsAsync(
        CancellationToken cancellationToken = default);

    Task<Response<CreateSectionResponse>> CreateSectionAsync(
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default);

    Task<Response<UpdateSectionResponse>> UpdateSectionAsync(
        UpdateSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<DeleteSectionResponse>> DeleteSectionAsync(
        DeleteSectionRequest request,
        CancellationToken cancellationToken = default);
}