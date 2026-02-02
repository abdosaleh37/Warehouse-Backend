using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.Items.Search;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemService;

public interface IItemService
{
    Task<Response<GetItemsOfSectionResponse>> GetItemsofSectionAsync(
        Guid userId,
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<GetItemByIdResponse>> GetByIdAsync(
        Guid userId,
        GetItemByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<GetItemsWithVouchersOfMonthResponse>> GetItemsWithVouchersOfMonthAsync(
        Guid userId,
        GetItemsWithVouchersOfMonthRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<SearchItemsResponse>> SearchItemsAsync(
        Guid userId,
        SearchItemsRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CreateItemResponse>> CreateItemAsync(
        Guid userId,
        CreateItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<UpdateItemResponse>> UpdateItemAsync(
        Guid userId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<DeleteItemResponse>> DeleteItemAsync(
        Guid userId,
        DeleteItemRequest request,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportMonthlyItemsToExcelAsync(
        Guid userId,
        GetItemsWithVouchersOfMonthRequest request,
        CancellationToken cancellationToken = default);
}
