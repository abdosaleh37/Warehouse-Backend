namespace Warehouse.Entities.Entities;

public class Section
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid WarehouseId { get; set; }

    // Navigation property
    public Warehouse? Warehouse { get; set; }
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
