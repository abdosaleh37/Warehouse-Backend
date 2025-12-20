namespace Warehouse.Entities.Entities;

public class Section
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
