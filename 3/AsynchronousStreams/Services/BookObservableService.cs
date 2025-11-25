using BookAPI.Models;
using BookAPI.Repositories;
using BookAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BookAPI.Services
{
    public class BookObservableService : IBookObservableService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<BooksHub> _hubContext;
        private readonly BehaviorSubject<IEnumerable<Book>> _pythonBooksSubject;
        private readonly IDisposable _subscription;

        public BookObservableService(IServiceScopeFactory serviceScopeFactory, IHubContext<BooksHub> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _hubContext = hubContext;
            
            // Initialize with current Python books
            var initialBooks = GetPythonBooks();
            _pythonBooksSubject = new BehaviorSubject<IEnumerable<Book>>(initialBooks);

            // Poll for changes every 2 seconds
            _subscription = Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Select(_ => GetPythonBooks())
                .DistinctUntilChanged(new BookListComparer())
                .Subscribe(books =>
                {
                    _pythonBooksSubject.OnNext(books);
                    // Send update to all connected clients via SignalR
                    _ = _hubContext.Clients.Group("PythonBooks").SendAsync("PythonBooksUpdated", books);
                });
        }

        public IObservable<IEnumerable<Book>> GetPythonBooksObservable()
        {
            return _pythonBooksSubject.AsObservable();
        }

        public void NotifyPythonBooksChanged()
        {
            var books = GetPythonBooks();
            _pythonBooksSubject.OnNext(books);
        }

        private async Task<IEnumerable<Book>> GetPythonBooksAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
            return (await bookRepository
                    .SearchByNameAsync("Python"))
                .OrderBy(b => b.Name)
                .ToList();
        }

        private IEnumerable<Book> GetPythonBooks()
        {
            return GetPythonBooksAsync().Result;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _pythonBooksSubject?.Dispose();
        }

        private class BookListComparer : IEqualityComparer<IEnumerable<Book>>
        {
            public bool Equals(IEnumerable<Book>? x, IEnumerable<Book>? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;

                var xList = x.OrderBy(b => b.Id).ToList();
                var yList = y.OrderBy(b => b.Id).ToList();

                if (xList.Count != yList.Count) return false;

                for (int i = 0; i < xList.Count; i++)
                {
                    if (xList[i].Id != yList[i].Id ||
                        xList[i].Name != yList[i].Name ||
                        xList[i].Price != yList[i].Price)
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(IEnumerable<Book> obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
    }
}

