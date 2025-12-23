using System;

namespace MindPulse.API.Models
{
    public class DailyLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int SleepHours { get; set; }

        // DEĞİŞİKLİK 1: Yorumu güncelledik (Artık 1-10 arası)
        public int MoodScore { get; set; } // 1 (Kötü) - 10 (Harika)

        // DEĞİŞİKLİK 2: Yeni eklenen alan (Kullanıcı notu için)
        public string Note { get; set; } = "";

        // İlişki (Foreign Key) - AYNEN KALSIN
        public int UserId { get; set; }
        public User User { get; set; }
    }
}