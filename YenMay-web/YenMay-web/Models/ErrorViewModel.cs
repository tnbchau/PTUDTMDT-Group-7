namespace YenMay_web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}