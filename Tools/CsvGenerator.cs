namespace notificationDlr.Tools
{
    public static class CsvGenerator
    {
        public static void GenerateSampleCsvFile(string filePath)
        {
            var names = new[]
            {
                "أحمد محمد", "محمد علي", "علي حسن", "حسن محمود", "محمود إبراهيم",
                "إبراهيم خالد", "خالد سعيد", "سعيد عمر", "عمر يوسف", "يوسف عبدالله",
                "عبدالله كريم", "كريم مصطفى", "مصطفى ناصر", "ناصر رامي", "رامي وليد",
                "وليد هشام", "هشام عماد", "عماد بلال", "بلال تامر", "تامر جمال",
                "جمال رضا", "رضا سامي", "سامي فارس", "فارس زياد", "زياد ماهر",
                "ماهر أسامة", "أسامة حاتم", "حاتم عادل", "عادل نادر", "نادر صالح",
                "صالح فؤاد", "فؤاد حازم", "حازم وسام", "وسام نبيل", "نبيل رفعت",
                "رفعت سليم", "سليم أنس", "أنس باسل", "باسل جاد", "جاد حمدي",
                "حمدي عاطف", "عاطف سيف", "سيف طارق", "طارق يحيى", "يحيى شريف",
                "شريف عمرو", "عمرو مؤمن", "مؤمن سعد", "سعد عيسى", "عيسى حبيب"
            };

            var random = new Random();
            var lines = new List<string>
            {
                "PhoneNumber,ReferenceNumber,PersonName"
            };

            for (int i = 0; i < 50; i++)
            {
                var phoneNumber = $"01{random.Next(0, 10)}{random.Next(0, 10)}{random.Next(1000000, 9999999)}";
                var referenceNumber = $"REF{random.Next(10000, 99999)}";
                
                lines.Add($"{phoneNumber},{referenceNumber},{names[i]}");
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}