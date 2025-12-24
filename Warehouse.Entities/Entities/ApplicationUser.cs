using Microsoft.AspNetCore.Identity;

namespace Warehouse.Entities.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Warehouse? Warehouse { get; set; }
}
