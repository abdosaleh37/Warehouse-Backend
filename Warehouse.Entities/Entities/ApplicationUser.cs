using Microsoft.AspNetCore.Identity;

namespace Warehouse.Entities.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Warehouse? Warehouse { get; set; }
}
