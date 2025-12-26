using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Warehouse.Entities.Entities;
using WarehouseEntity = Warehouse.Entities.Entities.Warehouse;

namespace Warehouse.DataAccess.ApplicationDbContext;

public class WarehouseDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options)
    {
    }

    public DbSet<Section> Sections { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemVoucher> ItemVouchers { get; set; }
    public DbSet<WarehouseEntity> Warehouses { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
