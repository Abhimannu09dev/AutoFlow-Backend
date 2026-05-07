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
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

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
            entity.Property(c => c.ApplicationUserId).IsRequired(false);
            entity.HasIndex(c => c.ApplicationUserId);
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

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staffs");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StaffCode).IsRequired().HasMaxLength(30);
            entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(200);
            entity.Property(s => s.PhoneNumber).HasMaxLength(30);
            entity.Property(s => s.Address).HasMaxLength(300);
            entity.Property(s => s.Position).HasMaxLength(100);
            entity.Property(s => s.IsActive).HasDefaultValue(true);
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.UpdatedAt).IsRequired(false);
            entity.HasIndex(s => s.ApplicationUserId).IsUnique();
            entity.HasIndex(s => s.StaffCode).IsUnique();
            entity.HasIndex(s => s.Email);
            entity.HasOne<ApplicationUser>()
                .WithOne(user => user.StaffProfile)
                .HasForeignKey<Staff>(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(20);
            entity.Property(v => v.Brand).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Mileage).IsRequired().HasDefaultValue(0);
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

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CustomerId).IsRequired();
            entity.Property(a => a.Date).IsRequired();
            entity.Property(a => a.Time).IsRequired();
            entity.Property(a => a.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(a => a.CreatedAt).IsRequired();
            entity.HasIndex(a => a.CustomerId);
            entity.HasIndex(a => a.Date);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(s => s.DiscountAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.Property(s => s.SaleDate).IsRequired();
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.HasOne(s => s.Customer)
                  .WithMany()
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(s => s.SaleItems)
                  .WithOne(si => si.Sale)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");
            entity.HasKey(si => si.Id);
            entity.Property(si => si.Id).ValueGeneratedOnAdd();
            entity.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(si => si.SubTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(si => si.Quantity).IsRequired();
            entity.HasOne(si => si.Part)
                  .WithMany()
                  .HasForeignKey(si => si.PartId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PartRequest>(entity =>
        {
            entity.ToTable("PartRequests");
            entity.HasKey(pr => pr.Id);
            entity.Property(pr => pr.CustomerId).IsRequired();
            entity.Property(pr => pr.PartName).IsRequired().HasMaxLength(150);
            entity.Property(pr => pr.Quantity).IsRequired();
            entity.Property(pr => pr.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(pr => pr.CreatedAt).IsRequired();
            entity.HasIndex(pr => pr.CustomerId);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.CustomerId).IsRequired();
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(1000);
            entity.Property(r => r.CreatedAt).IsRequired();
            entity.HasIndex(r => r.CustomerId);
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.ToTable("PurchaseInvoices");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.Notes).HasMaxLength(500);
            entity.Property(p => p.InvoiceDate).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(p => p.Items)
                  .WithOne(i => i.PurchaseInvoice)
                  .HasForeignKey(i => i.PurchaseInvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("PurchaseInvoiceItems");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.UnitCost).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(i => i.SubTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(i => i.Quantity).IsRequired();
            entity.HasOne(i => i.Part)
                  .WithMany()
                  .HasForeignKey(i => i.PartId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
