using AutoFlow_Background.Infrastructure.Entities;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.FullName).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(30);
            entity.Property(c => c.Address).HasMaxLength(300);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.VendorName).IsRequired().HasMaxLength(150);
            entity.Property(v => v.ContactPerson).HasMaxLength(100);
            entity.Property(v => v.Phone).IsRequired().HasMaxLength(20);
            entity.Property(v => v.Email).HasMaxLength(200);
            entity.Property(v => v.Address).HasMaxLength(300);
            entity.Property(v => v.IsActive).HasDefaultValue(true);
            entity.Property(v => v.CreatedAt).IsRequired();
            entity.HasIndex(v => v.VendorName);
            entity.HasIndex(v => v.Phone);
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.PartName).IsRequired().HasMaxLength(150);
            entity.Property(p => p.PartNumber).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Brand).HasMaxLength(100);
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.UnitPrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.SellingPrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.MinimumStockLevel).HasDefaultValue(10);
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(p => p.PartName);
            entity.HasIndex(p => p.PartNumber);
            entity.HasIndex(p => p.VendorId);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(20);
            entity.Property(v => v.Brand).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Color).HasMaxLength(30);
            entity.Property(v => v.VIN).HasMaxLength(50);
            entity.Property(v => v.CreatedAt).IsRequired();
            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(v => v.VehicleNumber);
            entity.HasIndex(v => v.UserId);
        });
    }
}