using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeautyParlourAPIWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioItemsController : ControllerBase
    {
        private readonly BeautyParlourAPIContext _context;

        public PortfolioItemsController(BeautyParlourAPIContext context)
        {
            _context = context;
        }

        // GET: /api/portfolioitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioItem>>> Get()
        {
            return await _context.PortfolioItems
                .Include(p => p.Board)
                .Include(p => p.PortfolioItemHashtags)
                    .ThenInclude(pih => pih.Hashtag)
                .ToListAsync();
        }

        // GET: /api/portfolioitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioItem>> Get(int id)
        {
            var item = await _context.PortfolioItems
                .Include(p => p.Board)
                .Include(p => p.PortfolioItemHashtags)
                    .ThenInclude(pih => pih.Hashtag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (item == null) return NotFound();
            return item;
        }

        // POST: /api/portfolioitems
        [HttpPost]
        public async Task<ActionResult<PortfolioItem>> Post([FromBody] CreatePortfolioItemDto itemDto)
        {
            var item = new PortfolioItem
            {
                BoardId = itemDto.BoardId,
                BeforePhotoUrl = itemDto.BeforePhotoUrl,
                AfterPhotoUrl = itemDto.AfterPhotoUrl,
                Price = itemDto.Price,
                Description = "",
                PortfolioItemHashtags = new List<PortfolioItemHashtag>()
            };

            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            if (itemDto.Hashtags != null && itemDto.Hashtags.Any())
            {
                foreach (var hashtagDto in itemDto.Hashtags)
                {
                    var normalizedTag = hashtagDto.Tag.Trim().ToLower();

                    var hashtag = await _context.Hashtags
                        .FirstOrDefaultAsync(h => h.Tag.ToLower() == normalizedTag);

                    if (hashtag == null)
                    {
                        hashtag = new Hashtag { Tag = hashtagDto.Tag.Trim() };
                        _context.Hashtags.Add(hashtag);
                        await _context.SaveChangesAsync();
                    }

                    bool alreadyLinked = await _context.PortfolioItemHashtags
                        .AnyAsync(pih => pih.PortfolioItemId == item.Id && pih.HashtagId == hashtag.Id);

                    if (!alreadyLinked)
                    {
                        var newLink = new PortfolioItemHashtag
                        {
                            PortfolioItemId = item.Id,
                            HashtagId = hashtag.Id
                        };
                        _context.PortfolioItemHashtags.Add(newLink);
                    }
                }
                await _context.SaveChangesAsync();
            }

            await _context.Entry(item)
                .Collection(p => p.PortfolioItemHashtags)
                .LoadAsync();
            await _context.Entry(item)
                 .Reference(p => p.Board)
                 .LoadAsync();
             foreach(var pih in item.PortfolioItemHashtags)
            {
                await _context.Entry(pih).Reference(pih => pih.Hashtag).LoadAsync();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        // PUT: /api/portfolioitems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PortfolioItem item)
        {
            if (id != item.Id) return BadRequest();

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PortfolioItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool PortfolioItemExists(int id)
        {
            return _context.PortfolioItems.Any(e => e.Id == id);
        }

        // DELETE: /api/portfolioitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.PortfolioItems.FindAsync(id);
            if (item == null) return NotFound();
            _context.PortfolioItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: /api/portfolioitems/byhashtag/{tag}
        [HttpGet("byhashtag/{tag}")]
        public async Task<ActionResult<IEnumerable<PortfolioItem>>> GetItemsByHashtag(string tag)
        {
            var items = await _context.PortfolioItems
                .Include(p => p.PortfolioItemHashtags)
                    .ThenInclude(pih => pih.Hashtag)
                .Where(p => p.PortfolioItemHashtags.Any(pih => pih.Hashtag.Tag.ToLower() == tag.ToLower()))
                .ToListAsync();

            return Ok(items);
        }

        // POST: /api/portfolioitems/{id}/add-hashtag
        [HttpPost("{id}/add-hashtag")]
        public async Task<IActionResult> AddHashtagToItem(int id, [FromBody] AddHashtagDto dto)
        {
            var item = await _context.PortfolioItems.FindAsync(id);
            if (item == null) return NotFound("Portfolio item not found");

            var normalizedTag = dto.Tag.Trim().ToLower();

            var hashtag = await _context.Hashtags
                .FirstOrDefaultAsync(h => h.Tag.ToLower() == normalizedTag);

            if (hashtag == null)
            {
                hashtag = new Hashtag { Tag = dto.Tag.Trim() };
                _context.Hashtags.Add(hashtag);
                await _context.SaveChangesAsync();
            }

            bool alreadyLinked = await _context.PortfolioItemHashtags
                .AnyAsync(pih => pih.PortfolioItemId == id && pih.HashtagId == hashtag.Id);

            if (!alreadyLinked)
            {
                var newLink = new PortfolioItemHashtag
                {
                    PortfolioItemId = id,
                    HashtagId = hashtag.Id
                };

                _context.PortfolioItemHashtags.Add(newLink);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Hashtag added successfully" });
        }

        // POST: /api/portfolioitems/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest("Unsupported file type.");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            var imageUrl = $"images/{fileName}";
            return Ok(new { imageUrl });
        }
    }

    // DTOs for Portfolio Items
    public class CreatePortfolioItemDto
    {
        public int BoardId { get; set; }
        public string? BeforePhotoUrl { get; set; }
        public string? AfterPhotoUrl { get; set; }
        public decimal Price { get; set; }
        public List<HashtagDto>? Hashtags { get; set; }
    }

    public class HashtagDto
    {
        public string Tag { get; set; }
    }
}
