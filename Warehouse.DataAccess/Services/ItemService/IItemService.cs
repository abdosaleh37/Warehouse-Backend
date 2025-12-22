using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemService;

public interface IItemService
{
    Task<Response<GetItemsOfSectionResponse>> GetItemsofSectionAsync(
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CreateItemResponse>> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default);
}
