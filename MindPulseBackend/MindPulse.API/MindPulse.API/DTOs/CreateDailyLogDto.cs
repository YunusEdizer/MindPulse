namespace MindPulse.API.DTOs
{
    public class CreateDailyLogDto
    {
        public int UserId { get; set; }
        public int SleepHours { get; set; }
        public int MoodScore { get; set; }
        public string Note { get; set; } = "";
    }
}