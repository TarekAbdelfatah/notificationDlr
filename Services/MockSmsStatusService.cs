using notificationDlr.Models;

namespace notificationDlr.Services
{
    public class MockSmsStatusService : ISmsStatusService
    {
        private readonly Random _random = new();
        
        public async Task<SmsStatusResult> GetSmsStatusAsync(string referenceNumber)
        {
            await Task.Delay(_random.Next(100, 500));
            
            var statuses = new[] { "DELIVERED", "FAILED", "PENDING", "SENT", "READ" };
            var status = statuses[_random.Next(statuses.Length)];
            
            var result = new SmsStatusResult
            {
                ReferenceNumber = referenceNumber,
                Status = status
            };
            
            if (status == "FAILED")
            {
                var errors = new[] { 
                    "Invalid number", 
                    "Network error", 
                    "Message expired", 
                    "Carrier rejection",
                    "Unknown error"
                };
                result.ErrorCode = "ERR_" + _random.Next(100, 999).ToString();
                result.ErrorMessage = errors[_random.Next(errors.Length)];
            }
            
            return result;
        }
        
        public async Task<Dictionary<string, SmsStatusResult>> GetBulkSmsStatusAsync(List<string> referenceNumbers)
        {
            var results = new Dictionary<string, SmsStatusResult>();
            
            foreach (var refNumber in referenceNumbers)
            {
                if (string.IsNullOrWhiteSpace(refNumber))
                {
                    results[refNumber] = new SmsStatusResult
                    {
                        ReferenceNumber = refNumber,
                        Status = "INVALID",
                        ErrorCode = "ERR_EMPTY",
                        ErrorMessage = "Empty reference number"
                    };
                    continue;
                }
                
                var status = await GetSmsStatusAsync(refNumber);
                results[refNumber] = status;
            }
            
            return results;
        }
    }
}