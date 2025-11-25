using Microsoft.AspNetCore.SignalR;
using BookAPI.Services;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BookAPI.Hubs
{
    public class BooksHub : Hub
    {
        private readonly IBookObservableService _bookObservableService;

        public BooksHub(IBookObservableService bookObservableService)
        {
            _bookObservableService = bookObservableService;
        }

        public async Task SubscribeToPythonBooks()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "PythonBooks");

            // Send initial data immediately
            var observable = _bookObservableService.GetPythonBooksObservable();
            var firstBooks = await observable.Take(1).FirstAsync();
            await Clients.Caller.SendAsync("PythonBooksUpdated", firstBooks);

            // No need to subscribe here - ObservableService handles broadcasting via IHubContext
        }
    }
}

