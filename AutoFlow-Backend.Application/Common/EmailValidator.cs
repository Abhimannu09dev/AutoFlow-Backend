namespace AutoFlow_Backend.Application.Common;

public static class EmailValidator
{
    public static bool IsValid(string email)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}