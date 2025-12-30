using Warehouse.Entities.DTO.ItemVoucher;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemVoucherService;

public interface IItemVoucherService
{
    Task<Response<GetVouchersOfItemResponse>> GetVouchersOfItemAsync(
        Guid userId,
        GetVouchersOfItemRequest request,
        CancellationToken cancellationToken = default);
}
