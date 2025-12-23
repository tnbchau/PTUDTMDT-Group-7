namespace YenMay_web.Models.Domain
{
    public class EditorImage
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
