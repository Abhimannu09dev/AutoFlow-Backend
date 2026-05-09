using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoFlow_Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerSelfService, CustomerSelfService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();
        services.AddScoped<ICustomerReportService, CustomerReportService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFailurePredictionService, FailurePredictionService>();
        services.AddScoped<IVehicleService, VehicleService>();

        return services;
    }
}