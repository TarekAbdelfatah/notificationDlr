using notificationDlr.Models;

namespace notificationDlr.Services
{
    public interface ISmsStatusService
    {
        Task<SmsStatusResult> GetSmsStatusAsync(string referenceNumber);
        Task<Dictionary<string, SmsStatusResult>> GetBulkSmsStatusAsync(List<string> referenceNumbers);
    }
}