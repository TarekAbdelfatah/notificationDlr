using Microsoft.AspNetCore.Http;
using notificationDlr.Models;

namespace notificationDlr.Services
{
    public class ExcelParser : IExcelParser
    {
        public async Task<List<SmsRecord>> ParseExcelFileAsync(IFormFile file)
        {
            var records = new List<SmsRecord>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            
            // Check file extension
            if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return ParseCsvFile(stream);
            }
            else
            {
                throw new NotSupportedException("File format not supported. Please upload CSV file.");
            }
        }
        
        private List<SmsRecord> ParseCsvFile(Stream stream)
        {
            var records = new List<SmsRecord>();
            
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            
            // Skip header
            reader.ReadLine();
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    records.Add(new SmsRecord
                    {
                        PhoneNumber = parts[0].Trim(),
                        ReferenceNumber = parts[1].Trim(),
                        PersonName = parts[2].Trim()
                    });
                }
            }
            
            return records;
        }
    }
}