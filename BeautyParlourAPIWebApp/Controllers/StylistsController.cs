using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyParlourAPIWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StylistsController : ControllerBase
    {
        private readonly BeautyParlourAPIContext _context;

        public StylistsController(BeautyParlourAPIContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stylist>>> Get()
        {
            return await _context.Stylists.Include(s => s.Boards).Include(s => s.Certificates).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Stylist>> Get(int id)
        {
            var stylist = await _context.Stylists.Include(s => s.Boards).Include(s => s.Certificates).FirstOrDefaultAsync(s => s.Id == id);
            if (stylist == null) return NotFound();
            return stylist;
        }

        [HttpPost]
        public async Task<ActionResult<Stylist>> Post(Stylist stylist)
        {
            _context.Stylists.Add(stylist);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = stylist.Id }, stylist);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Stylist stylist)
        {
            if (id != stylist.Id) return BadRequest();
            _context.Entry(stylist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stylist = await _context.Stylists.FindAsync(id);
            if (stylist == null) return NotFound();
            _context.Stylists.Remove(stylist);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("{id}/boards")]
        public async Task<ActionResult<IEnumerable<Board>>> GetBoardsForStylist(int id)
        {
            var stylist = await _context.Stylists
                .Include(s => s.Boards)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stylist == null)
            {
                return NotFound();
            }

            return Ok(stylist.Boards);
        }

    }
} 