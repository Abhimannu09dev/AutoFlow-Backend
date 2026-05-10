using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class PartRequestConfiguration : IEntityTypeConfiguration<PartRequest>
{
    public void Configure(EntityTypeBuilder<PartRequest> entity)
    {
        entity.ToTable("PartRequests");
        entity.HasKey(pr => pr.Id);
        entity.Property(pr => pr.Id).ValueGeneratedOnAdd();
        entity.Property(pr => pr.PartName).IsRequired().HasMaxLength(150);
        entity.Property(pr => pr.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(pr => pr.CreatedAt).IsRequired();
        entity.HasOne<Customer>()
              .WithMany()
              .HasForeignKey(pr => pr.CustomerId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}