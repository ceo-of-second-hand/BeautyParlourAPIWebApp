using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyParlourAPIWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HashtagsController : ControllerBase
    {
        private readonly BeautyParlourAPIContext _context;

        public HashtagsController(BeautyParlourAPIContext context)
        {
            _context = context;
            // Optionally add default data here
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hashtag>>> Get()
        {
            return await _context.Hashtags.Include(h => h.PortfolioItemHashtags).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Hashtag>> Get(int id)
        {
            var hashtag = await _context.Hashtags.Include(h => h.PortfolioItemHashtags).FirstOrDefaultAsync(h => h.Id == id);
            if (hashtag == null) return NotFound();
            return hashtag;
        }

        [HttpPost]
        public async Task<ActionResult<Hashtag>> Post(Hashtag hashtag)
        {
            _context.Hashtags.Add(hashtag);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = hashtag.Id }, hashtag);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Hashtag hashtag)
        {
            if (id != hashtag.Id) return BadRequest();
            _context.Entry(hashtag).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hashtag = await _context.Hashtags.FindAsync(id);
            if (hashtag == null) return NotFound();
            _context.Hashtags.Remove(hashtag);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<PortfolioItem>>> GetItemsForHashtag(int id)
        {
            var hashtag = await _context.Hashtags
                .Include(h => h.PortfolioItemHashtags)
                    .ThenInclude(pih => pih.PortfolioItem)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hashtag == null)
                return NotFound();

            var items = hashtag.PortfolioItemHashtags
                .Select(pih => pih.PortfolioItem)
                .ToList();

            return Ok(items);
        }
    }
} 