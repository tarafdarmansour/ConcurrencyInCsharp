using BookAPI.Models;
using System;
using System.Collections.Generic;

namespace BookAPI.Services
{
    public interface IBookObservableService
    {
        IObservable<IEnumerable<Book>> GetPythonBooksObservable();
        void NotifyPythonBooksChanged();
    }
}

