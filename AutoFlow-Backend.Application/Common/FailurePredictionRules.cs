namespace AutoFlow_Backend.Application.Common;

public static class FailurePredictionRules
{
    public const int BrakePadMileageThreshold = 50_000;
    public const int TimingBeltMileageThreshold = 80_000;
    public const int TransmissionFluidMileageThreshold = 100_000;
    public const int CoolantAgeThresholdYears = 5;
    public const int BatteryAgeThresholdYears = 10;
}