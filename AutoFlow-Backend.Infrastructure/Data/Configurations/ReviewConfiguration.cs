using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> entity)
    {
        entity.ToTable("Reviews");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.Id).ValueGeneratedOnAdd();
        entity.Property(r => r.Rating).IsRequired();
        entity.Property(r => r.Comment).HasMaxLength(1000);
        entity.Property(r => r.CreatedAt).IsRequired();
        entity.HasOne<Customer>()
              .WithMany()
              .HasForeignKey(r => r.CustomerId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}