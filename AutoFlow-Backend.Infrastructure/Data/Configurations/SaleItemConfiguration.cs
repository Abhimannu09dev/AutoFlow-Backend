using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> entity)
    {
        entity.ToTable("SaleItems");
        entity.HasKey(si => si.Id);
        entity.Property(si => si.Id).ValueGeneratedOnAdd();
        entity.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
        entity.Property(si => si.SubTotal).HasColumnType("decimal(18,2)");
        entity.HasOne(si => si.Part)
              .WithMany()
              .HasForeignKey(si => si.PartId)
              .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(si => si.Sale)
              .WithMany(s => s.SaleItems)
              .HasForeignKey(si => si.SaleId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}