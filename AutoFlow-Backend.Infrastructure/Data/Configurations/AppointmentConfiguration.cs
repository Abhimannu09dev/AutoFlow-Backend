using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFlow_Backend.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {
        entity.ToTable("Appointments");
        entity.HasKey(a => a.Id);
        entity.Property(a => a.Id).ValueGeneratedOnAdd();
        entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(a => a.CreatedAt).IsRequired();
        entity.HasOne<Customer>()
              .WithMany()
              .HasForeignKey(a => a.CustomerId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}