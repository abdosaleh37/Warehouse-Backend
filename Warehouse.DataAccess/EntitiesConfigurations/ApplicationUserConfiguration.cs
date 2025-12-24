using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Entities.Entities;
using WarehouseEntity = Warehouse.Entities.Entities.Warehouse;

namespace Warehouse.DataAccess.EntitiesConfigurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        // Configure one-to-one relationship with Warehouse if present
        builder.HasOne(u => u.Warehouse)
            .WithOne(w => w.User)
            .HasForeignKey<WarehouseEntity>(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
