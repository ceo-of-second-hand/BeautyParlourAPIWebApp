using BeautyParlourAPIWebApp.Controllers;
using BeautyParlourAPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BeautyParlourAPIWebApp.Tests
{
    public class BoardsControllerTests
    {
        // Helper method to create a new in-memory database context
        private BeautyParlourAPIContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BeautyParlourAPIContext>()
                // Use a unique database name for each test to ensure isolation
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new BeautyParlourAPIContext(options);
            // Ensure the database is created for InMemory
            context.Database.EnsureCreated();
            return context;
        }

        // Helper method to seed the database with test data
        private void SeedDatabase(BeautyParlourAPIContext context)
        {
            var hashtag1 = new Hashtag { Id = 1, Tag = "nails" };
            var hashtag2 = new Hashtag { Id = 2, Tag = "diva" };

            var item1 = new PortfolioItem
            {
                Id = 1,
                BoardId = 1,
                AfterPhotoUrl = "image1.jpg",
                Price = 10.00m,
                PortfolioItemHashtags = new List<PortfolioItemHashtag>
                {
                    new PortfolioItemHashtag { Id = 1, HashtagId = hashtag1.Id, Hashtag = hashtag1 }
                }
            };
            var item2 = new PortfolioItem
            {
                Id = 2,
                BoardId = 1,
                AfterPhotoUrl = "image2.jpg",
                Price = 20.00m,
                PortfolioItemHashtags = new List<PortfolioItemHashtag>
                {
                    new PortfolioItemHashtag { Id = 2, HashtagId = hashtag1.Id, Hashtag = hashtag1 },
                    new PortfolioItemHashtag { Id = 3, HashtagId = hashtag2.Id, Hashtag = hashtag2 }
                }
            };
            var item3 = new PortfolioItem // Item for a different board
            {
                Id = 3,
                BoardId = 2,
                AfterPhotoUrl = "image3.jpg",
                Price = 30.00m,
                PortfolioItemHashtags = new List<PortfolioItemHashtag>
                {
                    new PortfolioItemHashtag { Id = 4, HashtagId = hashtag2.Id, Hashtag = hashtag2 }
                }
            };


            var board1 = new Board { Id = 1, Name = "Nails Board", Items = new List<PortfolioItem> { item1, item2 } };
            var board2 = new Board { Id = 2, Name = "Hair Board", Items = new List<PortfolioItem> { item3 } };
            var board3 = new Board { Id = 3, Name = "Empty Board", Items = new List<PortfolioItem>() }; // Board with no items


            context.Hashtags.AddRange(hashtag1, hashtag2);
            context.Boards.AddRange(board1, board2, board3);
            context.PortfolioItems.AddRange(item1, item2, item3);
            // For InMemory, adding items with their relationships populated is usually enough to seed.
            // The relationships within PortfolioItemHashtags should be set up on the PortfolioItem side in the seed data.

            context.SaveChanges();
        }

        [Fact] // Test for GET /api/boards
        public async Task Get_ReturnsAllBoardsWithItemsAndHashtags()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context);
            var controller = new BoardsController(context);

            // Act
            var result = await controller.Get();

            // Assert
            // Check that the result is an ActionResult containing an IEnumerable of Board
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Board>>>(result);
            var boards = Assert.IsAssignableFrom<IEnumerable<Board>>(actionResult.Value);

            // Check the number of boards returned
            Assert.Equal(3, boards.Count());

            // Check that items and hashtags are included and correct for a sample board
            var board1 = boards.FirstOrDefault(b => b.Id == 1);
            Assert.NotNull(board1);
            Assert.Equal("Nails Board", board1.Name);
            Assert.NotNull(board1.Items);
            Assert.Equal(2, board1.Items.Count);

            var item1InResult = board1.Items.FirstOrDefault(item => item.Id == 1);
            Assert.NotNull(item1InResult);
            Assert.NotNull(item1InResult.PortfolioItemHashtags);
            Assert.Single(item1InResult.PortfolioItemHashtags);
            Assert.Equal("nails", item1InResult.PortfolioItemHashtags.First().Hashtag.Tag);

            var item2InResult = board1.Items.FirstOrDefault(item => item.Id == 2);
            Assert.NotNull(item2InResult);
            Assert.NotNull(item2InResult.PortfolioItemHashtags);
            Assert.Equal(2, item2InResult.PortfolioItemHashtags.Count);
            Assert.Contains(item2InResult.PortfolioItemHashtags, pih => pih.Hashtag.Tag == "nails");
            Assert.Contains(item2InResult.PortfolioItemHashtags, pih => pih.Hashtag.Tag == "diva");
        }

        [Fact] // Test for GET /api/boards/{id} with a valid ID
        public async Task GetById_ReturnsCorrectBoardForValidId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context);
            var controller = new BoardsController(context);
            var boardIdToGet = 1;

            // Act
            var result = await controller.Get(boardIdToGet);

            // Assert
            // Check that the result is an ActionResult containing a Board
            var actionResult = Assert.IsType<ActionResult<Board>>(result);
            var board = Assert.IsAssignableFrom<Board>(actionResult.Value);

            // Check that the correct board was returned
            Assert.NotNull(board);
            Assert.Equal(boardIdToGet, board.Id);
            Assert.Equal("Nails Board", board.Name);

            // Check that items are included
            Assert.NotNull(board.Items);
            Assert.Equal(2, board.Items.Count);
        }

        [Fact] // Test for GET /api/boards/{id} with an invalid ID
        public async Task GetById_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context); // Seed with some data, but not the ID we'll request
            var controller = new BoardsController(context);
            var invalidBoardId = 99; // An ID that does not exist

            // Act
            var result = await controller.Get(invalidBoardId);

            // Assert
            // Check that the result is a NotFoundResult
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact] // Test for POST /api/boards with valid data
        public async Task Post_CreatesNewBoardAndReturnsCreatedAtAction()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // No need to seed existing boards if we're testing creation
            var controller = new BoardsController(context);
            var newBoardDto = new CreateBoardDto
            {
                Name = "New Test Board",
                ThemeColor = "#ffffff",
                StylistId = 1 // Assuming StylistId can be null or exists
            };

            // Act
            var result = await controller.Post(newBoardDto);

            // Assert
            // Check that the result is a CreatedAtActionResult
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            // Check that the value is a Board object
            var board = Assert.IsType<Board>(createdAtActionResult.Value);

            // Check that the board was saved to the database
            Assert.Equal(1, context.Boards.Count()); // Should be one board now (the one we added)
            Assert.Equal("New Test Board", board.Name);
            Assert.Equal("#ffffff", board.ThemeColor);
            Assert.Equal(1, board.StylistId);

            // Check the route values in CreatedAtAction result
            Assert.Equal(nameof(BoardsController.Get), createdAtActionResult.ActionName);
            Assert.Equal(board.Id, createdAtActionResult.RouteValues["id"]); // Check the ID in route values
        }

        [Fact] // Test for PUT /api/boards/{id} with valid data
        public async Task Put_UpdatesExistingBoardAndReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context); // Seed with the board we will update
            var controller = new BoardsController(context);
            var boardIdToUpdate = 1;
            var updatedBoardDto = new UpdateBoardDto
            {
                Name = "Updated Nails Board",
                TitleImageUrl = "updated_image.jpg",
                ThemeColor = "#abcdef"
            };

            // Act
            var result = await controller.Put(boardIdToUpdate, updatedBoardDto);

            // Assert
            // Check that the result is NoContent
            Assert.IsType<NoContentResult>(result);

            // Check that the board in the database was updated
            var updatedBoard = await context.Boards.FindAsync(boardIdToUpdate);
            Assert.NotNull(updatedBoard);
            Assert.Equal("Updated Nails Board", updatedBoard.Name);
            Assert.Equal("updated_image.jpg", updatedBoard.TitleImageUrl);
            Assert.Equal("#abcdef", updatedBoard.ThemeColor);
        }

        [Fact] // Test for PUT /api/boards/{id} with an invalid ID
        public async Task Put_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context); // Seed with data, but not the ID we'll try to update
            var controller = new BoardsController(context);
            var invalidBoardId = 99; // An ID that does not exist
            var updatedBoardDto = new UpdateBoardDto
            {
                Name = "Attempted Update"
            };

            // Act
            var result = await controller.Put(invalidBoardId, updatedBoardDto);

            // Assert
            // Check that the result is NotFound
            Assert.IsType<NotFoundResult>(result);

            // Ensure no changes were made to the database (optional, but good practice)
            Assert.Equal(3, context.Boards.Count()); // Should still have the initial seeded boards
        }


        [Fact] // Test for DELETE /api/boards/{id} with a valid ID
        public async Task Delete_RemovesBoardAndReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context); // Seed with the board we will delete
            var controller = new BoardsController(context);
            var boardIdToDelete = 1;

            // Check that the board exists before deletion
            Assert.NotNull(await context.Boards.FindAsync(boardIdToDelete));

            // Act
            var result = await controller.Delete(boardIdToDelete);

            // Assert
            // Check that the result is NoContent
            Assert.IsType<NoContentResult>(result);

            // Check that the board was removed from the database
            Assert.Null(await context.Boards.FindAsync(boardIdToDelete));
            Assert.Equal(2, context.Boards.Count()); // Should have one less board now
        }

        [Fact] // Test for DELETE /api/boards/{id} with an invalid ID
        public async Task Delete_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedDatabase(context); // Seed with data, but not the ID we'll try to delete
            var controller = new BoardsController(context);
            var invalidBoardId = 99; // An ID that does not exist

            // Act
            var result = await controller.Delete(invalidBoardId);

            // Assert
            // Check that the result is NotFound
            Assert.IsType<NotFoundResult>(result);

            // Ensure no changes were made to the database (optional, but good practice)
            Assert.Equal(3, context.Boards.Count()); // Should still have the initial seeded boards
        }
    }
}