using BookAPI.Repositories;
using BookAPI.Services;
using BookAPI.Hubs;
using BookAPI.Data;
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add SignalR with JSON options
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Register repository
builder.Services.AddScoped<IBookRepository, BookRepository>();

// Register observable service
builder.Services.AddSingleton<IBookObservableService, BookObservableService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable static files and default files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<BooksHub>("/booksHub");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    context.Database.EnsureCreated();
    
    // Seed data if database is empty
    if (!context.Books.Any())
    {
        var books = new List<Book>
        {
            new Book { Name = "Clean Code", Price = 45.50m },
            new Book { Name = "Design Patterns", Price = 52.00m },
            new Book { Name = "The Pragmatic Programmer", Price = 48.99m },
            new Book { Name = "Refactoring", Price = 55.00m },
            new Book { Name = "Code Complete", Price = 62.50m },
            new Book { Name = "Head First Design Patterns", Price = 59.99m },
            new Book { Name = "You Don't Know JS", Price = 39.99m },
            new Book { Name = "Eloquent JavaScript", Price = 42.00m },
            new Book { Name = "Effective Java", Price = 58.75m },
            new Book { Name = "C# in Depth", Price = 49.99m },
            new Book { Name = "CLR via C#", Price = 65.00m },
            new Book { Name = "ASP.NET Core in Action", Price = 54.99m },
            new Book { Name = "Entity Framework Core in Action", Price = 57.50m },
            new Book { Name = "Microservices Patterns", Price = 61.99m },
            new Book { Name = "Building Microservices", Price = 53.25m },
            new Book { Name = "Domain-Driven Design", Price = 60.00m },
            new Book { Name = "Test Driven Development", Price = 46.99m },
            new Book { Name = "The Art of Unit Testing", Price = 44.50m },
            new Book { Name = "Working Effectively with Legacy Code", Price = 51.00m },
            new Book { Name = "Continuous Delivery", Price = 56.75m },
            new Book { Name = "The Phoenix Project", Price = 43.99m },
            new Book { Name = "The DevOps Handbook", Price = 59.50m },
            new Book { Name = "Site Reliability Engineering", Price = 64.99m },
            new Book { Name = "Release It!", Price = 47.25m },
            new Book { Name = "Fluent Python", Price = 52.50m },
            new Book { Name = "Python Tricks", Price = 41.99m },
            new Book { Name = "Dive Into Python 3", Price = 45.25m },
            new Book { Name = "JavaScript: The Good Parts", Price = 40.99m },
            new Book { Name = "TypeScript Deep Dive", Price = 43.50m },
            new Book { Name = "React: Up & Running", Price = 49.25m },
            new Book { Name = "Vue.js in Action", Price = 51.99m },
            new Book { Name = "Angular: The Complete Guide", Price = 55.75m },
            new Book { Name = "Node.js in Action", Price = 48.50m },
            new Book { Name = "Express in Action", Price = 46.75m },
            new Book { Name = "MongoDB: The Definitive Guide", Price = 54.25m },
            new Book { Name = "Redis in Action", Price = 52.99m },
            new Book { Name = "PostgreSQL: Up and Running", Price = 47.50m },
            new Book { Name = "High Performance MySQL", Price = 63.75m },
            new Book { Name = "Designing Data-Intensive Applications", Price = 67.99m }
        };
        
        context.Books.AddRange(books);
        context.SaveChanges();
    }
}

app.Run();

