namespace YenMay_web.Utilities
{
    public static class PhoneHelper
    {
        public static string Normalize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            return phone
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("+84", "0")
                .Trim();
        }
    }
}