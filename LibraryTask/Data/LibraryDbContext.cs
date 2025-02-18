using LibraryTask.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryTask.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        { }

        // DbSet для таблиці книг
        public DbSet<Book> Books { get; set; }
    }
}
