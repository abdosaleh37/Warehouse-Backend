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

    public int OpeningQuantity { get; set; } = 0;

    public decimal OpeningUnitPrice { get; set; } = 0;

    public DateTime OpeningDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Section Section { get; set; } = null!;
    public virtual ICollection<ItemVoucher> ItemVouchers { get; set; } = new List<ItemVoucher>();
}
