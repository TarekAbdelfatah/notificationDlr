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
                    ErrorMessage = "Please select a CSV file" 
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
                        ErrorMessage = "No records found in the CSV file" 
                    });
                }
                
                var referenceNumbers = records.Select(r => r.ReferenceNumber).ToList();
                var statusResults = await _smsStatusService.GetBulkSmsStatusAsync(referenceNumbers);
                
                var viewModel = new SmsQueryViewModel
                {
                    Records = records,
                    StatusResults = statusResults,
                    HasResults = true,
                    SuccessMessage = $"Successfully processed {records.Count} records from {excelFile.FileName}"
                };
                
                viewModel.CalculateStatistics();
                
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
        
        [HttpPost]
        public IActionResult ExportResults(SmsQueryViewModel model)
        {
            if (model.Records == null || model.Records.Count == 0)
            {
                return BadRequest("No data to export");
            }
            
            var csvContent = GenerateCsvContent(model);
            var fileName = $"sms_status_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }
        
        private string GenerateCsvContent(SmsQueryViewModel model)
        {
            var csvLines = new List<string>
            {
                "PhoneNumber,ReferenceNumber,PersonName,Status,ErrorCode,ErrorMessage"
            };
            
            foreach (var record in model.Records)
            {
                var status = model.StatusResults.ContainsKey(record.ReferenceNumber) 
                    ? model.StatusResults[record.ReferenceNumber] 
                    : new SmsStatusResult 
                    { 
                        Status = "NOT_FOUND", 
                        ErrorMessage = "Reference number not found in service" 
                    };
                
                var phoneNumber = record.PhoneNumber ?? "";
                var referenceNumber = record.ReferenceNumber ?? "";
                var personName = record.PersonName ?? "";
                var statusValue = status.Status ?? "";
                var errorCode = status.ErrorCode ?? "";
                var errorMessage = status.ErrorMessage ?? "";
                
                // Escape CSV special characters
                phoneNumber = EscapeCsvField(phoneNumber);
                referenceNumber = EscapeCsvField(referenceNumber);
                personName = EscapeCsvField(personName);
                statusValue = EscapeCsvField(statusValue);
                errorCode = EscapeCsvField(errorCode);
                errorMessage = EscapeCsvField(errorMessage);
                
                csvLines.Add($"{phoneNumber},{referenceNumber},{personName},{statusValue},{errorCode},{errorMessage}");
            }
            
            return string.Join(Environment.NewLine, csvLines);
        }
        
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            // If field contains comma, quote, or newline, wrap in quotes and escape quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            
            return field;
        }
    }
}