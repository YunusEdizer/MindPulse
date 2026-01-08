namespace MindPulse.UI.DTOs 
{
    // Listedeki her bir satır
    public class LeaderboardItemDto
    {
        public int Rank { get; set; }              // Sıralama (1, 2, 3...)
        public string Username { get; set; } = string.Empty;
        public double TotalScore { get; set; }     // TOPLAM PUAN (Refleks + Hafıza)
        public double BestReflex { get; set; }     // En iyi refleks skoru
        public double BestMemory { get; set; }     // En iyi hafıza skoru
    }

    // API'den dönecek ana paket (Liste + Kullanıcının Yeri)
    public class LeaderboardResponseDto
    {
        public List<LeaderboardItemDto> TopList { get; set; } = new();
        public LeaderboardItemDto? UserStats { get; set; }
    }
}