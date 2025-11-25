using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BookAPI.Models;
using BookAPI.Repositories;

namespace BookAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(CancellationToken cancellationToken)
        {
            var books = await _bookRepository.GetAllAsync(cancellationToken);
            return Ok(books);
        }

        // GET: api/books/search?term=pattern
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("عبارت جستجو نمی‌تواند خالی باشد.");
            }

            var books = await _bookRepository.SearchByNameAsync(term, cancellationToken);
            return Ok(books);
        }

        // GET: api/books/search/stream?term=pattern
        [HttpGet("search/stream")]
        [Produces("application/x-ndjson")]
        public async Task SearchBooksStream([FromQuery] string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("عبارت جستجو نمی‌تواند خالی باشد.", cancellationToken);
                return;
            }

            Response.ContentType = "application/x-ndjson";
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            await foreach (var book in _bookRepository.SearchByNameStreamAsync(term, cancellationToken))
            {
                var json = JsonSerializer.Serialize(book, jsonOptions);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        // POST: api/books
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(Book book, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                return BadRequest("نام کتاب نمی‌تواند خالی باشد.");
            }

            if (book.Price < 0)
            {
                return BadRequest("قیمت نمی‌تواند منفی باشد.");
            }

            var createdBook = await _bookRepository.CreateAsync(book, cancellationToken);
            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
        }

        // PUT: api/books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book book, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                return BadRequest("نام کتاب نمی‌تواند خالی باشد.");
            }

            if (book.Price < 0)
            {
                return BadRequest("قیمت نمی‌تواند منفی باشد.");
            }

            var updatedBook = await _bookRepository.UpdateAsync(id, book, cancellationToken);
            
            if (updatedBook == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id, CancellationToken cancellationToken)
        {
            var deleted = await _bookRepository.DeleteAsync(id, cancellationToken);
            
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

