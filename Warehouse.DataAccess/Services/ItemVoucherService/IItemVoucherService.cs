using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.Delete;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.DTO.ItemVoucher.Update;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.ItemVoucherService;

public interface IItemVoucherService
{
    Task<Response<GetVouchersOfItemResponse>> GetVouchersOfItemAsync(
        Guid userId,
        GetVouchersOfItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<GetVoucherByIdResponse>> GetVoucherByIdAsync(
        Guid userId,
        GetVoucherByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<GetMonthlyVouchersOfItemResponse>> GetMonthlyVouchersOfItemAsync(
        Guid userId,
        GetMonthlyVouchersOfItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<CreateVoucherResponse>> CreateVoucherAsync(
        Guid userId,
        CreateVoucherRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<UpdateVoucherResponse>> UpdateVoucherAsync(
        Guid userId,
        UpdateVoucherRequest request,
        CancellationToken cancellationToken = default);

    Task<Response<DeleteVoucherResponse>> DeleteVoucherAsync(
        Guid userId,
        DeleteVoucherRequest request,
        CancellationToken cancellationToken = default);
}
