using System.Linq;
using System.Threading.Tasks;
using BookStoreAPI.Controllers;
using BookStoreAPI.Data;
using BookStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BookStoreAPI.Tests
{
    public class GenreControllerTests
    {
        private GenresController _controller;
        private BookStoreDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Create InMemory DbContext for testing
            var options = new DbContextOptionsBuilder<BookStoreDbContext>()
                .UseInMemoryDatabase(databaseName: "BookStoreTestDb")
                .Options;

            _context = new BookStoreDbContext(options);
            _controller = new GenresController(_context);

            // Seed test data
            _context.Genres.AddRange(
                new Genre { Id = 1, Name = "Fiction" },
                new Genre { Id = 2, Name = "Thriller" },
                new Genre { Id = 3, Name = "Romance" }
            );
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
            _context.SaveChanges();
        }

        [TearDown]
        public void Cleanup()
        {
            // Clear the InMemory DbContext to reset the data for each test
            _context.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetAll_Genres_Returns_Correct_Count()
        {
            // Act
            var result = await _controller.GetGenres();

            // Assert
            Assert.AreEqual(3, result.Value.Count());
        }

        [Test]
        public async Task GetGenreById_With_Valid_Id_Returns_Correct_Genre()
        {
            // Act
            var result = await _controller.GetGenre(1) as ActionResult<Genre>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Fiction", result.Value.Name);
        }

        [Test]
        public async Task GetGenreById_With_Invalid_Id_Returns_NotFound()
        {
            // Act
            var result = await _controller.GetGenre(99);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task PostGenre_Saves_New_Genre()
        {
            // Arrange
            var newGenre = new Genre { Name = "Science Fiction" };

            // Act
            await _controller.PostGenre(newGenre);

            // Assert
            var savedGenre = await _context.Genres.FindAsync(newGenre.Id);
            Assert.IsNotNull(savedGenre);
            Assert.AreEqual("Science Fiction", savedGenre.Name);
        }

        [Test]
        public async Task PutGenre_Updates_Existing_Genre()
        {
            // Arrange
            var updatedGenre = new Genre { Id = 1, Name = "Mystery" };

            // Act
            await _controller.PutGenre(updatedGenre.Id, updatedGenre);

            // Assert
            var savedGenre = await _context.Genres.FindAsync(updatedGenre.Id);
            Assert.IsNotNull(savedGenre);
            Assert.AreEqual("Mystery", savedGenre.Name);
        }

        [Test]
        public async Task DeleteGenre_Removes_Genre_From_Db()
        {
            // Arrange
            int genreIdToRemove = 1;

            // Act
            await _controller.DeleteGenre(genreIdToRemove);

            // Assert
            var deletedGenre = await _context.Genres.FindAsync(genreIdToRemove);
            Assert.IsNull(deletedGenre);
        }
    }
}