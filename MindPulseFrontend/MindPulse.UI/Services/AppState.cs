namespace MindPulse.UI.Services
{
    public static class AppState
    {
        public static int UserId { get; set; } = 0; // 0 ise giriş yapılmamış demektir
        public static string Username { get; set; } = "";

        // Giriş yapılıp yapılmadığını kontrol eden basit bir özellik
        public static bool IsLoggedIn => UserId > 0;
        // 1. Değişiklik olduğunda tetiklenecek olay (Event)
        public static event Action? OnChange;

        // 2. Bu metodu çağırdığımızda "Zil Çalar" ve dinleyen herkes güncellenir.
        public static void NotifyStateChanged() => OnChange?.Invoke();
    }
}