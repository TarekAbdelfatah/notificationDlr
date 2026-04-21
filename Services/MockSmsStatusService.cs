using notificationDlr.Models;

namespace notificationDlr.Services
{
    public class MockSmsStatusService : ISmsStatusService
    {
        private readonly Random _random = new();
        private readonly ILogger<MockSmsStatusService> _logger;
        
        public MockSmsStatusService(ILogger<MockSmsStatusService> logger)
        {
            _logger = logger;
        }
        
        public async Task<SmsStatusResult> GetSmsStatusAsync(string referenceNumber)
        {
            try
            {
                // Simulate network delay
                await Task.Delay(_random.Next(100, 500));
                
                // Simulate random failures (5% chance)
                if (_random.Next(100) < 5)
                {
                    throw new Exception("Simulated network timeout");
                }
                
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting status for reference {ReferenceNumber}", referenceNumber);
                
                return new SmsStatusResult
                {
                    ReferenceNumber = referenceNumber,
                    Status = "ERROR",
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"Service error: {ex.Message}"
                };
            }
        }
        
        public async Task<Dictionary<string, SmsStatusResult>> GetBulkSmsStatusAsync(List<string> referenceNumbers)
        {
            var results = new Dictionary<string, SmsStatusResult>();
            var tasks = new List<Task>();
            
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
                
                // Process each reference number in parallel but with error handling
                var task = ProcessReferenceNumberAsync(refNumber, results);
                tasks.Add(task);
            }
            
            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            
            return results;
        }
        
        private async Task ProcessReferenceNumberAsync(string referenceNumber, Dictionary<string, SmsStatusResult> results)
        {
            try
            {
                var status = await GetSmsStatusAsync(referenceNumber);
                lock (results)
                {
                    results[referenceNumber] = status;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process reference {ReferenceNumber}", referenceNumber);
                
                lock (results)
                {
                    results[referenceNumber] = new SmsStatusResult
                    {
                        ReferenceNumber = referenceNumber,
                        Status = "PROCESSING_ERROR",
                        ErrorCode = "PROCESS_ERR",
                        ErrorMessage = $"Processing failed: {ex.Message}"
                    };
                }
            }
        }
    }
}