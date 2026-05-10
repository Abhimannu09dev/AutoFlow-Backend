using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> entity)
    {
        entity.ToTable("PurchaseInvoiceItems");
        entity.HasKey(pii => pii.Id);
        entity.Property(pii => pii.Id).ValueGeneratedOnAdd();
        entity.Property(pii => pii.UnitCost).HasColumnType("decimal(18,2)");
        entity.Property(pii => pii.SubTotal).HasColumnType("decimal(18,2)");
        entity.HasOne(pii => pii.Part)
              .WithMany()
              .HasForeignKey(pii => pii.PartId)
              .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(pii => pii.PurchaseInvoice)
              .WithMany(pi => pi.Items)
              .HasForeignKey(pii => pii.PurchaseInvoiceId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}