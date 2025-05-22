using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BeautyParlourAPIWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly BeautyParlourAPIContext _context;

        public BoardsController(BeautyParlourAPIContext context)
        {
            _context = context;
            // Optionally add default data here
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Board>>> Get()
        {
            return await _context.Boards
                .Include(b => b.Items)
                    .ThenInclude(item => item.PortfolioItemHashtags)
                        .ThenInclude(pih => pih.Hashtag)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Board>> Get(int id)
        {
            var board = await _context.Boards.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == id);
            if (board == null) return NotFound();
            return board;
        }

        [HttpPost]
        public async Task<ActionResult<Board>> Post([FromBody] CreateBoardDto boardDto)
        {
            var board = new Board
            {
                Name = boardDto.Name,
                ThemeColor = boardDto.ThemeColor,
                StylistId = boardDto.StylistId // Ensure StylistId is also mapped if it were in the DTO
            };
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Reload board with items for response if needed, similar to Get(id)
             await _context.Entry(board)
                 .Collection(b => b.Items)
                 .LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = board.Id }, board);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateBoardDto boardDto)
        {
            var existingBoard = await _context.Boards.FindAsync(id);
            if (existingBoard == null) return NotFound();

            // Apply updates only if the corresponding property is provided in the DTO
            if (boardDto.Name != null) existingBoard.Name = boardDto.Name;
            if (boardDto.TitleImageUrl != null) existingBoard.TitleImageUrl = boardDto.TitleImageUrl;
            if (boardDto.ThemeColor != null) existingBoard.ThemeColor = boardDto.ThemeColor;
            // Add other properties here if you want to allow updating them via this PUT

            // Entity Framework tracks changes, so SaveChangesAsync will update the modified fields.
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content is standard for successful PUT that doesn't return a resource body
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();
            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateBoardDto
    {
        public string Name { get; set; }
        public string? ThemeColor { get; set; }
        public int? StylistId { get; set; } // Include if StylistId is sent during creation
    }

    public class UpdateBoardDto
    {
        public string? Name { get; set; }
        public string? TitleImageUrl { get; set; }
        public string? ThemeColor { get; set; }
    }
} 