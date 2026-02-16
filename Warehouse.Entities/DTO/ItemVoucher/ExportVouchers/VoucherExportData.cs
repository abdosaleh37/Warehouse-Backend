using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;

public class VoucherExportData
{
    public DateTime VoucherDate { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public List<VoucherItemData> Items { get; set; } = new();
}

public class VoucherItemData
{
    public string ItemPartNo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public UnitOfMeasure Unit { get; set; }
    public string SectionName { get; set; } = string.Empty;
}
