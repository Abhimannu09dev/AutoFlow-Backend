using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Data;

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
            entity.HasIndex(c => c.Email).IsUnique();
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Customer>(c => c.ApplicationUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).ValueGeneratedOnAdd();
            entity.Property(v => v.VendorName).IsRequired().HasMaxLength(150);
            entity.Property(v => v.Email).HasMaxLength(200);
            entity.Property(v => v.Phone).HasMaxLength(30);
            entity.Property(v => v.Address).HasMaxLength(300);
            entity.HasIndex(v => v.Email).IsUnique();
            entity.Property(v => v.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.PartName).IsRequired().HasMaxLength(150);
            entity.Property(p => p.PartNumber).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staff");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.StaffCode).IsRequired().HasMaxLength(30);
            entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(200);
            entity.Property(s => s.PhoneNumber).HasMaxLength(30);
            entity.Property(s => s.Address).HasMaxLength(300);
            entity.Property(s => s.Position).HasMaxLength(100);
            entity.Property(s => s.IsActive).IsRequired();
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.HasIndex(s => s.StaffCode).IsUnique();
            entity.HasIndex(s => s.Email).IsUnique();
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Staff>(s => s.ApplicationUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).ValueGeneratedOnAdd();
            entity.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(20);
            entity.Property(v => v.Brand).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Color).HasMaxLength(30);
            entity.Property(v => v.VIN).HasMaxLength(50);
            entity.Property(v => v.CreatedAt).IsRequired();
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
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
        });

        modelBuilder.Entity<Sale>(entity =>
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
            entity.HasOne(s => s.Customer)
                  .WithMany()
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Staff>()
                  .WithMany()
                  .HasForeignKey(s => s.StaffId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");
            entity.HasKey(si => si.Id);
            entity.Property(si => si.Id).ValueGeneratedOnAdd();
            entity.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(si => si.SubTotal).HasColumnType("decimal(18,2)");
            entity.HasOne(si => si.Part)
                  .WithMany()
                  .HasForeignKey(si => si.PartId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Sale>()
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartRequest>(entity =>
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
        });

        modelBuilder.Entity<Review>(entity =>
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
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.ToTable("PurchaseInvoices");
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.Id).ValueGeneratedOnAdd();
            entity.Property(pi => pi.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(pi => pi.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(pi => pi.Notes).HasMaxLength(500);
            entity.Property(pi => pi.CreatedAt).IsRequired();
            entity.HasOne(pi => pi.Vendor)
                  .WithMany()
                  .HasForeignKey(pi => pi.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("PurchaseInvoiceItems");
            entity.HasKey(pii => pii.Id);
            entity.Property(pii => pii.Id).ValueGeneratedOnAdd();
            entity.Property(pii => pii.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(pii => pii.SubTotal).HasColumnType("decimal(18,2)");
            entity.HasOne(pii => pii.Part)
                  .WithMany()
                  .HasForeignKey(pii => pii.PartId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PurchaseInvoice>()
                  .WithMany(pi => pi.Items)
                  .HasForeignKey(pii => pii.PurchaseInvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}