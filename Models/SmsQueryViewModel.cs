namespace notificationDlr.Models
{
    public class SmsQueryViewModel
    {
        public List<SmsRecord> Records { get; set; } = new();
        public Dictionary<string, SmsStatusResult> StatusResults { get; set; } = new();
        public bool HasResults { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";
    }
}