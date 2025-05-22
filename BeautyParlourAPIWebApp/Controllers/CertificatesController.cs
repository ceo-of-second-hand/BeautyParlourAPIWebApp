using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyParlourAPIWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly BeautyParlourAPIContext _context;

        public CertificatesController(BeautyParlourAPIContext context)
        {
            _context = context;
            // Optionally add default data here
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Certificate>>> Get()
        {
            return await _context.Certificates.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Certificate>> Get(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null) return NotFound();
            return certificate;
        }

        [HttpPost]
        public async Task<ActionResult<Certificate>> Post(Certificate certificate)
        {
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = certificate.Id }, certificate);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Certificate certificate)
        {
            if (id != certificate.Id) return BadRequest();
            _context.Entry(certificate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null) return NotFound();
            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 