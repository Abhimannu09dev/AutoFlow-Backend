using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using AutoFlow_Backend.Infrastructure.Data;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoFlow_Backend.Infrastructure.Seed;

public static class DevDatabaseSeeder
{
    private const string DefaultPassword = "Admin@12345";
    private static readonly Random Random = new(20260518);

    private static readonly string[] StaffPositions =
    [
        "Service Advisor",
        "Mechanic",
        "Technician",
        "Parts Coordinator",
        "Workshop Supervisor"
    ];

    private static readonly string[] VendorPrefixes =
    [
        "Apex", "Prime", "Metro", "Global", "North", "Rapid", "Silver", "Summit", "Trident", "Velocity"
    ];

    private static readonly string[] VendorSuffixes =
    [
        "Motors", "Auto Parts", "Supplies", "Components", "Traders", "Distributors", "Logistics", "Engineering"
    ];

    private static readonly string[] PartCategories =
    [
        "Engine", "Brake", "Suspension", "Electrical", "Tyres", "Fluids", "Filters", "Transmission", "Cooling"
    ];

    private static readonly string[] PartBrands =
    [
        "Bosch", "Denso", "NGK", "ACDelco", "Castrol", "Bridgestone", "Valeo", "Mann", "SKF", "Brembo"
    ];

    private static readonly (string Brand, string Model)[] VehicleModels =
    [
        ("Toyota", "Corolla"),
        ("Toyota", "RAV4"),
        ("Honda", "Civic"),
        ("Mazda", "CX-5"),
        ("Hyundai", "i30"),
        ("Tesla", "Model 3"),
        ("Ford", "Ranger"),
        ("Nissan", "X-Trail")
    ];

    private static readonly string[] Colors =
    [
        "White", "Black", "Silver", "Blue", "Red", "Grey", "Green"
    ];

    private static readonly string[] FirstNames =
    [
        "Aarav", "Olivia", "Noah", "Emma", "Liam", "Amelia", "Mason", "Sophia", "Ethan", "Mia",
        "Isla", "Lucas", "James", "Aria", "Logan", "Ella", "Ava", "Daniel", "Chloe", "Zoe"
    ];

    private static readonly string[] LastNames =
    [
        "Sharma", "Wilson", "Brown", "Taylor", "Walker", "Singh", "Nguyen", "Patel", "Ali", "Miller",
        "Johnson", "Davis", "Clark", "Evans", "Scott", "Young", "Allen", "Lee", "Baker", "Morgan"
    ];

    private sealed class SeedSummary
    {
        public int Users { get; set; }
        public int Staff { get; set; }
        public int Customers { get; set; }
        public int Vendors { get; set; }
        public int Parts { get; set; }
        public int Vehicles { get; set; }
        public int Appointments { get; set; }
        public int Sales { get; set; }
        public int SaleItems { get; set; }
        public int CreditPayments { get; set; }
        public int PurchaseInvoices { get; set; }
        public int PurchaseInvoiceItems { get; set; }
        public int PartRequests { get; set; }
        public int Reviews { get; set; }
    }

    public static async Task ResetAndSeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var hostEnvironment = scopedServices.GetRequiredService<IHostEnvironment>();
        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

        if (!hostEnvironment.IsDevelopment())
        {
            throw new InvalidOperationException("Refusing to reset database outside Development environment.");
        }

        Console.WriteLine("Starting development database reset and seed...");
        await dbContext.Database.MigrateAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var summary = new SeedSummary();

        await ClearDatabaseAsync(dbContext, cancellationToken);
        await SeedRolesAsync(roleManager);

        var seededUsers = await SeedUsersAsync(userManager, cancellationToken);
        summary.Users = seededUsers.Count;

        var staffProfiles = await SeedStaffAsync(dbContext, seededUsers.StaffUsers, cancellationToken);
        summary.Staff = staffProfiles.Count;

        var customers = await SeedCustomersAsync(dbContext, seededUsers.CustomerUsers, cancellationToken);
        summary.Customers = customers.Count;

        var vendors = await SeedVendorsAsync(dbContext, cancellationToken);
        summary.Vendors = vendors.Count;

        var parts = await SeedPartsAsync(dbContext, vendors, cancellationToken);
        summary.Parts = parts.Count;

        var vehicles = await SeedVehiclesAsync(dbContext, customers, cancellationToken);
        summary.Vehicles = vehicles.Count;

