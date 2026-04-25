using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vendor> Vendors => Set<Vendor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.HasKey(v => v.Id);

            entity.Property(v => v.VendorName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(v => v.ContactPerson)
                .HasMaxLength(100);

            entity.Property(v => v.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(v => v.Email)
                .HasMaxLength(200);

            entity.Property(v => v.Address)
                .HasMaxLength(300);

            entity.Property(v => v.IsActive)
                .HasDefaultValue(true);

            entity.Property(v => v.CreatedAt)
                .IsRequired();

            entity.HasIndex(v => v.VendorName);
            entity.HasIndex(v => v.Phone);
        });

        base.OnModelCreating(modelBuilder);
    }
}
