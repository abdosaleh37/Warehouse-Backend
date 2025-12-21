using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.SectionService;

public interface ISectionService
{
    Task<Response<CreateSectionResponse>> CreateSectionAsync(
        CreateSectionRequest request, 
        CancellationToken cancellationToken = default);
}
