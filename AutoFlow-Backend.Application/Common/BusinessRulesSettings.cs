namespace AutoFlow_Backend.Application.Common;

public class BusinessRulesSettings
{
    public decimal LoyaltyDiscountThreshold { get; set; } = 5000m;
    public decimal LoyaltyDiscountRate { get; set; } = 0.10m;
    public int RegularCustomerMinimumPurchases { get; set; } = 3;
}