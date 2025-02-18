using LibraryTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryTask.Data;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _context;

    public BooksController(LibraryDbContext context)
    {
        _context = context;
        Console.WriteLine("Database Connection Initialized");
    }

    // GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        try
        {
            return await _context.Books.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving books: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Books/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { message = "Book not found" });
            }
            return book;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving book {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Books
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(Book book)
    {
        try
        {
            if (book == null)
                return BadRequest(new { message = "Invalid book data" });

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.id }, book);
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database error while adding book: {dbEx.Message}");
            return StatusCode(500, "Database error occurred");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding book: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Books/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        try
        {
            if (id != book.id)
                return BadRequest(new { message = "ID mismatch" });

            _context.Entry(book).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Books.Any(e => e.id == id))
            {
                return NotFound(new { message = "Book not found" });
            }
            return StatusCode(500, "Concurrency error occurred");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating book {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Books/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { message = "Book not found" });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting book {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Books/search?author=xxx&year=yyyy
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
        [FromQuery] string? author, [FromQuery] int? year)
    {
        try
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.author.Contains(author));
            }

            if (year.HasValue)
            {
                query = query.Where(b => b.year == year);
            }

            var result = await query.ToListAsync();

            if (!result.Any())
            {
                return NotFound(new { message = "No books found" });
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching books: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Books/sort?field=title&order=asc
    [HttpGet("sort")]
    public async Task<ActionResult<IEnumerable<Book>>> SortBooks(
        [FromQuery] string field, [FromQuery] string order = "asc")
    {
        try
        {
            IQueryable<Book> query = _context.Books;

            query = field.ToLower() switch
            {
                "id" => order == "desc" ? query.OrderByDescending(b => b.id) : query.OrderBy(b => b.id),
                "title" => order == "desc" ? query.OrderByDescending(b => b.title) : query.OrderBy(b => b.title),
                "author" => order == "desc" ? query.OrderByDescending(b => b.author) : query.OrderBy(b => b.author),
                "year" => order == "desc" ? query.OrderByDescending(b => b.year) : query.OrderBy(b => b.year),
                "genre" => order == "desc" ? query.OrderByDescending(b => b.genre) : query.OrderBy(b => b.genre),
                _ => throw new ArgumentException("Invalid sort field")
            };

            return await query.ToListAsync();
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(new { message = argEx.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sorting books: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}
