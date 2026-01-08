using Microsoft.AspNetCore.Mvc;
using MindPulse.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MindPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAnalysis(int userId)
        {
            try
            {
                // 1. VERİLERİ ÇEKME
                var rawLogs = await _context.DailyLogs
                    .Where(l => l.UserId == userId && l.Date > DateTime.Now.AddDays(-30))
                    .ToListAsync();

                var tests = await _context.TestResults
                    .Where(t => t.UserId == userId && t.CreatedAt > DateTime.Now.AddDays(-30))
                    .ToListAsync();

                // Eğer hiç veri yoksa
                if (!rawLogs.Any() && !tests.Any())
                {
                    return Ok(new
                    {
                        Suggestion = "Henüz veri girişi yapmadınız. Lütfen günlük veri girin ve oyun oynayın.",
                        MoodSuggestion = "",
                        Insights = new List<string>(),
                        Message = "Veri bekleniyor..."
                    });
                }

                // 2. VERİ BİRLEŞTİRME
                var uniqueLogs = rawLogs
                    .OrderByDescending(l => l.Date)
                    .GroupBy(l => l.Date.Date)
                    .Select(g => g.First())
                    .ToList();

                var combinedData = from log in uniqueLogs
                                   join test in tests on log.Date.Date equals test.CreatedAt.Date into testGroup
                                   from test in testGroup.DefaultIfEmpty()
                                   select new
                                   {
                                       Sleep = log.SleepHours,
                                       Mood = log.MoodScore,
                                       Score = test != null ? test.Score : 0,
                                       TestTime = test != null ? test.CreatedAt : log.Date,
                                       Type = test != null ? test.TestType : "",
                                       Day = test != null ? test.CreatedAt.DayOfWeek : log.Date.DayOfWeek
                                   };

                var dataList = combinedData.ToList();

                // Veri yoksa veya eksikse basit dönüşler
                if (!dataList.Any())
                {
                    if (tests.Any() && !rawLogs.Any())
                    {
                        var reflexTestsOnly = tests.Where(x => x.TestType.Contains("Refleks")).Select(x => (double)x.Score).ToList();
                        var memoryTestsOnly = tests.Where(x => x.TestType.Contains("Hafıza")).Select(x => (double)x.Score).ToList();
                        double avgReflexOnly = reflexTestsOnly.Any() ? reflexTestsOnly.Average() : 0;
                        double avgMemoryOnly = memoryTestsOnly.Any() ? memoryTestsOnly.Average() : 0;

                        return Ok(new
                        {
                            CategoryLow = 0,
                            CategoryMedium = 0,
                            CategoryHigh = 0,
                            AvgReflexScore = avgReflexOnly,
                            AvgMemoryScore = avgMemoryOnly,
                            Suggestion = "Test sonuçlarınız var ancak günlük veri girişi yapmadınız.",
                            MoodSuggestion = "",
                            PredictedScore = 0,
                            Insights = new List<string>(),
                            TrendDates = new string[0],
                            TrendReflex = new double[0],
                            TrendMemory = new double[0]
                        });
                    }
                    else if (rawLogs.Any() && !tests.Any())
                    {
                        double avgSleep = rawLogs.Average(l => l.SleepHours);
                        return Ok(new
                        {
                            CategoryLow = 0,
                            CategoryMedium = 0,
                            CategoryHigh = 0,
                            AvgReflexScore = 0,
                            AvgMemoryScore = 0,
                            Suggestion = $"Günlük verileriniz var (Ortalama uyku: {avgSleep:F1} saat). Oyun oynayarak analiz alın.",
                            MoodSuggestion = "",
                            PredictedScore = 0,
                            Insights = new List<string>(),
                            TrendDates = new string[0],
                            TrendReflex = new double[0],
                            TrendMemory = new double[0]
                        });
                    }
                    return Ok(new { message = "Veri eşleşmesi yok." });
                }

                // 3. TEMEL İSTATİSTİKLER
                double CalcAvg(Func<dynamic, bool> predicate)
                {
                    var filtered = dataList.Where(predicate).Select(x => (double)x.Score).ToList();
                    return filtered.Any() ? filtered.Average() : 0;
                }

                double scoreLow = CalcAvg(x => x.Sleep < 6);
                double scoreMed = CalcAvg(x => x.Sleep >= 6 && x.Sleep < 7.5);
                double scoreHigh = CalcAvg(x => x.Sleep >= 7.5);
                double scoreStressed = CalcAvg(x => x.Mood <= 2);
                double scoreRelaxed = CalcAvg(x => x.Mood >= 4);

                var reflexTests = tests.Where(x => x.TestType.Contains("Refleks")).Select(x => (double)x.Score).ToList();
                double avgReflexScore = reflexTests.Any() ? reflexTests.Average() : 0;

                var memoryTests = tests.Where(x => x.TestType.Contains("Hafıza")).Select(x => (double)x.Score).ToList();
                double avgMemoryScore = memoryTests.Any() ? memoryTests.Average() : 0;

                string moodInsight = scoreRelaxed > scoreStressed ?
                    "Rahat olduğunda performansın daha yüksek." : "Baskı altında da iyi çalışıyorsun.";

                // 4. INSIGHTS LİSTESİ
                var insights = new List<string>();
                var morningData = dataList.Where(x => x.TestTime.Hour < 12).Select(x => (double)x.Score).ToList();
                var eveningData = dataList.Where(x => x.TestTime.Hour >= 18).Select(x => (double)x.Score).ToList();
                double morningScore = morningData.Any() ? morningData.Average() : 0;
                double eveningScore = eveningData.Any() ? eveningData.Average() : 0;
                if (morningScore > 0 && eveningScore > 0 && morningScore > eveningScore + 5) insights.Add("🌅 **Sabah İnsanısın:** Zihnin sabahları %20 daha açık.");
                else if (morningScore > 0 && eveningScore > 0 && eveningScore > morningScore + 5) insights.Add("🦉 **Gece Kuşusun:** Akşam saatlerinde odaklanman zirve yapıyor.");

                var weekendData = dataList.Where(x => x.Day == DayOfWeek.Saturday || x.Day == DayOfWeek.Sunday).Select(x => (double)x.Score).ToList();
                var weekdayData = dataList.Where(x => x.Day != DayOfWeek.Saturday && x.Day != DayOfWeek.Sunday).Select(x => (double)x.Score).ToList();
                double weekendScore = weekendData.Any() ? weekendData.Average() : 0;
                double weekdayScore = weekdayData.Any() ? weekdayData.Average() : 0;
                if (weekendScore > 0 && weekdayScore > 0 && weekendScore > weekdayScore + 5) insights.Add("📅 **Hafta Sonu Modu:** Tatil günlerinde performansın artıyor.");

                double reflexScoreAvg = reflexTests.Any() ? reflexTests.Average() : 0;
                double memoryScoreAvg = memoryTests.Any() ? memoryTests.Average() : 0;
                if (reflexScoreAvg > memoryScoreAvg + 10) insights.Add("⚡ **Hız Tutkunu:** Reflekslerin harika ama hafızan geliştirilebilir.");
                else if (memoryScoreAvg > reflexScoreAvg + 10) insights.Add("🧠 **Fil Hafızası:** Bilgileri tutmada iyisin, hızlanman gerek.");

                // Trend Grafiği Verileri
                var trendData = dataList.GroupBy(x => x.TestTime.Date).OrderBy(g => g.Key).TakeLast(10).ToList();
                var trendDates = trendData.Select(g => g.Key.ToString("dd/MM")).ToArray();
                var trendReflex = trendData.Select(g => { var s = g.Where(x => x.Type.Contains("Refleks")).Select(x => (double)x.Score).ToList(); return s.Any() ? s.Average() : 0; }).ToArray();
                var trendMemory = trendData.Select(g => { var s = g.Where(x => x.Type.Contains("Hafıza")).Select(x => (double)x.Score).ToList(); return s.Any() ? s.Average() : 0; }).ToArray();


                // ==========================================================================================
                // 5. --- GELİŞMİŞ YAPAY ZEKA MOTORU (HİBRİT YAPI: PERFORMANS + SAĞLIK) ---
                // ==========================================================================================
                string aiPrediction = "Veri bekleniyor...";
                double predictedScore = 0;

                var dataPoints = combinedData.Select(x => new { x.Sleep, x.Score }).ToList();
                string accuracyWarning = dataPoints.Count < 5 ? "⚠️ (Düşük Doğruluk) " : "";

                // Veri kontrolü (Test için limiti düşürebilirsin ama standart 3)
                if (dataPoints.Count >= 3)
                {
                    double avgSleepTotal = dataPoints.Average(p => p.Sleep);
                    double avgScoreTotal = dataPoints.Average(p => p.Score);
                    double maxScoreTotal = dataPoints.Max(p => p.Score);

                    // --- A. BİYOLOJİK VERİLERİ HAZIRLA ---
                    var user = await _context.Users.FindAsync(userId);
                    int userAge = user?.Age > 0 ? user.Age : 25; // Varsayılan 25

                    var sleepStd = GetSleepStandards(userAge);
                    double memoryStd = GetExpectedMemoryScore(userAge);

                    // --- B. PERFORMANS ANALİZİ (Regresyon & Durumlar) ---
                    string performanceMsg = "";

                    // Regresyon Matematiği
                    double n = dataPoints.Count;
                    double sumX = dataPoints.Sum(p => (double)p.Sleep);
                    double sumY = dataPoints.Sum(p => (double)p.Score);
                    double sumXY = dataPoints.Sum(p => (double)p.Sleep * (double)p.Score);
                    double sumX2 = dataPoints.Sum(p => (double)p.Sleep * (double)p.Sleep);
                    double denominator = (n * sumX2 - sumX * sumX);

                    // Varsayılan tahmin
                    predictedScore = avgScoreTotal;

                    // DURUM 1: ŞAMPİYON PERFORMANSI
                    if (avgScoreTotal > 92)
                    {
                        performanceMsg = "🏆 **Efsanevi Seviye:** Uyku süren ne olursa olsun performansın zirvede. Sen bu işi çözmüşsün, aynen devam et!";
                        predictedScore = 99;
                    }
                    // DURUM 2: STRES ANALİZİ (Sabit Uyku)
                    else if (Math.Abs(denominator) < 0.2)
                    {
                        // Hafıza Analizi
                        string memMsg = "";
                        if (avgMemoryScore > 0)
                        {
                            double diff = avgMemoryScore - memoryStd;
                            if (diff > 15) memMsg = $" (Hafızan {userAge} yaş ortalamasının çok üstünde! 🧠)";
                            else if (diff < -15) memMsg = $" (Hafızan {userAge} yaş ortalamasının biraz altında.)";
                        }

                        performanceMsg = $"🤖 **Robotik Düzen:** Uyku süren çok stabil, performansındaki değişimler stresten kaynaklanıyor olabilir.{memMsg}";
                        predictedScore = avgScoreTotal;
                    }
                    // DURUM 3: REGRESYON ANALİZİ
                    else
                    {
                        double m = (n * sumXY - sumX * sumY) / denominator;
                        double b = (sumY - m * sumX) / n;
                        predictedScore = Math.Clamp((m * 8) + b, 0, 100);

                        if (m > 4) performanceMsg = $"🚀 **Roket Etkisi:** Uykunu aldığın günler durdurulamazsın! (+{m:0.0} puan/saat).";
                        else if (m > 1.5) performanceMsg = "📈 **Pozitif Trend:** Verilerin net; daha fazla uyku = daha yüksek beyin gücü.";
                        else if (m < -3) performanceMsg = "⚡ **Adrenalin Tutkunu:** Yorgunken daha iyi odaklanıyorsun (Ters Manyel). Ama bu sürdürülebilir değil.";
                        else if (m < -1) performanceMsg = "📉 **Negatif Trend:** Çok uyumak sana yaramıyor, 'Uyku Sersemi' (Sleep Inertia) oluyor olabilirsin.";
                        else performanceMsg = "⚖️ **Dengeli:** Uyku süren ile performansın arasında radikal bir matematiksel bağ yok.";
                    }

                    // --- C. SAĞLIK BİLGİLENDİRMESİ (HER DURUMDA GÖSTERİLİR) ---
                    string healthMsg = "";
                    if (avgSleepTotal < sleepStd.Critical)
                    {
                        healthMsg = $"⛔ **Biyolojik Uyarı:** {userAge} yaş grubu için kritik sınır {sleepStd.Critical} saattir. Sen ortalama **{avgSleepTotal:0.0} sa** uyuyorsun.";
                    }
                    else if (avgSleepTotal >= sleepStd.Min && avgSleepTotal <= sleepStd.Max)
                    {
                        healthMsg = $"✅ **İdeal Aralık:** {userAge} yaş grubu için önerilen uyku **{sleepStd.Min}-{sleepStd.Max} sa** aralığındadır. Senin düzenin harika ({avgSleepTotal:0.0} sa)!";
                    }
                    else if (avgSleepTotal > sleepStd.Max)
                    {
                        healthMsg = $"ℹ️ **Bilgi:** {userAge} yaş grubu için üst sınır {sleepStd.Max} saattir. Sen **{avgSleepTotal:0.0} sa** ile biraz fazla uyuyor olabilirsin.";
                    }
                    else
                    {
                        // Min ile Critical arasında kalan gri bölge
                        healthMsg = $"ℹ️ **Bilgi:** {userAge} yaş grubu için önerilen aralık **{sleepStd.Min}-{sleepStd.Max} saattir** (Sen: {avgSleepTotal:0.0} sa).";
                    }

                    // --- SONUÇLARI BİRLEŞTİR ---
                    // Performans mesajı + Yeni satır + Sağlık Mesajı
                    aiPrediction = accuracyWarning + performanceMsg + "\n\n" + healthMsg;
                }
                else
                {
                    aiPrediction = "Yapay zeka tahmini için en az 3 günlük veri gerekli.";
                }

                // ==========================================================================================

                return Ok(new
                {
                    CategoryLow = scoreLow,
                    CategoryMedium = scoreMed,
                    CategoryHigh = scoreHigh,
                    ScoreStressed = scoreStressed,
                    ScoreRelaxed = scoreRelaxed,
                    AvgReflexScore = avgReflexScore,
                    AvgMemoryScore = avgMemoryScore,
                    Suggestion = aiPrediction,
                    MoodSuggestion = moodInsight,
                    PredictedScore = predictedScore,
                    Insights = insights,
                    TrendDates = trendDates,
                    TrendReflex = trendReflex,
                    TrendMemory = trendMemory
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hata: " + ex.Message });
            }
        }

        // --- BİLİMSEL STANDARTLARI HESAPLAYAN YARDIMCI METOTLAR ---

        // 1. Uyku Standartları (National Sleep Foundation)
        private (double Min, double Max, double Critical) GetSleepStandards(int age)
        {
            if (age < 18) return (8.0, 10.0, 7.0);       // Genç
            if (age <= 25) return (7.0, 9.0, 6.0);       // Genç Yetişkin
            if (age <= 64) return (7.0, 9.0, 6.0);       // Yetişkin
            return (7.0, 8.0, 5.5);                      // 65+
        }

        // 2. Refleks Standartları (Yaşa Bağlı Nörolojik Gecikme)
        private double GetExpectedReactionTime(int age)
        {
            double baseReaction = 250;
            if (age > 24) baseReaction += (age - 24) * 3;
            return baseReaction;
        }

        // 3. Hafıza Standartları (Görsel Çalışma Belleği Eğrisi)
        private double GetExpectedMemoryScore(int age)
        {
            double baseScore = 80;
            if (age > 25) baseScore -= (age - 25) * 0.8;
            return Math.Max(baseScore, 45);
        }
    }
}