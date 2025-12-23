using Microsoft.EntityFrameworkCore;
using MindPulse.API.Models;

namespace MindPulse.API.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor: Ayarları (bağlantı adresi vb.) Program.cs'ten alır
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Modellerimizi Tablo olarak tanımlıyoruz
        public DbSet<User> Users { get; set; }
        public DbSet<DailyLog> DailyLogs { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
    }
}