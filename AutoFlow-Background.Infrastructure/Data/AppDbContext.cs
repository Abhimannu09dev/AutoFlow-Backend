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
    public DbSet<Part> Parts => Set<Part>();

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

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.PartName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.PartNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Brand)
                .HasMaxLength(100);

            entity.Property(p => p.Category)
                .HasMaxLength(100);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.UnitPrice)
                .HasColumnType("numeric(18,2)");

            entity.Property(p => p.SellingPrice)
                .HasColumnType("numeric(18,2)");

            entity.Property(p => p.MinimumStockLevel)
                .HasDefaultValue(10);

            entity.Property(p => p.IsActive)
                .HasDefaultValue(true);

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => p.PartName);
            entity.HasIndex(p => p.PartNumber);
            entity.HasIndex(p => p.VendorId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
