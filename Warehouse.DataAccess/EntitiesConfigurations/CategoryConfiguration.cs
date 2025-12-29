using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.EntitiesConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.WarehouseId)
            .IsRequired();

        builder.HasIndex(c => new { c.WarehouseId, c.Name })
            .IsUnique();

        builder.HasOne(c => c.Warehouse)
            .WithMany(w => w.Categories)
            .HasForeignKey(c => c.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Sections)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
