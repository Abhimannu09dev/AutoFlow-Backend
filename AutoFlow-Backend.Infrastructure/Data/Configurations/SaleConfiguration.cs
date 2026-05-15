using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> entity)
    {
        entity.ToTable("Sales");
        entity.HasKey(s => s.Id);
        entity.Property(s => s.Id).ValueGeneratedOnAdd();
        entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)");
        entity.Property(s => s.DiscountAmount).HasColumnType("decimal(18,2)");
        entity.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        entity.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(s => s.Notes).HasMaxLength(500);
        entity.Property(s => s.CreatedAt).IsRequired();
        entity.Property(s => s.CreditStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(s => s.DueDate);

        entity.HasMany(s => s.CreditPayments)
            .WithOne(cp => cp.Sale)
            .HasForeignKey(cp => cp.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(s => s.Customer)
              .WithMany()
              .HasForeignKey(s => s.CustomerId)
              .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(s => s.Staff)
              .WithMany()
              .HasForeignKey(s => s.StaffId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}