namespace MindPulse.UI.Services
{
    public class AppState
    {
        public int UserId { get; set; } = 0; // 0 ise giriş yapılmamış demektir
        public string Username { get; set; } = "";

        // Giriş yapılıp yapılmadığını kontrol eden basit bir özellik
        public bool IsLoggedIn => UserId > 0;
        // 1. Değişiklik olduğunda tetiklenecek olay (Event)
        public event Action? OnChange;

        // 2. Bu metodu çağırdığımızda "Zil Çalar" ve dinleyen herkes güncellenir.
        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}