using Microsoft.AspNetCore.Mvc;
using MindPulse.API.Data;
using MindPulse.API.DTOs;
using MindPulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MindPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // Kullanıcı var mı kontrol et (Basit kontrol)
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Bu kullanıcı adı zaten alınmış.");
            }

            // Yeni kullanıcı oluştur
            var newUser = new User
            {
                Username = request.Username,
                // Gerçek projede şifreyi hash'lemeliyiz, şimdilik eğitim amaçlı düz kaydediyoruz
                PasswordHash = request.Password
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu!", userId = newUser.Id });
        }

        // Giriş yapma (Login) simülasyonu - ID'yi öğrenmek için
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserRegisterDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            return Ok(new { userId = user.Id, username = user.Username });
        }
    }
}