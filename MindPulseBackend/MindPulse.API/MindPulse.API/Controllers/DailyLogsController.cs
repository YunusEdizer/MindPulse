using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindPulse.API.Data;
using MindPulse.API.Models;
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
        public async Task<ActionResult<DailyLog>> PostDailyLog(DailyLog log)
        {
            log.Date = DateTime.Now;
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
    }
}