using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.DataAccess.EntitiesConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(i => i.ItemCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.PartNo)
            .HasMaxLength(100);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Unit)
            .IsRequired()
            .HasDefaultValue(UnitOfMeasure.Piece)
            .HasConversion<string>();

        builder.Property(i => i.OpeningQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.OpeningUnitPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(i => i.OpeningDate)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(i => i.Section)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.ItemVouchers)
            .WithOne(v => v.Item)
            .HasForeignKey(v => v.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
