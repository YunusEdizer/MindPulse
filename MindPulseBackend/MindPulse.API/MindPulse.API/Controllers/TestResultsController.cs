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
    }
}