namespace Warehouse.Entities.Entities;

public class ItemVoucher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string VoucherCode { get; set; } = string.Empty;
    public int InQuantity { get; set; } = 0;
    public int OutQuantity { get; set; } = 0;
    public decimal UnitPrice { get; set; } = 0;
    public DateTime VoucherDate { get; set; }
    public string? Notes { get; set; }
    public Guid ItemId { get; set; }

    // Navigation property
    public virtual Item Item { get; set; } = null!;
}
