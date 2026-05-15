using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class CreditPaymentConfiguration : IEntityTypeConfiguration<CreditPayment>
{
    public void Configure(EntityTypeBuilder<CreditPayment> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cp => cp.PaymentDate)
            .IsRequired();

        builder.Property(cp => cp.PaymentMethod)
            .HasConversion(new EnumToStringConverter<PaymentMethod>())
            .IsRequired();

        builder.Property(cp => cp.Note)
            .HasMaxLength(500);

        builder.Property(cp => cp.CreatedAt)
            .IsRequired();

        builder.HasOne(cp => cp.Sale)
            .WithMany(s => s.CreditPayments)
            .HasForeignKey(cp => cp.SaleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
