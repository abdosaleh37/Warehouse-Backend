using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.Entities;

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Foreign key
    public Guid SectionId { get; set; }
    
    public string ItemCode { get; set; } = string.Empty;
    public string? PartNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Piece;

    public decimal OpeningQuantity { get; set; }
    
    public decimal OpeningValue { get; set; }
    
    public DateTime OpeningDate { get; set; }

    // Navigation properties
    public virtual Section Section { get; set; } = null!;
    public virtual ICollection<ItemVoucher> ItemVouchers { get; set; } = new List<ItemVoucher>();
}
