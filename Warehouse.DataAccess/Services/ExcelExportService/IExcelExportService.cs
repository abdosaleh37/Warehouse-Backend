using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
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
}

public class ItemExportData
{
    public string ItemCode { get; set; } = string.Empty;
    public string? PartNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public UnitOfMeasure Unit { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
