using Warehouse.Entities.DTO.Items.Export;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;
using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.DataAccess.Services.ExcelExportService;

public interface IExcelExportService
{
    Task<byte[]> ExportMonthlyItemsToExcelAsync(
        List<GetItemsWithVouchersOfMonthResult> items,
        int month,
        int year,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportAllItemsToExcelAsync(
        Dictionary<string, List<ItemExportData>> itemsBySections,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportVouchersToExcelAsync(
        List<VoucherExportData> vouchers,
        VoucherType voucherType,
        CancellationToken cancellationToken = default);
}
