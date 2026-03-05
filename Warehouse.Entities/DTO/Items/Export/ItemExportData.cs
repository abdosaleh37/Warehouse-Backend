using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.Export;

public class ItemExportData
{
    public string ItemCode { get; set; } = string.Empty;
    public string? PartNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public UnitOfMeasure Unit { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
