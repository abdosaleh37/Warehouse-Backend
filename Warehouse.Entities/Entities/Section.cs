namespace Warehouse.Entities.Entities;

public class Section
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CategoryId { get; set; }

    // Navigation property
    public Category Category { get; set; } = null!;
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
