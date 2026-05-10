using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> entity)
    {
        entity.ToTable("PurchaseInvoices");
        entity.HasKey(pi => pi.Id);
        entity.Property(pi => pi.Id).ValueGeneratedOnAdd();
        entity.Property(pi => pi.TotalAmount).HasColumnType("decimal(18,2)");
        entity.Property(pi => pi.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(pi => pi.Notes).HasMaxLength(500);
        entity.Property(pi => pi.CreatedAt).IsRequired();
        entity.HasOne(pi => pi.Vendor)
              .WithMany()
              .HasForeignKey(pi => pi.VendorId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}