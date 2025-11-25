using BookAPI.Models;
using BookAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookDbContext _context;

        public BookRepository(BookDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.Books.ToListAsync(cancellationToken);
        }

        public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _context.Books.Add(book);
            await _context.SaveChangesAsync(cancellationToken);
            return book;
        }

        public async Task<Book?> UpdateAsync(int id, Book book, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (existingBook == null)
                return null;

            existingBook.Name = book.Name;
            existingBook.Price = book.Price;
            await _context.SaveChangesAsync(cancellationToken);
            return existingBook;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<Book>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Book>();

            cancellationToken.ThrowIfCancellationRequested();

            var results = await _context.Books
                .Where(b => b.Name.Contains(searchTerm))
                .ToListAsync(cancellationToken);

            return results;
        }

        public async IAsyncEnumerable<Book> SearchByNameStreamAsync(string searchTerm, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                yield break;

            // استفاده از AsAsyncEnumerable برای استریم واقعی از دیتابیس
            // این روش رکوردها را یکی یکی از دیتابیس می‌خواند و اگر در حین خواندن
            // داده‌ای تغییر کند، در نتایج نخواهد بود
            var query = _context.Books
                .Where(b => b.Name.Contains(searchTerm))
                .OrderBy(b => b.Id)
                .AsAsyncEnumerable();

            await foreach (var book in query.WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // شبیه‌سازی تاخیر برای نمایش استریم
                await Task.Delay(500, cancellationToken);
                
                cancellationToken.ThrowIfCancellationRequested();
                
                yield return book;
            }
        }
    }
}

