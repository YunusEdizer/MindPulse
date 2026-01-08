using Microsoft.AspNetCore.Mvc;
using MindPulse.API.Data;
using MindPulse.API.DTOs;
using MindPulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MindPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestResultsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/testresults (Skor Kaydet)
        [HttpPost]
        public async Task<IActionResult> AddTestResult(CreateTestResultDto request)
        {
            // Validation
            if (request.UserId <= 0)
                return BadRequest("Geçersiz kullanıcı ID.");
            
            if (string.IsNullOrWhiteSpace(request.TestType))
                return BadRequest("Test tipi boş olamaz.");
            
            if (request.Score < 0 || request.Score > 100)
                return BadRequest("Skor 0-100 arasında olmalıdır.");
            
            if (request.ReactionTimeMs < 0)
                return BadRequest("Tepki süresi negatif olamaz.");

            var result = new TestResult
            {
                UserId = request.UserId,
                TestType = request.TestType,
                Score = request.Score,
                ReactionTimeMs = request.ReactionTimeMs,
                CreatedAt = DateTime.Now
            };

            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test sonucu başarıyla kaydedildi." });
        }

        // GET: api/testresults/user/1 (Kullanıcının geçmiş skorları)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserResults(int userId)
        {
            var results = await _context.TestResults
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // GET: Bugünkü test kontrolü
        [HttpGet("user/{userId}/today")]
        public async Task<ActionResult<object>> GetTodayTestStatus(int userId)
        {
            var today = DateTime.Now.Date;
            var hasTestToday = await _context.TestResults
                .AnyAsync(r => r.UserId == userId && r.CreatedAt.Date == today);
            
            var hasReflexTest = await _context.TestResults
                .AnyAsync(r => r.UserId == userId && r.CreatedAt.Date == today && r.TestType.Contains("Refleks"));
            
            var hasMemoryTest = await _context.TestResults
                .AnyAsync(r => r.UserId == userId && r.CreatedAt.Date == today && r.TestType.Contains("Hafıza"));
            
            return Ok(new 
            { 
                HasTestToday = hasTestToday,
                HasReflexTest = hasReflexTest,
                HasMemoryTest = hasMemoryTest
            });
        }
        [HttpGet("leaderboard/{currentUserId}")]
        public async Task<ActionResult<LeaderboardResponseDto>> GetLeaderboard(int currentUserId)
        {
            // 1. Tüm test sonuçlarını kullanıcı bilgisiyle çek
            var rawData = await _context.TestResults
                .Include(t => t.User)
                .ToListAsync();

            // 2. Kullanıcı bazlı grupla ve EN İYİ skorları bul
            var calculatedList = rawData
                .GroupBy(t => t.UserId)
                .Select(g => {
                    // Kullanıcının en iyi refleks skoru (Yoksa 0)
                    double bestReflex = g.Where(x => x.TestType.Contains("Refleks"))
                                         .Select(x => x.Score)
                                         .DefaultIfEmpty(0)
                                         .Max();

                    // Kullanıcının en iyi hafıza skoru (Yoksa 0)
                    double bestMemory = g.Where(x => x.TestType.Contains("Hafıza"))
                                         .Select(x => x.Score)
                                         .DefaultIfEmpty(0)
                                         .Max();

                    return new LeaderboardItemDto
                    {
                        Username = g.First().User.Username, // Grup içinden ismini al
                        BestReflex = bestReflex,
                        BestMemory = bestMemory,
                        TotalScore = bestReflex + bestMemory // --- KRİTİK NOKTA: PUAN TOPLAMI ---
                    };
                })
                .OrderByDescending(x => x.TotalScore) // En yüksek puana göre sırala
                .ToList();

            // 3. Sıralama Numarası (Rank) Ata
            for (int i = 0; i < calculatedList.Count; i++)
            {
                calculatedList[i].Rank = i + 1;
            }

            // 4. Paketi Hazırla
            var response = new LeaderboardResponseDto
            {
                // İlk 10 kişiyi al
                TopList = calculatedList.Take(10).ToList(),

                // İstekte bulunan kullanıcının kendi istatistiğini bul
                // (Username üzerinden eşleştirme yapıyoruz, daha güvenli yol userId taşımaktır ama bu hızlı çözüm)
                UserStats = null
            };

            // Mevcut kullanıcıyı bulmak için veritabanından adını çekelim
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser != null)
            {
                response.UserStats = calculatedList.FirstOrDefault(x => x.Username == currentUser.Username);
            }

            return Ok(response);
        }
    }
}