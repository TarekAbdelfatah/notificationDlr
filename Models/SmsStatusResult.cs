namespace notificationDlr.Models
{
    public class SmsStatusResult
    {
        public string ReferenceNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}