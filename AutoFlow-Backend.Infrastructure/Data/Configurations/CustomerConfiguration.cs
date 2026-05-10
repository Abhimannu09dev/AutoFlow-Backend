using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.ToTable("Customers");
        entity.HasKey(c => c.Id);
        entity.Property(c => c.Id).ValueGeneratedOnAdd();
        entity.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
        entity.Property(c => c.Phone).HasMaxLength(30);
        entity.Property(c => c.Address).HasMaxLength(300);
        entity.HasIndex(c => c.Email).IsUnique();
        entity.Property(c => c.CreatedAt).IsRequired();
        entity.HasOne<ApplicationUser>()
              .WithOne()
              .HasForeignKey<Customer>(c => c.ApplicationUserId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}