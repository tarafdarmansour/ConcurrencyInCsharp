using BookAPI.Models;

namespace BookAPI.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Book> CreateAsync(Book book, CancellationToken cancellationToken = default);
        Task<Book?> UpdateAsync(int id, Book book, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
        IAsyncEnumerable<Book> SearchByNameStreamAsync(string searchTerm, CancellationToken cancellationToken = default);
    }
}

