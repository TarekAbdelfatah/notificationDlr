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
        public IActionResult ExportResults([FromForm] string recordsJson, [FromForm] string statusResultsJson)
        {
            try
            {
                Console.WriteLine($"Export request received. Records JSON length: {recordsJson?.Length}, Status Results JSON length: {statusResultsJson?.Length}");
                
                if (string.IsNullOrEmpty(recordsJson) || string.IsNullOrEmpty(statusResultsJson))
                {
                    Console.WriteLine("Missing JSON data");
                    return BadRequest("No data to export");
                }
                
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var records = System.Text.Json.JsonSerializer.Deserialize<List<SmsRecord>>(recordsJson, options);
                var statusResults = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, SmsStatusResult>>(statusResultsJson, options);
                
                if (records == null || records.Count == 0)
                {
                    Console.WriteLine("No records found after deserialization");
                    return BadRequest("No records to export");
                }
                
                if (statusResults == null)
                {
                    Console.WriteLine("No status results found after deserialization");
                    statusResults = new Dictionary<string, SmsStatusResult>();
                }
                
                Console.WriteLine($"Processing {records.Count} records and {statusResults.Count} status results");
                
                // Debug: Print first few records and status results
                if (records.Count > 0)
                {
                    Console.WriteLine($"First record - Phone: {records[0].PhoneNumber}, Ref: {records[0].ReferenceNumber}, Name: {records[0].PersonName}");
                }
                
                if (statusResults.Count > 0)
                {
                    var firstKey = statusResults.Keys.First();
                    Console.WriteLine($"First status result - Key: {firstKey}, Status: {statusResults[firstKey].Status}");
                }
                
                var csvContent = GenerateCsvContent(records, statusResults);
                var fileName = $"sms_status_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                Console.WriteLine($"Generated CSV with {csvContent.Length} characters");
                
                // Debug: Print first few lines of CSV
                var csvLines = csvContent.Split(Environment.NewLine);
                for (int i = 0; i < Math.Min(5, csvLines.Length); i++)
                {
                    Console.WriteLine($"CSV Line {i}: {csvLines[i]}");
                }
                
                return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting data: {ex.Message}\n{ex.StackTrace}");
                return BadRequest($"Error exporting data: {ex.Message}");
            }
        }
        
        private string GenerateCsvContent(List<SmsRecord> records, Dictionary<string, SmsStatusResult> statusResults)
        {
            var csvLines = new List<string>
            {
                "PhoneNumber,ReferenceNumber,PersonName,Status,ErrorCode,ErrorMessage"
            };
            
            foreach (var record in records)
            {
                var status = statusResults.ContainsKey(record.ReferenceNumber ?? "") 
                    ? statusResults[record.ReferenceNumber ?? ""] 
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