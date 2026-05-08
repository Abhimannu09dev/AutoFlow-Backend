namespace AutoFlow_Backend.Application.Common;

public static class PasswordGenerator
{
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijkmnopqrstuvwxyz";
    private const string DigitChars = "23456789";
    private const string SpecialChars = "!@#$%^&*";

    public static string Generate(int length = 12)
    {
        if (length < 8) length = 8;

        var password = new char[length];
        var random = new Random();

        password[0] = UppercaseChars[random.Next(UppercaseChars.Length)];
        password[1] = LowercaseChars[random.Next(LowercaseChars.Length)];
        password[2] = DigitChars[random.Next(DigitChars.Length)];
        password[3] = SpecialChars[random.Next(SpecialChars.Length)];

        var allChars = UppercaseChars + LowercaseChars + DigitChars + SpecialChars;
        for (var i = 4; i < length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}