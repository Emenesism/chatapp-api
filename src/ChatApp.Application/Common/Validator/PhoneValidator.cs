namespace ChatApp.Application.Common.Validator;

public static class PhoneValidator
{
    public static bool IsValidIranianPhoneNumber(string phone)
    {
        phone = NormalizePhone(phone);

        if (phone.StartsWith("+989") && phone.Length == 13)
            return true;

        if (phone.StartsWith("09") && phone.Length == 11)
            return true;
        return false;
    }

    public static string NormalizePhone(string phone)
    {
        phone = phone.Replace(" ", "").Replace("-", "");

        if (phone.StartsWith("+98"))
            return "09" + phone.Substring(2);

        return phone;
    }
}
