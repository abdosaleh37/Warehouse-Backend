namespace Warehouse.Entities.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
