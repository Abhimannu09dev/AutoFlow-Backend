using System.Security.Cryptography;

namespace AutoFlow_Backend.Application.Common;

public static class PasswordGenerator
{
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghjkmnopqrstuvwxyz";
    private const string DigitChars = "23456789";
    private const string SpecialChars = "!@#$%^&*";

    public static string Generate(int length = 12)
    {
        if (length < 8) length = 8;

        var password = new char[length];

        password[0] = UppercaseChars[RandomNumberGenerator.GetInt32(UppercaseChars.Length)];
        password[1] = LowercaseChars[RandomNumberGenerator.GetInt32(LowercaseChars.Length)];
        password[2] = DigitChars[RandomNumberGenerator.GetInt32(DigitChars.Length)];
        password[3] = SpecialChars[RandomNumberGenerator.GetInt32(SpecialChars.Length)];

        var allChars = UppercaseChars + LowercaseChars + DigitChars + SpecialChars;
        for (var i = 4; i < length; i++)
        {
            password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}