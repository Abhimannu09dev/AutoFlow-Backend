using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Services;
using AutoFlow_Backend.Application.Services.PredictionRules;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AutoFlow_Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerSelfService, CustomerSelfService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IStaffSelfService, StaffSelfService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();
        services.AddScoped<ICustomerReportService, CustomerReportService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminProfileService, AdminProfileService>();

        services.AddTransient<IFailurePredictionRule, BrakePadRule>();
        services.AddTransient<IFailurePredictionRule, TimingBeltRule>();
        services.AddTransient<IFailurePredictionRule, TransmissionFluidRule>();
        services.AddTransient<IFailurePredictionRule, CoolantRule>();
        services.AddTransient<IFailurePredictionRule, BatteryRule>();
        services.AddScoped<IFailurePredictionService, FailurePredictionService>();

        services.AddScoped<IVehicleService, VehicleService>();

        services.AddValidatorsFromAssemblyContaining<AutoFlow_Backend.Application.Validators.Customers.CustomerCreateDtoValidator>();

        return services;
    }
}
