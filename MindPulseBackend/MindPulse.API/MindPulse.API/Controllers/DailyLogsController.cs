using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindPulse.API.Data;
using MindPulse.API.Models;
using MindPulse.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MindPulse.API.Controllers
{
    [Route("api/[controller]")] // Adres: api/dailylogs
    [ApiController]
    public class DailyLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DailyLogsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Kaydetme İşlemi
        [HttpPost]
        public async Task<ActionResult<DailyLog>> PostDailyLog(CreateDailyLogDto dto)
        {
            // Validation
            if (dto.UserId <= 0)
                return BadRequest("Geçersiz kullanıcı ID.");
            
            if (dto.SleepHours < 0 || dto.SleepHours > 24)
                return BadRequest("Uyku süresi 0-24 saat arasında olmalıdır.");
            
            if (dto.MoodScore < 1 || dto.MoodScore > 10)
                return BadRequest("Ruh hali skoru 1-10 arasında olmalıdır.");

            var log = new DailyLog
            {
                UserId = dto.UserId,
                SleepHours = dto.SleepHours,
                MoodScore = dto.MoodScore,
                Note = dto.Note ?? "",
                Date = DateTime.Now
            };
            _context.DailyLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(log);
        }

        // GET: Geçmişi Çekme İşlemi (Analiz için)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<DailyLog>>> GetUserLogs(int userId)
        {
            return await _context.DailyLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }

        // GET: Bugünkü veri kontrolü
        [HttpGet("user/{userId}/today")]
        public async Task<ActionResult<object>> GetTodayStatus(int userId)
        {
            var today = DateTime.Now.Date;
            var hasLogToday = await _context.DailyLogs
                .AnyAsync(x => x.UserId == userId && x.Date.Date == today);
            
            return Ok(new { HasLogToday = hasLogToday });
        }
        // DELETE: api/dailylogs/{id} - Kayıt Silme (Yeni eklendi)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null) return NotFound("Kayıt bulunamadı.");

            _context.DailyLogs.Remove(log);
            await _context.SaveChangesAsync();
            return Ok("Kayıt silindi.");
        }
    }
}