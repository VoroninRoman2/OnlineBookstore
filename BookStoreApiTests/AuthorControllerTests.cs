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
    public class AuthorControllerTests
    {
        private AuthorsController _controller;
        private BookStoreDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Create InMemory DbContext for testing
            var options = new DbContextOptionsBuilder<BookStoreDbContext>()
                .UseInMemoryDatabase(databaseName: "BookStoreTestDb")
                .Options;

            _context = new BookStoreDbContext(options);
            _controller = new AuthorsController(_context);

            // Seed test data
            _context.Authors.AddRange(
                new Author { Id = 1, Name = "Author 1" },
                new Author { Id = 2, Name = "Author 2" },
                new Author { Id = 3, Name = "Author 3" }
            );

            _context.SaveChanges();

            // Detach all tracked entities
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        [TearDown]
        public void Cleanup()
        {
            // Clear the InMemory DbContext to reset the data for each test
            _context.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetAll_Authors_Returns_Correct_Count()
        {
            // Act
            var result = await _controller.GetAuthors();

            // Assert
            Assert.AreEqual(3, result.Value.Count());
        }

        [Test]
        public async Task GetAuthorById_With_Valid_Id_Returns_Correct_Author()
        {
            // Act
            var result = await _controller.GetAuthor(1) as ActionResult<Author>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Author 1", result.Value.Name);
        }

        [Test]
        public async Task GetAuthorById_With_Invalid_Id_Returns_NotFound()
        {
            // Act
            var result = await _controller.GetAuthor(99);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task PostAuthor_Saves_New_Author()
        {
            // Arrange
            var newAuthor = new Author { Name = "Author 4" };

            // Act
            await _controller.PostAuthor(newAuthor);

            // Assert
            var savedAuthor = await _context.Authors.FindAsync(newAuthor.Id);
            Assert.IsNotNull(savedAuthor);
            Assert.AreEqual("Author 4", savedAuthor.Name);
        }

        [Test]
        public async Task PutAuthor_Updates_Existing_Author()
        {
            // Arrange
            var updatedAuthor = new Author { Id = 1, Name = "Updated Author 1" };

            // Act
            await _controller.PutAuthor(updatedAuthor.Id, updatedAuthor);

            // Assert
            var savedAuthor = await _context.Authors.FindAsync(updatedAuthor.Id);
            Assert.IsNotNull(savedAuthor);
            Assert.AreEqual("Updated Author 1", savedAuthor.Name);
        }

        [Test]
        public async Task DeleteAuthor_Removes_Author_From_Db()
        {
            // Arrange
            int authorIdToRemove = 1;

            // Act
            await _controller.DeleteAuthor(authorIdToRemove);

            // Assert
            var deletedAuthor = await _context.Authors.FindAsync(authorIdToRemove);
            Assert.IsNull(deletedAuthor);
        }
    }
}