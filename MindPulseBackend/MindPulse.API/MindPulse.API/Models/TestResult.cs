namespace MindPulse.API.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string TestType { get; set; } = string.Empty; // "Dikkat", "Hafıza" vs.
        public int Score { get; set; } // Puan
        public double ReactionTimeMs { get; set; } // Tepki süresi (milisaniye)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişki (Foreign Key)
        public int UserId { get; set; }
        public User User { get; set; }
    }
}