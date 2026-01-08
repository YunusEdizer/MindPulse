namespace MindPulse.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
       
        public string PasswordHash { get; set; } = string.Empty; // Şifreyi açık saklamayacağız

        public int Age { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Bir kullanıcının birden fazla logu ve test sonucu olabilir
        public List<DailyLog> DailyLogs { get; set; }
        public List<TestResult> TestResults { get; set; }
    }
}