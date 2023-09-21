using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace BookStoreAPI.Data
{
    public class BookStoreDbContext : DbContext
    {
        public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }

        // Configure entity relationships and properties
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure default character set to Utf8Mb4
            modelBuilder.HasCharSet(CharSet.Utf8Mb4);

            modelBuilder.Entity<Author>()
                .HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId);

            modelBuilder.Entity<Genre>()
                .HasMany(g => g.Books)
                .WithOne(b => b.Genre)
                .HasForeignKey(b => b.GenreId);

            // Additional configuration and seed data can be added here
        }
    }
}