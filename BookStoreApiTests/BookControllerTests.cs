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
    public class BookControllerTests
    {
        private BooksController _controller;
        private BookStoreDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Create InMemory DbContext for testing
            var options = new DbContextOptionsBuilder<BookStoreDbContext>()
                .UseInMemoryDatabase(databaseName: "BookStoreTestDb")
                .Options;

            _context = new BookStoreDbContext(options);
            _controller = new BooksController(_context);

            // Seed test data
            _context.Authors.AddRange(
                new Author { Id = 1, Name = "Author 1" },
                new Author { Id = 2, Name = "Author 2" }
            );

            _context.Genres.AddRange(
                new Genre { Id = 1, Name = "Fiction" },
                new Genre { Id = 2, Name = "Thriller" }
            );

            _context.Books.AddRange(
                new Book { Id = 1, Title = "Book 1", AuthorId = 1, GenreId = 1 },
                new Book { Id = 2, Title = "Book 2", AuthorId = 1, GenreId = 2 },
                new Book { Id = 3, Title = "Book 3", AuthorId = 2, GenreId = 1 }
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
        public async Task GetAll_Books_Returns_Correct_Count()
        {
            // Act
            var result = await _controller.GetBooks();

            // Assert
            Assert.AreEqual(3, result.Value.Count());
        }

        [Test]
        public async Task GetBookById_With_Valid_Id_Returns_Correct_Book()
        {
            // Act
            var result = await _controller.GetBook(1) as ActionResult<Book>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Book 1", result.Value.Title);
        }

        [Test]
        public async Task GetBookById_With_Invalid_Id_Returns_NotFound()
        {
            // Act
            var result = await _controller.GetBook(99);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task PostBook_Saves_New_Book()
        {
            // Arrange
            var newBook = new Book { Title = "Book 4", AuthorId = 1, GenreId = 1 };

            // Act
            await _controller.PostBook(newBook);

            // Assert
            var savedBook = await _context.Books.FindAsync(newBook.Id);
            Assert.IsNotNull(savedBook);
            Assert.AreEqual("Book 4", savedBook.Title);
        }

        [Test]
        public async Task PutBook_Updates_Existing_Book()
        {
            // Arrange
            var updatedBook = new Book { Id = 1, Title = "Updated Book 1", AuthorId = 1, GenreId = 1 };

            // Act
            await _controller.PutBook(updatedBook.Id, updatedBook);

            // Assert
            var savedBook = await _context.Books.FindAsync(updatedBook.Id);
            Assert.IsNotNull(savedBook);
            Assert.AreEqual("Updated Book 1", savedBook.Title);
        }

        [Test]
        public async Task DeleteBook_Removes_Book_From_Db()
        {
            // Arrange
            int bookIdToRemove = 1;

            // Act
            await _controller.DeleteBook(bookIdToRemove);

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookIdToRemove);
            Assert.IsNull(deletedBook);
        }
    }
}