using Microsoft.AspNetCore.Http;
using notificationDlr.Models;

namespace notificationDlr.Services
{
    public interface IExcelParser
    {
        Task<List<SmsRecord>> ParseExcelFileAsync(IFormFile file);
    }
}