using System.Text;
using System.Text.RegularExpressions;

namespace YenMay_web.Utilities
{
    public class ImageHelper
    {
        public static string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "/images/no-image.png"; // Ảnh mặc định

            // Nếu đã có http:// hoặc https:// thì giữ nguyên
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            // Nếu bắt đầu bằng / thì giữ nguyên
            if (imagePath.StartsWith("/"))
                return imagePath;

            // Nếu là đường dẫn tương đối, thêm / ở đầu
            return "/" + imagePath;
        }

        // Tạo đường dẫn lưu ảnh theo cấu trúc
        public static string GenerateImagePath(string categoryName, string productName,
                                               string originalFileName, int imageIndex = 0)
        {
            // Chuẩn hóa tên thư mục và tên file
            var formattedCategoryName = FormatName(categoryName);
            var formattedProductName = FormatName(productName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName).ToLower();
            var formattedFileName = FormatName(fileNameWithoutExt);

            // Tạo tên file có đánh số nếu cần
            var fileName = imageIndex == 0
                ? $"{MakeValidFileName(fileNameWithoutExt)}{extension}"
                : $"{MakeValidFileName(fileNameWithoutExt)}({imageIndex}){extension}";

            // Tạo đường dẫn
            return $"img-product/{formattedCategoryName}/{formattedProductName}/{fileName}";
        }
        public static string FormatName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "unknown";
            input = RemoveDiacritics(input);

            // Tách thành các từ (split by space, hyphen, underscore, etc.)
            var words = Regex.Split(input, @"[\s\-_]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            if (words.Length == 0)
                return "Default";
            var formattedWords = words.Select(word =>
            {
                if (word.Length == 0)
                    return string.Empty;

                // Giữ nguyên số nếu có ở đầu
                var numberPart = "";
                var letterPart = word;

                // Tách phần số ở đầu (nếu có)
                var match = Regex.Match(word, @"^(\d+)(.*)$");
                if (match.Success)
                {
                    numberPart = match.Groups[1].Value;
                    letterPart = match.Groups[2].Value;
                }

                // Format phần chữ
                if (letterPart.Length == 0)
                    return numberPart;

                var formattedLetterPart = char.ToUpper(letterPart[0]) +
                                         (letterPart.Length > 1 ? letterPart.Substring(1).ToLower() : "");

                return numberPart + formattedLetterPart;
            });
            var result = string.Join("", formattedWords);

            // Loại bỏ ký tự đặc biệt còn sót
            result = RemoveSpecialCharacters(result);

            // Đảm bảo không rỗng
            return string.IsNullOrWhiteSpace(result) ? "Default" : result;
        }
        // Loại bỏ dấu tiếng Việt
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        // Loại bỏ ký tự đặc biệt (chỉ giữ chữ, số)
        private static string RemoveSpecialCharacters(string input)
        {
            // Chỉ giữ chữ cái, số, và dấu ngoặc đơn cho (1), (2), ...
            return Regex.Replace(input, @"[^a-zA-Z0-9()]", "");
        }

        // Xóa file ảnh cũ
        public static bool DeleteImageFile(IWebHostEnvironment environment, string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);

                    // Xóa thư mục rỗng nếu có
                    var directory = Path.GetDirectoryName(fullPath);
                    if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory);

                        // Xóa thư mục category nếu rỗng
                        var categoryDir = Path.GetDirectoryName(directory);
                        if (Directory.Exists(categoryDir) && !Directory.EnumerateFileSystemEntries(categoryDir).Any())
                        {
                            Directory.Delete(categoryDir);
                        }
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Kiểm tra file đã tồn tại chưa
        public static bool ImageExists(IWebHostEnvironment environment, string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            var fullPath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));
            return File.Exists(fullPath);
        }

        // Lấy số lượng ảnh hiện có trong thư mục sản phẩm
        public static int GetExistingImageCount(IWebHostEnvironment environment,
                                                string categoryName, string productName)
        {
            var safeCategoryName = MakeValidFileName(categoryName);
            var safeProductName = MakeValidFileName(productName);
            var productDir = Path.Combine(environment.WebRootPath,
                                         "img-product", safeCategoryName, safeProductName);

            if (!Directory.Exists(productDir))
                return 0;

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return Directory.GetFiles(productDir, "*.*")
                .Count(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()));
        }

        // Chuẩn hóa tên file
        private static string MakeValidFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "product";

            // Loại bỏ ký tự không hợp lệ
            var invalidChars = Path.GetInvalidFileNameChars();
            var validName = new string(name
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray())
                .Replace(" ", "-")
                .ToLower();

            // Giới hạn độ dài
            if (validName.Length > 100)
                validName = validName.Substring(0, 100);

            return validName;
        }

        // Tạo đường dẫn thư mục cho sản phẩm
        public static string GetProductImageDirectory(IWebHostEnvironment environment,
                                                      string categoryName, string productName)
        {
            var formattedCategoryName = FormatName(categoryName);
            var formattedProductName = FormatName(productName);

            return Path.Combine(environment.WebRootPath,
                               "img-product", formattedCategoryName, formattedProductName);
        }
        public static string GenerateArticleImagePath(string articleTitle, string originalFileName)
        {
            // Format tên bài viết thành tên thư mục
            var formattedTitle = FormatName(articleTitle); // Tận dụng hàm FormatName có sẵn

            // Xử lý tên file
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName).ToLower();
            var safeFileName = MakeValidFileName(fileNameWithoutExt) + extension;

            // Cấu trúc: img-article/tieu-de-bai-viet/ten-file.jpg
            return $"img-article/{formattedTitle}/{safeFileName}";
        }

        public static void DeleteArticleDirectory(IWebHostEnvironment environment, string articleTitle)
        {
            var formattedTitle = FormatName(articleTitle);
            var dirPath = Path.Combine(environment.WebRootPath, "img-article", formattedTitle);
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true); // True để xóa cả file bên trong
            }
        }
    }
}
