using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace YenMay_web.Utilities
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLowerInvariant().Trim();

            // Remove Vietnamese accents
            input = RemoveDiacritics(input);

            // Replace spaces with hyphens
            input = Regex.Replace(input, @"\s+", "-");

            // Remove invalid characters
            input = Regex.Replace(input, @"[^a-z0-9\-]", "");

            // Remove multiple hyphens
            input = Regex.Replace(input, @"-+", "-");

            return input.Trim('-');
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                // FIX: Đổi từ GetUnicodeCategory.NonSpacingMark thành UnicodeCategory.NonSpacingMark
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}