using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemService;

public interface IItemService
{
    Task<Response<GetItemsOfSectionResponse>> GetItemsofSectionAsync(
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<GetItemByIdResponse>> GetByIdAsync(
        GetItemByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CreateItemResponse>> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<UpdateItemResponse>> UpdateItemAsync(
        UpdateItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<DeleteItemResponse>> DeleteItemAsync(
        DeleteItemRequest request,
        CancellationToken cancellationToken = default);
}