        var appointments = await SeedAppointmentsAsync(dbContext, customers, vehicles, cancellationToken);
        summary.Appointments = appointments.Count;

        var (sales, saleItems, creditPayments) = await SeedSalesAsync(dbContext, customers, staffProfiles, parts, cancellationToken);
        summary.Sales = sales.Count;
        summary.SaleItems = saleItems.Count;
        summary.CreditPayments = creditPayments.Count;

        var (purchaseInvoices, purchaseInvoiceItems) = await SeedPurchaseInvoicesAsync(
            dbContext,
            vendors,
            staffProfiles,
            parts,
            cancellationToken);
        summary.PurchaseInvoices = purchaseInvoices.Count;
        summary.PurchaseInvoiceItems = purchaseInvoiceItems.Count;

        var partRequests = await SeedPartRequestsAsync(dbContext, customers, cancellationToken);
        summary.PartRequests = partRequests.Count;

        var reviews = await SeedReviewsAsync(dbContext, customers, cancellationToken);
        summary.Reviews = reviews.Count;

        await transaction.CommitAsync(cancellationToken);
        PrintSummary(summary);
    }

    private static async Task ClearDatabaseAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.CreditPayments.ExecuteDeleteAsync(cancellationToken);
        await dbContext.SaleItems.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Sales.ExecuteDeleteAsync(cancellationToken);
        await dbContext.PurchaseInvoiceItems.ExecuteDeleteAsync(cancellationToken);
        await dbContext.PurchaseInvoices.ExecuteDeleteAsync(cancellationToken);
        await dbContext.PartRequests.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Reviews.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Appointments.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Vehicles.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Parts.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Vendors.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Staff.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Customers.ExecuteDeleteAsync(cancellationToken);

        await dbContext.Set<IdentityUserToken<Guid>>().ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<IdentityUserLogin<Guid>>().ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<IdentityUserClaim<Guid>>().ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<IdentityUserRole<Guid>>().ExecuteDeleteAsync(cancellationToken);
        await dbContext.Users.ExecuteDeleteAsync(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in new[] { "Admin", "Staff", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                EnsureSucceeded(createResult, $"Failed to create role '{role}'.");
            }
        }
    }

    private static async Task<(List<ApplicationUser> StaffUsers, List<ApplicationUser> CustomerUsers, ApplicationUser AdminUser, int Count)> SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var users = new List<ApplicationUser>();
        var staffUsers = new List<ApplicationUser>();
        var customerUsers = new List<ApplicationUser>();

        var admin = await CreateUserAsync(
            userManager,
            "admin@autoflow.local",
            "System Admin",
            "Sydney NSW",
            "Admin",
            cancellationToken);
        users.Add(admin);

        var staffSample = await CreateUserAsync(
            userManager,
            "staff@autoflow.local",
            "Staff Sample",
            "Sydney NSW",
            "Staff",
            cancellationToken);
        users.Add(staffSample);
        staffUsers.Add(staffSample);

        for (var i = 1; i <= 24; i++)
        {
            var name = BuildPersonName(i);
            var user = await CreateUserAsync(
                userManager,
                $"staff{i:D3}@autoflow.local",
                name,
                $"Workshop Zone {Random.Next(1, 8)}, Sydney",
                "Staff",
                cancellationToken);
            users.Add(user);
            staffUsers.Add(user);
        }

        var customerSample = await CreateUserAsync(
            userManager,
            "customer@autoflow.local",
            "Customer Sample",
            "Parramatta, Sydney",
            "Customer",
            cancellationToken);
        users.Add(customerSample);
        customerUsers.Add(customerSample);

        for (var i = 1; i <= 249; i++)
        {
            var name = BuildPersonName(1000 + i);
            var user = await CreateUserAsync(
                userManager,
                $"customer{i:D3}@autoflow.local",
                name,
                $"{Random.Next(10, 300)} Service St, Sydney",
                "Customer",
                cancellationToken);
            users.Add(user);
            customerUsers.Add(user);
        }

        return (staffUsers, customerUsers, admin, users.Count);
    }

    private static async Task<List<Staff>> SeedStaffAsync(
        AppDbContext dbContext,
        List<ApplicationUser> staffUsers,
        CancellationToken cancellationToken)
    {
        var createdAt = DateTime.UtcNow;
        var staff = new List<Staff>();

        for (var i = 0; i < staffUsers.Count; i++)
        {
            var user = staffUsers[i];
            staff.Add(new Staff
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                StaffCode = $"STF{i + 1:D3}",
                FullName = user.FullName,
                Email = user.Email ?? $"staff{i + 1:D3}@autoflow.local",
                PhoneNumber = $"04{Random.Next(10000000, 99999999)}",
                Address = user.Address ?? $"Service Bay {Random.Next(1, 20)}, Sydney",
                Position = StaffPositions[i % StaffPositions.Length],
                IsActive = i % 8 != 0,
                CreatedAt = createdAt.AddDays(-Random.Next(10, 730))
            });
        }

        await dbContext.Staff.AddRangeAsync(staff, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return staff;
    }

    private static async Task<List<Customer>> SeedCustomersAsync(
        AppDbContext dbContext,
        List<ApplicationUser> customerUsers,
        CancellationToken cancellationToken)
    {
        var customers = new List<Customer>(customerUsers.Count);

        for (var i = 0; i < customerUsers.Count; i++)
        {
            var user = customerUsers[i];
            customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                FullName = user.FullName,
                Email = user.Email ?? $"customer{i + 1:D3}@autoflow.local",
                Phone = $"04{Random.Next(10000000, 99999999)}",
                Address = user.Address ?? $"{Random.Next(1, 500)} Repair Ave, Sydney",
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(30, 900)),
                ApplicationUserId = user.Id
            });
        }

        await dbContext.Customers.AddRangeAsync(customers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customers;
    }

    private static async Task<List<Vendor>> SeedVendorsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var vendors = new List<Vendor>(250);

        for (var i = 1; i <= 250; i++)
        {
            var prefix = VendorPrefixes[(i - 1) % VendorPrefixes.Length];
            var suffix = VendorSuffixes[(i - 1) % VendorSuffixes.Length];
            vendors.Add(new Vendor
            {
                Id = Guid.NewGuid(),
                VendorName = $"{prefix} {suffix} {i:D3}",
                ContactPerson = BuildPersonName(i + 400),
                Phone = $"02{Random.Next(10000000, 99999999)}",
                Email = $"vendor{i:D3}@autoflow-supply.local",
                Address = $"{Random.Next(1, 120)} Industry Park Rd, Sydney",
                IsActive = i % 11 != 0,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(30, 1200))
            });
        }

        await dbContext.Vendors.AddRangeAsync(vendors, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return vendors;
    }

    private static async Task<List<Part>> SeedPartsAsync(
        AppDbContext dbContext,
        List<Vendor> vendors,
        CancellationToken cancellationToken)
    {
        var parts = new List<Part>(250);
        for (var i = 1; i <= 250; i++)
        {
            var category = PartCategories[(i - 1) % PartCategories.Length];
            var brand = PartBrands[(i - 1) % PartBrands.Length];
            var minimum = Random.Next(5, 20);
            var stockBucket = i % 5;
            var stock = stockBucket switch
            {
                0 => 0,
                1 => Random.Next(1, minimum + 1),
                _ => Random.Next(minimum + 1, minimum + 120)
            };

            var unitPrice = Math.Round((decimal)Random.NextDouble() * 280m + 20m, 2);
            var markup = Math.Round(unitPrice * (decimal)(0.20 + Random.NextDouble() * 0.35), 2);
            var sellingPrice = unitPrice + markup;

            parts.Add(new Part
            {
                Id = Guid.NewGuid(),
                PartName = $"{category} Component {i:D3}",
                PartNumber = $"PRT-{category[..Math.Min(3, category.Length)].ToUpperInvariant()}-{i:D4}",
                Brand = brand,
                Category = category,
                Description = $"Seeded {category.ToLowerInvariant()} part for integration testing.",
                UnitPrice = unitPrice,
                SellingPrice = sellingPrice,
                StockQuantity = stock,
                MinimumStockLevel = minimum,
                VendorId = vendors[(i - 1) % vendors.Count].Id,
                IsActive = i % 13 != 0,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(20, 1000))
            });
        }

        await dbContext.Parts.AddRangeAsync(parts, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return parts;
    }

    private static async Task<List<Vehicle>> SeedVehiclesAsync(
        AppDbContext dbContext,
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var vehicles = new List<Vehicle>(250);
        for (var i = 1; i <= 250; i++)
        {
            var model = VehicleModels[(i - 1) % VehicleModels.Length];
            var owner = customers[(i - 1) % customers.Count];

            vehicles.Add(new Vehicle
            {
                Id = Guid.NewGuid(),
                VehicleNumber = $"NSW-{Random.Next(100, 999)}-{i:D3}",
                Brand = model.Brand,
                Model = model.Model,
                Year = Random.Next(2009, 2026),
                Mileage = Random.Next(5000, 260000),
                Color = Colors[(i - 1) % Colors.Length],
                VIN = $"VIN{Random.Next(100000, 999999)}{i:D4}",
                UserId = owner.ApplicationUserId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(5, 900))
            });
        }

        await dbContext.Vehicles.AddRangeAsync(vehicles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return vehicles;
    }

    private static async Task<List<Appointment>> SeedAppointmentsAsync(
        AppDbContext dbContext,
        List<Customer> customers,
        List<Vehicle> vehicles,
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<AppointmentStatus>();
        var appointments = new List<Appointment>(250);

        for (var i = 1; i <= 250; i++)
        {
            var customer = customers[(i - 1) % customers.Count];
            var vehicle = vehicles[(i - 1) % vehicles.Count];
            var status = statuses[i % statuses.Length];
            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(Random.Next(-90, 90)));
            var hour = Random.Next(8, 17);
            var minute = Random.Next(0, 2) == 0 ? 0 : 30;

            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                Date = date,
                Time = new TimeOnly(hour, minute),
                Status = status,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(5, 200)),
                UpdatedAt = status == AppointmentStatus.Pending ? null : DateTime.UtcNow.AddDays(-Random.Next(1, 100))
            });
        }

        await dbContext.Appointments.AddRangeAsync(appointments, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return appointments;
    }

    private static async Task<(List<Sale> Sales, List<SaleItem> SaleItems, List<CreditPayment> CreditPayments)> SeedSalesAsync(
        AppDbContext dbContext,
        List<Customer> customers,
        List<Staff> staff,
        List<Part> parts,
        CancellationToken cancellationToken)
    {
        var sales = new List<Sale>(250);
        var saleItems = new List<SaleItem>();
        var creditPayments = new List<CreditPayment>();
        var statuses = Enum.GetValues<SaleStatus>();

        for (var i = 1; i <= 250; i++)
        {
            var customer = customers[(i - 1) % customers.Count];
            var staffMember = staff[(i - 1) % staff.Count];

            var paymentMethod = i % 4 == 0
                ? PaymentMethod.Credit
                : (i % 3 == 0 ? PaymentMethod.Card : PaymentMethod.Cash);

            var itemCount = Random.Next(1, 4);
            decimal subTotal = 0;
            var usedPartIndexes = new HashSet<int>();

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                StaffId = staffMember.Id,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{i:D4}",
                SaleDate = DateTime.UtcNow.AddDays(-Random.Next(1, 365)),
                PaymentMethod = paymentMethod,
                Status = statuses[i % statuses.Length],
                Notes = i % 7 == 0 ? "Customer requested urgent processing." : null,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 365))
            };

            for (var j = 0; j < itemCount; j++)
            {
                var partIndex = Random.Next(parts.Count);
                while (!usedPartIndexes.Add(partIndex))
                {
                    partIndex = Random.Next(parts.Count);
                }

                var selectedPart = parts[partIndex];
                var qty = Random.Next(1, 4);
                var rowSubTotal = selectedPart.SellingPrice * qty;
                subTotal += rowSubTotal;

                saleItems.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = sale.Id,
                    PartId = selectedPart.Id,
                    Quantity = qty,
                    UnitPrice = selectedPart.SellingPrice,
                    SubTotal = rowSubTotal
                });
            }

            if (i % 17 == 0)
            {
                subTotal += 5500m;
            }

            var discount = subTotal > 5000m
                ? Math.Round(subTotal * 0.10m, 2)
                : Math.Round(subTotal * (decimal)Random.NextDouble() * 0.05m, 2);

            sale.SubTotal = Math.Round(subTotal, 2);
            sale.DiscountAmount = discount;
            sale.TotalAmount = Math.Max(0m, Math.Round(subTotal - discount, 2));
            sale.UpdatedAt = DateTime.UtcNow.AddDays(-Random.Next(0, 200));

            if (sale.PaymentMethod == PaymentMethod.Credit)
            {
                sale.DueDate = sale.SaleDate.AddDays(30);
                sale.InvoiceEmail = customer.Email;

                var creditProfile = (i / 4) % 4;
                switch (creditProfile)
                {
                    case 0:
                        sale.CreditStatus = sale.DueDate.Value.Date < DateTime.UtcNow.Date
                            ? CreditStatus.Overdue
                            : CreditStatus.Outstanding;
                        break;
                    case 1:
                    {
                        var payment = Math.Round(sale.TotalAmount * 0.35m, 2);
                        creditPayments.Add(new CreditPayment
                        {
                            Id = Guid.NewGuid(),
                            SaleId = sale.Id,
                            Amount = payment,
                            PaymentDate = sale.SaleDate.AddDays(Random.Next(5, 20)),
                            PaymentMethod = PaymentMethod.Cash,
                            Note = "Partial payment",
                            CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(5, 120))
                        });
                        sale.CreditStatus = CreditStatus.PartiallyPaid;
                        break;
                    }
                    case 2:
                    {
                        var first = Math.Round(sale.TotalAmount * 0.45m, 2);
                        var second = sale.TotalAmount - first;
                        creditPayments.Add(new CreditPayment
                        {
                            Id = Guid.NewGuid(),
                            SaleId = sale.Id,
                            Amount = first,
                            PaymentDate = sale.SaleDate.AddDays(Random.Next(2, 12)),
                            PaymentMethod = PaymentMethod.Card,
                            Note = "Installment 1",
                            CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(10, 160))
                        });
                        creditPayments.Add(new CreditPayment
                        {
                            Id = Guid.NewGuid(),
                            SaleId = sale.Id,
                            Amount = second,
                            PaymentDate = sale.SaleDate.AddDays(Random.Next(15, 35)),
                            PaymentMethod = PaymentMethod.Card,
                            Note = "Installment 2",
                            CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 90))
                        });
                        sale.CreditStatus = CreditStatus.Paid;
                        break;
                    }
                    case 3:
                    {
                        var payment = Math.Round(sale.TotalAmount * 0.25m, 2);
                        creditPayments.Add(new CreditPayment
                        {
                            Id = Guid.NewGuid(),
                            SaleId = sale.Id,
                            Amount = payment,
                            PaymentDate = sale.SaleDate.AddDays(Random.Next(8, 28)),
                            PaymentMethod = PaymentMethod.Cash,
                            Note = "Late partial payment",
                            CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(5, 120))
                        });
                        sale.CreditStatus = CreditStatus.Overdue;
                        break;
                    }
                    default:
                        sale.CreditStatus = CreditStatus.Outstanding;
                        break;
                }
            }
            else
            {
                sale.CreditStatus = null;
                if (i % 5 == 0)
                {
                    sale.InvoiceSentAt = DateTime.UtcNow.AddDays(-Random.Next(1, 60));
                    sale.InvoiceEmail = customer.Email;
                    sale.InvoiceFailedAt = null;
                    sale.InvoiceFailureReason = null;
                }
                else if (i % 11 == 0)
                {
                    sale.InvoiceFailedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 40));
                    sale.InvoiceFailureReason = "SMTP temporary timeout.";
                    sale.InvoiceEmail = customer.Email;
                }
            }

            sales.Add(sale);
        }

        await dbContext.Sales.AddRangeAsync(sales, cancellationToken);
        await dbContext.SaleItems.AddRangeAsync(saleItems, cancellationToken);
        await dbContext.CreditPayments.AddRangeAsync(creditPayments, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (sales, saleItems, creditPayments);
    }

    private static async Task<(List<PurchaseInvoice> Invoices, List<PurchaseInvoiceItem> Items)> SeedPurchaseInvoicesAsync(
        AppDbContext dbContext,
        List<Vendor> vendors,
        List<Staff> staff,
        List<Part> parts,
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<PurchaseInvoiceStatus>();
        var invoices = new List<PurchaseInvoice>(250);
        var items = new List<PurchaseInvoiceItem>();

        for (var i = 1; i <= 250; i++)
        {
            var invoice = new PurchaseInvoice
            {
                Id = Guid.NewGuid(),
                VendorId = vendors[(i - 1) % vendors.Count].Id,
                CreatedByStaffId = staff[(i - 1) % staff.Count].Id,
                InvoiceDate = DateTime.UtcNow.AddDays(-Random.Next(2, 400)),
                Status = statuses[i % statuses.Length],
                Notes = i % 9 == 0 ? "Delayed shipment." : null,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(2, 400))
            };

            var lineCount = Random.Next(1, 4);
            decimal total = 0;
            for (var j = 0; j < lineCount; j++)
            {
                var part = parts[Random.Next(parts.Count)];
                var qty = Random.Next(1, 10);
                var unitCost = Math.Round(part.UnitPrice * (decimal)(0.8 + Random.NextDouble() * 0.25), 2);
                var subTotal = Math.Round(unitCost * qty, 2);
                total += subTotal;

                items.Add(new PurchaseInvoiceItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseInvoiceId = invoice.Id,
                    PartId = part.Id,
                    Quantity = qty,
                    UnitCost = unitCost,
                    SubTotal = subTotal
                });
            }

            invoice.TotalAmount = Math.Round(total, 2);
            invoices.Add(invoice);
        }

        await dbContext.PurchaseInvoices.AddRangeAsync(invoices, cancellationToken);
        await dbContext.PurchaseInvoiceItems.AddRangeAsync(items, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (invoices, items);
    }

    private static async Task<List<PartRequest>> SeedPartRequestsAsync(
        AppDbContext dbContext,
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<PartRequestStatus>();
        var requests = new List<PartRequest>(250);

        for (var i = 1; i <= 250; i++)
        {
            var category = PartCategories[(i - 1) % PartCategories.Length];
            requests.Add(new PartRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[(i - 1) % customers.Count].Id,
                PartName = $"{category} Request Part {i:D3}",
                Quantity = Random.Next(1, 6),
                Status = statuses[i % statuses.Length],
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 300)),
                UpdatedAt = i % 3 == 0 ? DateTime.UtcNow.AddDays(-Random.Next(0, 150)) : null
            });
        }

        await dbContext.PartRequests.AddRangeAsync(requests, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return requests;
    }

    private static async Task<List<Review>> SeedReviewsAsync(
        AppDbContext dbContext,
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var comments = new[]
        {
            "Great service and quick turnaround.",
            "Satisfied with the repair quality.",
            "Pricing was fair and transparent.",
            "Waiting area could be improved.",
            "Excellent communication from the staff."
        };

        var reviews = new List<Review>(250);
        for (var i = 1; i <= 250; i++)
        {
            var rating = i % 10 switch
            {
                <= 5 => 5,
                6 or 7 => 4,
                8 => 3,
                _ => 2
            };

            reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[(i - 1) % customers.Count].Id,
                Rating = rating,
                Comment = comments[i % comments.Length],
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Next(1, 365))
            });
        }

        await dbContext.Reviews.AddRangeAsync(reviews, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return reviews;
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string address,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName,
            Address = address,
            EmailConfirmed = true,
            PhoneNumber = $"04{Random.Next(10000000, 99999999)}",
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, DefaultPassword);
        EnsureSucceeded(createResult, $"Failed to create user {email}.");

        var roleResult = await userManager.AddToRoleAsync(user, role);
        EnsureSucceeded(roleResult, $"Failed to assign role '{role}' to user {email}.");
        return user;
    }

    private static string BuildPersonName(int seed)
    {
        var first = FirstNames[seed % FirstNames.Length];
        var last = LastNames[seed % LastNames.Length];
        return $"{first} {last}";
    }

    private static void EnsureSucceeded(IdentityResult result, string prefix)
    {
        if (result.Succeeded) return;
        var details = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{prefix} {details}");
    }

    private static void PrintSummary(SeedSummary summary)
    {
        Console.WriteLine("Development database reset and seed completed.");
        Console.WriteLine($"Users: {summary.Users} (Admin: 1, Staff: 25, Customers: 250)");
        Console.WriteLine($"Staff profiles: {summary.Staff}");
        Console.WriteLine($"Customer profiles: {summary.Customers}");
        Console.WriteLine($"Vendors: {summary.Vendors}");
        Console.WriteLine($"Parts: {summary.Parts}");
        Console.WriteLine($"Vehicles: {summary.Vehicles}");
        Console.WriteLine($"Appointments: {summary.Appointments}");
        Console.WriteLine($"Sales: {summary.Sales}");
        Console.WriteLine($"Sale items: {summary.SaleItems}");
        Console.WriteLine($"Credit payments: {summary.CreditPayments}");
        Console.WriteLine($"Purchase invoices: {summary.PurchaseInvoices}");
        Console.WriteLine($"Purchase invoice items: {summary.PurchaseInvoiceItems}");
        Console.WriteLine($"Part requests: {summary.PartRequests}");
        Console.WriteLine($"Reviews: {summary.Reviews}");
        Console.WriteLine("Default seeded password for all test users: Admin@12345");
    }
}
