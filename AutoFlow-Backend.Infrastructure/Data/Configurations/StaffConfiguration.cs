using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> entity)
    {
        entity.ToTable("Staff");
        entity.HasKey(s => s.Id);
        entity.Property(s => s.Id).ValueGeneratedOnAdd();
        entity.Property(s => s.StaffCode).IsRequired().HasMaxLength(30);
        entity.Property(s => s.FullName).IsRequired().HasMaxLength(200);
        entity.Property(s => s.Email).IsRequired().HasMaxLength(200);
        entity.Property(s => s.PhoneNumber).HasMaxLength(30);
        entity.Property(s => s.Address).HasMaxLength(300);
        entity.Property(s => s.Position).HasMaxLength(100);
        entity.Property(s => s.IsActive).IsRequired();
        entity.Property(s => s.CreatedAt).IsRequired();
        entity.HasIndex(s => s.StaffCode).IsUnique();
        entity.HasIndex(s => s.Email).IsUnique();
        entity.HasOne<ApplicationUser>()
              .WithOne()
              .HasForeignKey<Staff>(s => s.ApplicationUserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}