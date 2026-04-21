using notificationDlr.Tools;

namespace notificationDlr
{
    public static class ProgramExtensions
    {
        public static void GenerateSampleFile(this WebApplication app)
        {
            var samplePath = Path.Combine(app.Environment.WebRootPath, "samples", "sample_sms_data.csv");
            var directory = Path.GetDirectoryName(samplePath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            
            if (!File.Exists(samplePath))
            {
                CsvGenerator.GenerateSampleCsvFile(samplePath);
                Console.WriteLine($"Sample CSV file generated: {samplePath}");
            }
        }
    }
}