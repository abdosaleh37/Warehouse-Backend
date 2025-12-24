using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.DataAccess.EntitiesConfigurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Entities.Entities.Warehouse>
{
    public void Configure(EntityTypeBuilder<Entities.Entities.Warehouse> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.UserId)
            .IsRequired();

        // One-to-one relationship between Warehouse and ApplicationUser
        builder.HasIndex(w => w.UserId)
            .IsUnique();

        builder.HasOne(w => w.User)
            .WithOne(u => u.Warehouse)
            .HasForeignKey<Entities.Entities.Warehouse>(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
