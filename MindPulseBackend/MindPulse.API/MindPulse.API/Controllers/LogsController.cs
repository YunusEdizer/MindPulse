using Microsoft.AspNetCore.Mvc;
using MindPulse.API.Data;
using MindPulse.API.DTOs;
using MindPulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MindPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/logs (Günlük veri kaydet)
        [HttpPost]
        public async Task<IActionResult> AddLog(CreateDailyLogDto request)
        {
            var log = new DailyLog
            {
                UserId = request.UserId,
                SleepHours = request.SleepHours,
                MoodScore = request.MoodScore,
                Date = DateTime.Now
            };

            _context.DailyLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok("Günlük veri kaydedildi.");
        }

        // GET: api/logs/user/1 (Kullanıcının geçmiş kayıtlarını gör)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLogs(int userId)
        {
            var logs = await _context.DailyLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            return Ok(logs);
        }
        // DELETE: api/logs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound("Kayıt bulunamadı.");
            }

            _context.DailyLogs.Remove(log);
            await _context.SaveChangesAsync();

            return Ok("Kayıt silindi.");
        }
    }
}