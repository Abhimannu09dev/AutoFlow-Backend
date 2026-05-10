using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> entity)
    {
        entity.ToTable("Vehicles");
        entity.HasKey(v => v.Id);
        entity.Property(v => v.Id).ValueGeneratedOnAdd();
        entity.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(20);
        entity.Property(v => v.Brand).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Color).HasMaxLength(30);
        entity.Property(v => v.VIN).HasMaxLength(50);
        entity.Property(v => v.CreatedAt).IsRequired();
        entity.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey(v => v.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}