using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SporiumAPI.Data;
using SporiumAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SporiumAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // <-- TEST İÇİN BURAYI İPTAL ETTİK, ARTIK 401 VERMEZ!
    public class UyelerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UyelerController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uye>>> GetUyeler()
        {
            return await _context.Uyeler.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Uye>> GetUye(int id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();
            return uye;
        }

        [HttpPost("giris")]
        [AllowAnonymous]
        public IActionResult GirisYap([FromBody] LoginModel model)
        {
            var uye = _context.Uyeler.FirstOrDefault(u => u.Email == model.Email);
            if (uye == null || !BCrypt.Net.BCrypt.Verify(model.Sifre, uye.Sifre))
            {
                return Unauthorized(new { message = "E-posta veya şifre hatalı!" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "Varsayilan_Cok_Gizli_Anahtar_32_Karakter");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, uye.Id.ToString()),
                    new Claim(ClaimTypes.Email, uye.Email),
                    new Claim(ClaimTypes.Role, uye.Rol ?? "Uye")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            uye.Sifre = "";
            return Ok(new { token = tokenString, user = uye });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Uye>> PostUye(Uye uye)
        {
            if (await _context.Uyeler.AnyAsync(u => u.Email == uye.Email))
            {
                return BadRequest("Bu e-posta adresiyle zaten bir kayıt mevcut.");
            }

            uye.Sifre = BCrypt.Net.BCrypt.HashPassword(uye.Sifre);
            uye.KayitTarihi = DateTime.Now;
            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUye", new { id = uye.Id }, uye);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUye(int id, Uye uye)
        {
            if (id != uye.Id) return BadRequest();
            _context.Entry(uye).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUye(int id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();
            _context.Uyeler.Remove(uye);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Sifre { get; set; }
    }
}