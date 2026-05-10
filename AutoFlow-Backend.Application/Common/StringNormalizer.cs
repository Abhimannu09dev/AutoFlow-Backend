namespace AutoFlow_Backend.Application.Common;

public static class StringNormalizer
{
    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}