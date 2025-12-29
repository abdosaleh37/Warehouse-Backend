namespace Warehouse.Entities.Entities;
public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
