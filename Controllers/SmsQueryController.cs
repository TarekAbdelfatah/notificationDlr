using Microsoft.AspNetCore.Mvc;
using notificationDlr.Models;
using notificationDlr.Services;

namespace notificationDlr.Controllers
{
    public class SmsQueryController : Controller
    {
        private readonly IExcelParser _excelParser;
        private readonly ISmsStatusService _smsStatusService;
        
        public SmsQueryController(IExcelParser excelParser, ISmsStatusService smsStatusService)
        {
            _excelParser = excelParser;
            _smsStatusService = smsStatusService;
        }
        
        public IActionResult Index()
        {
            return View(new SmsQueryViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadAndQuery(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return View("Index", new SmsQueryViewModel 
                { 
                    ErrorMessage = "Please select an Excel file" 
                });
            }
            
            if (!excelFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return View("Index", new SmsQueryViewModel 
                { 
                    ErrorMessage = "Please upload a CSV file (.csv)" 
                });
            }
            
            try
            {
                var records = await _excelParser.ParseExcelFileAsync(excelFile);
                
                if (records.Count == 0)
                {
                    return View("Index", new SmsQueryViewModel 
                    { 
                        ErrorMessage = "No records found in the Excel file" 
                    });
                }
                
                var referenceNumbers = records.Select(r => r.ReferenceNumber).ToList();
                var statusResults = await _smsStatusService.GetBulkSmsStatusAsync(referenceNumbers);
                
                var viewModel = new SmsQueryViewModel
                {
                    Records = records,
                    StatusResults = statusResults,
                    HasResults = true,
                    SuccessMessage = $"Processed {records.Count} records from {excelFile.FileName}"
                };
                
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                return View("Index", new SmsQueryViewModel 
                { 
                    ErrorMessage = $"Error processing file: {ex.Message}" 
                });
            }
        }
    }
}