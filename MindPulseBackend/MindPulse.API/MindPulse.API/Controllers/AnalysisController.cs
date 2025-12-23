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

                if (rawLogs == null || !rawLogs.Any() || tests == null || !tests.Any())
                {
                    return Ok(new { Suggestion = "Veri bekleniyor...", MoodSuggestion = "", Insights = new List<string>() });
                }

                // 2. VERİ BİRLEŞTİRME
                var uniqueLogs = rawLogs.GroupBy(l => l.Date.Date).Select(g => g.Last()).ToList();

                var combinedData = from log in uniqueLogs
                                   join test in tests on log.Date.Date equals test.CreatedAt.Date
                                   select new
                                   {
                                       Sleep = log.SleepHours,
                                       Mood = log.MoodScore,
                                       Score = test.Score,
                                       TestTime = test.CreatedAt,
                                       Type = test.TestType,
                                       Day = test.CreatedAt.DayOfWeek
                                   };

                var dataList = combinedData.ToList();

                if (!dataList.Any()) return Ok(new { message = "Veri eşleşmesi yok." });

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
                string moodInsight = scoreRelaxed > scoreStressed ?
                    "Rahat olduğunda performansın daha yüksek." : "Baskı altında da iyi çalışıyorsun.";

                // 4. INSIGHTS LİSTESİ
                var insights = new List<string>();

                // Kronotip
                double morningScore = dataList.Where(x => x.TestTime.Hour < 12).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                double eveningScore = dataList.Where(x => x.TestTime.Hour >= 18).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                if (morningScore > eveningScore + 5) insights.Add("🌅 **Sabah İnsanısın:** Zihnin sabahları %20 daha açık.");
                else if (eveningScore > morningScore + 5) insights.Add("🦉 **Gece Kuşusun:** Akşam saatlerinde odaklanman zirve yapıyor.");

                // Hafta Sonu
                double weekendScore = dataList.Where(x => x.Day == DayOfWeek.Saturday || x.Day == DayOfWeek.Sunday).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                double weekdayScore = dataList.Where(x => x.Day != DayOfWeek.Saturday && x.Day != DayOfWeek.Sunday).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                if (weekendScore > weekdayScore + 5) insights.Add("📅 **Hafta Sonu Modu:** Tatil günlerinde performansın artıyor.");

                // Refleks vs Hafıza
                double reflexScore = dataList.Where(x => x.Type.Contains("Refleks")).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                double memoryScore = dataList.Where(x => x.Type.Contains("Hafıza")).Select(x => (double)x.Score).DefaultIfEmpty(0).Average();
                if (reflexScore > memoryScore + 10) insights.Add("⚡ **Hız Tutkunu:** Reflekslerin harika ama hafızan geliştirilebilir.");
                else if (memoryScore > reflexScore + 10) insights.Add("🧠 **Fil Hafızası:** Bilgileri tutmada iyisin, hızlanman gerek.");

                // Trend Grafiği Verileri
                var trendData = dataList.GroupBy(x => x.TestTime.Date).OrderBy(g => g.Key).TakeLast(10).ToList();
                var trendDates = trendData.Select(g => g.Key.ToString("dd/MM")).ToArray();
                var trendReflex = trendData.Select(g => g.Where(x => x.Type.Contains("Refleks")).Select(x => (double)x.Score).DefaultIfEmpty(0).Average()).ToArray();
                var trendMemory = trendData.Select(g => g.Where(x => x.Type.Contains("Hafıza")).Select(x => (double)x.Score).DefaultIfEmpty(0).Average()).ToArray();


                // 5. --- GELİŞMİŞ YAPAY ZEKA MOTORU (STEP-BY-STEP) --- 
                string aiPrediction = "Veri bekleniyor...";
                double predictedScore = 0;

                var dataPoints = combinedData.Select(x => new { x.Sleep, x.Score }).ToList();

                if (dataPoints.Count >= 3)
                {
                    double avgSleepTotal = dataPoints.Average(p => p.Sleep);
                    double avgScoreTotal = dataPoints.Average(p => p.Score);
                    double maxScoreTotal = dataPoints.Max(p => p.Score);

                    // Regresyon Matematiği
                    double n = dataPoints.Count;
                    double sumX = dataPoints.Sum(p => (double)p.Sleep);
                    double sumY = dataPoints.Sum(p => (double)p.Score);
                    double sumXY = dataPoints.Sum(p => (double)p.Sleep * (double)p.Score);
                    double sumX2 = dataPoints.Sum(p => (double)p.Sleep * (double)p.Sleep);
                    double denominator = (n * sumX2 - sumX * sumX);

                    // Varsayılan tahmin
                    predictedScore = avgScoreTotal;

                    // --- KARAR AĞACI ---

                    // DURUM 1: KRONİK UYKUSUZLUK (Öncelikli Uyarı)
                    if (avgSleepTotal < 5.5)
                    {
                        aiPrediction = $"⛔ **Tehlikeli Bölge:** Ortalama uykun ({avgSleepTotal:0.0} sa) çok düşük. Biyolojik limitlerini zorluyorsun, önce uykunu düzeltmelisin.";
                    }
                    // DURUM 2: ŞAMPİYON PERFORMANSI (Uyku ne olursa olsun yüksek puan)
                    else if (avgScoreTotal > 92)
                    {
                        aiPrediction = "🏆 **Efsanevi Seviye:** Uyku süren ne olursa olsun performansın zirvede. Sen bu işi çözmüşsün, aynen devam et!";
                        predictedScore = 99;
                    }
                    // DURUM 3: STRES ANALİZİ (Uyku sabit ama Puan Dalgalı)
                    else if (Math.Abs(denominator) < 0.2)
                    {
                        double minS = dataPoints.Min(p => p.Score);
                        double maxS = dataPoints.Max(p => p.Score);

                        if ((maxS - minS) > 25)
                            aiPrediction = "⚠️ **Gizli Düşman:** Uyku düzenin harika ama performansın çok dalgalı. Veriler sorunun uyku değil, **Stres/Kaygı** olduğunu gösteriyor.";
                        else
                            aiPrediction = "🤖 **Robotik Düzen:** Hem uykun hem performansın bir İsviçre saati gibi stabil. Sürpriz yok.";

                        predictedScore = avgScoreTotal;
                    }
                    // DURUM 4: NORMAL EĞİM ANALİZİ (Regresyon)
                    else
                    {
                        double m = (n * sumXY - sumX * sumY) / denominator;
                        double b = (sumY - m * sumX) / n;
                        predictedScore = Math.Clamp((m * 8) + b, 0, 100);

                        if (m > 4) aiPrediction = $"🚀 **Roket Etkisi:** Uykunu aldığın günler durdurulamazsın! (+{m:0.0} puan/saat).";
                        else if (m > 1.5) aiPrediction = "📈 **Pozitif Trend:** Verilerin net; daha fazla uyku = daha yüksek beyin gücü.";
                        else if (m < -3) aiPrediction = "⚡ **Adrenalin Tutkunu:** Yorgunken daha iyi odaklanıyorsun (Ters Manyel). Ama bu sürdürülebilir değil.";
                        else if (m < -1) aiPrediction = "📉 **Negatif Trend:** Çok uyumak sana yaramıyor, 'Uyku Sersemi' (Sleep Inertia) oluyor olabilirsin.";
                        else aiPrediction = "⚖️ **Dengeli:** Uyku süren ile performansın arasında radikal bir matematiksel bağ yok.";
                    }
                }
                else
                {
                    aiPrediction = "Yapay zeka tahmini için en az 3 günlük veri gerekli.";
                }

                return Ok(new
                {
                    CategoryLow = scoreLow,
                    CategoryMedium = scoreMed,
                    CategoryHigh = scoreHigh,
                    ScoreStressed = scoreStressed,
                    ScoreRelaxed = scoreRelaxed,
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
    }
}