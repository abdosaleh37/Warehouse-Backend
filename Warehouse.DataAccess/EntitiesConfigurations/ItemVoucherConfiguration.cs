using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.EntitiesConfigurations;

public class ItemVoucherConfiguration : IEntityTypeConfiguration<ItemVoucher>
{
    public void Configure(EntityTypeBuilder<ItemVoucher> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(v => v.VoucherCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.VoucherCode);

        builder.Property(v => v.InQuantity)
            .IsRequired()
            .HasPrecision(18, 3)
            .HasDefaultValue(0);

        builder.Property(v => v.OutQuantity)
            .IsRequired()
            .HasPrecision(18, 3)
            .HasDefaultValue(0);

        builder.Property(v => v.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(v => v.VoucherDate)
            .IsRequired();

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        builder.HasOne(v => v.Item)
            .WithMany(i => i.ItemVouchers)
            .HasForeignKey(v => v.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.ItemId, v.VoucherDate });
    }
}
