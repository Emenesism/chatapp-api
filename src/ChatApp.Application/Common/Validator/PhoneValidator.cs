namespace ChatApp.Application.Common.Validator;

public static class PhoneValidator
{
    public static bool IsValidIranianPhoneNumber(string phone)
    {
        var normalized = NormalizePhone(phone);
        return normalized.Length == 11 &&
               normalized.StartsWith("09", StringComparison.Ordinal) &&
               normalized.All(char.IsDigit);
    }

    public static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        phone = ConvertToEnglishDigits(phone);
        phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (phone.StartsWith("+98", StringComparison.Ordinal))
            return "0" + phone.Substring(3);

        if (phone.StartsWith("0098", StringComparison.Ordinal))
            return "0" + phone.Substring(4);

        if (phone.StartsWith("98", StringComparison.Ordinal) && phone.Length == 12)
            return "0" + phone.Substring(2);

        if (phone.StartsWith("9", StringComparison.Ordinal) && phone.Length == 10)
            return "0" + phone;

        if (phone.StartsWith("09", StringComparison.Ordinal) && phone.Length > 11)
            return phone.Substring(0, 11);

        return phone;
    }

    private static string ConvertToEnglishDigits(string input)
    {
        return input
            .Replace('۰', '0').Replace('٠', '0')
            .Replace('۱', '1').Replace('١', '1')
            .Replace('۲', '2').Replace('٢', '2')
            .Replace('۳', '3').Replace('٣', '3')
            .Replace('۴', '4').Replace('٤', '4')
            .Replace('۵', '5').Replace('٥', '5')
            .Replace('۶', '6').Replace('٦', '6')
            .Replace('۷', '7').Replace('٧', '7')
            .Replace('۸', '8').Replace('٨', '8')
            .Replace('۹', '9').Replace('٩', '9');
    }
}
