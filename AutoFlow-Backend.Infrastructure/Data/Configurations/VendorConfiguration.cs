using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> entity)
    {
        entity.ToTable("Vendors");
        entity.HasKey(v => v.Id);
        entity.Property(v => v.Id).ValueGeneratedOnAdd();
        entity.Property(v => v.VendorName).IsRequired().HasMaxLength(150);
        entity.Property(v => v.ContactPerson).HasMaxLength(150);
        entity.Property(v => v.Email).HasMaxLength(200);
        entity.Property(v => v.Phone).HasMaxLength(30);
        entity.Property(v => v.Address).HasMaxLength(300);
        entity.HasIndex(v => v.Email).IsUnique();
        entity.Property(v => v.CreatedAt).IsRequired();
    }
}