using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> entity)
    {
        entity.ToTable("Parts");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Id).ValueGeneratedOnAdd();
        entity.Property(p => p.PartName).IsRequired().HasMaxLength(150);
        entity.Property(p => p.PartNumber).IsRequired().HasMaxLength(50);
        entity.Property(p => p.Description).HasMaxLength(500);
        entity.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
        entity.Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");
        entity.Property(p => p.CreatedAt).IsRequired();
        entity.HasOne(p => p.Vendor)
              .WithMany()
              .HasForeignKey(p => p.VendorId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}