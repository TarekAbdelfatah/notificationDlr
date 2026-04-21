namespace notificationDlr.Models
{
    public class SmsQueryViewModel
    {
        public List<SmsRecord> Records { get; set; } = new();
        public Dictionary<string, SmsStatusResult> StatusResults { get; set; } = new();
        public bool HasResults { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";
        
        // Statistics
        public int TotalRecords { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public int ErrorCount { get; set; }
        public int OtherCount { get; set; }
        
        public void CalculateStatistics()
        {
            TotalRecords = Records.Count;
            DeliveredCount = 0;
            FailedCount = 0;
            PendingCount = 0;
            ErrorCount = 0;
            OtherCount = 0;
            
            foreach (var record in Records)
            {
                if (StatusResults.TryGetValue(record.ReferenceNumber, out var status))
                {
                    switch (status.Status)
                    {
                        case "DELIVERED":
                            DeliveredCount++;
                            break;
                        case "FAILED":
                        case "INVALID":
                            FailedCount++;
                            break;
                        case "PENDING":
                            PendingCount++;
                            break;
                        case "ERROR":
                        case "PROCESSING_ERROR":
                        case "SERVICE_ERROR":
                            ErrorCount++;
                            break;
                        default:
                            OtherCount++;
                            break;
                    }
                }
                else
                {
                    ErrorCount++;
                }
            }
        }
    }
}