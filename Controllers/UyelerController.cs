using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporiumAPI.Data;
using SporiumAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SporiumAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // TEST İÇİN AUTHORIZE KALDIRILDI
    public class UyelerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UyelerController(AppDbContext context)
        {
            _context = context;
        }

        // TÜM ÜYELERİ GETİR
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uye>>> GetUyeler()
        {
            return await _context.Uyeler.ToListAsync();
        }

        // TEK ÜYE GETİR
        [HttpGet("{id}")]
        public async Task<ActionResult<Uye>> GetUye(int id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();
            return uye;
        }
    }
}