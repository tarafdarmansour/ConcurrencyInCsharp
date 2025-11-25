using System.Collections.Concurrent;
using System.Diagnostics;

namespace ValueTaskDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Practical ValueTask Usage Demo ===\n");

        // Example 1: Cache with ValueTask - preventing unnecessary memory allocation
        //await DemonstrateCachingWithValueTask();

        // Example 2: Performance comparison Task vs ValueTask
        //await DemonstratePerformanceComparison();

        // Example 3: Usage in Repository Pattern
        //await DemonstrateRepositoryPattern();

        // Example 4: Usage in API with Cache
        //await DemonstrateApiWithCache();

        // Example 5: Usage with CancellationToken
        //await DemonstrateWithCancellationToken();

        // Example 6: Usage in LINQ operations
        await DemonstrateWithLinq();

        // Example 7: Usage in IAsyncEnumerable
        //await DemonstrateWithAsyncEnumerable();

        Console.WriteLine("\n=== All examples completed ===");
    }

    // Example 1: Using ValueTask for Cache
    // This pattern is useful when the result may already be in memory
    static async Task DemonstrateCachingWithValueTask()
    {
        Console.WriteLine("--- Example 1: Cache with ValueTask ---");
        
        var userService = new UserService();
        
        // First call - must read from database
        var sw = Stopwatch.StartNew();
        var user1 = await userService.GetUserAsync(1);
        sw.Stop();
        Console.WriteLine($"User 1: {user1.Name} - Time: {sw.ElapsedMilliseconds}ms");

        // Subsequent calls - uses Cache
        sw.Restart();
        var user2 = await userService.GetUserAsync(1);
        sw.Stop();
        Console.WriteLine($"User 1 (from Cache): {user2.Name} - Time: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var user3 = await userService.GetUserAsync(1);
        sw.Stop();
        Console.WriteLine($"User 1 (from Cache): {user3.Name} - Time: {sw.ElapsedMilliseconds}ms");

        Console.WriteLine();
    }

    // Example 2: Performance comparison Task vs ValueTask
    static async Task DemonstratePerformanceComparison()
    {
        Console.WriteLine("--- Example 2: Performance Comparison Task vs ValueTask ---");
        
        const int iterations = 50000;
        
        // Test with Task
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await GetValueWithTask(i % 2 == 0);
        }
        sw.Stop();
        var taskTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Task: {taskTime}ms for {iterations} calls");

        // Test with ValueTask
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            await GetValueWithValueTask(i % 2 == 0);
        }
        sw.Stop();
        var valueTaskTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"ValueTask: {valueTaskTime}ms for {iterations} calls");

        Console.WriteLine($"Difference: {taskTime - valueTaskTime}ms ({((double)(taskTime - valueTaskTime) / taskTime * 100):F1}% faster)");
        Console.WriteLine();
    }

    // Example 3: Usage in Repository Pattern
    static async Task DemonstrateRepositoryPattern()
    {
        Console.WriteLine("--- Example 3: Repository Pattern with ValueTask ---");
        
        var productRepository = new ProductRepository();
        
        // Reading products
        var products1 = await productRepository.GetAllAsync();
        Console.WriteLine($"Products (first time): {products1.Count} items");

        var products2 = await productRepository.GetAllAsync();
        Console.WriteLine($"Products (from Cache): {products2.Count} items");

        // Reading a specific product
        var product = await productRepository.GetByIdAsync(5);
        Console.WriteLine($"Product 5: {product?.Name ?? "Not found"}");

        Console.WriteLine();
    }

    // Example 4: Usage in API with Cache
    static async Task DemonstrateApiWithCache()
    {
        Console.WriteLine("--- Example 4: API Service with Cache ---");
        
        var weatherService = new WeatherService();
        
        // Multiple concurrent calls
        var tasks = new List<Task<WeatherData>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(weatherService.GetWeatherAsync("Tehran").AsTask());
        }

        var results = await Task.WhenAll(tasks);
        Console.WriteLine($"Weather data for Tehran: {results[0].Temperature}°C");
        Console.WriteLine($"All {results.Length} calls used Cache");
        Console.WriteLine();
    }

    // Example 5: Using ValueTask with CancellationToken
    static async Task DemonstrateWithCancellationToken()
    {
        Console.WriteLine("--- Example 5: ValueTask with CancellationToken ---");
        
        var fileService = new FileService();
        var cts = new CancellationTokenSource();
        
        // Start reading file
        var readTask = fileService.ReadFileAsync("example.txt", cts.Token);
        
        // Cancel after 50ms
        await Task.Delay(50);
        cts.Cancel();
        
        try
        {
            var content = await readTask;
            Console.WriteLine($"File content: {content}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("File read operation was cancelled");
        }
        
        Console.WriteLine();
    }

    // Example 6: Using ValueTask in LINQ operations
    static async Task DemonstrateWithLinq()
    {
        Console.WriteLine("--- Example 6: ValueTask in LINQ Operations ---");
        
        var dataService = new DataService();
        var ids = new[] { 1, 2, 3, 4, 5 };
        
        // Convert to Task for use in LINQ
        var tasks = ids.Select(id => dataService.GetDataAsync(id).AsTask());
        var results = await Task.WhenAll(tasks);
        
        Console.WriteLine($"Retrieved {results.Length} data items:");
        foreach (var data in results)
        {
            Console.WriteLine($"  - {data.Name}: {data.Value}");
        }
        
        Console.WriteLine();
    }

    // Example 7: Using ValueTask in IAsyncEnumerable
    static async Task DemonstrateWithAsyncEnumerable()
    {
        Console.WriteLine("--- Example 7: ValueTask in IAsyncEnumerable ---");
        
        var streamService = new StreamService();
        var count = 0;
        
        await foreach (var item in streamService.GetItemsAsync())
        {
            Console.WriteLine($"  Item {++count}: {item}");
            if (count >= 5) break; // Only first 5 items
        }
        
        Console.WriteLine($"Retrieved {count} items from stream");
        Console.WriteLine();
    }

    // Helper methods for comparison
    static Task<int> GetValueWithTask(bool useCache)
    {
        if (useCache)
        {
            return Task.FromResult(42); // Memory allocation for Task
        }
        return Task.Run(async () =>
        {
            return 42;
        });
    }

    static ValueTask<int> GetValueWithValueTask(bool useCache)
    {
        if (useCache)
        {
            return new ValueTask<int>(42); // No memory allocation
        }
        return new ValueTask<int>(Task.Run(async () =>
        {
            return 42;
        }));
    }
}

// UserService class - practical example with Cache
public class UserService
{
    private readonly ConcurrentDictionary<int, User> _cache = new();
    private int _dbCallCount = 0;

    public async ValueTask<User> GetUserAsync(int id)
    {
        // If in Cache, returns without memory allocation
        if (_cache.TryGetValue(id, out var cachedUser))
        {
            return cachedUser;
        }

        // Simulate database read
        await Task.Delay(100);
        _dbCallCount++;
        
        var user = new User { Id = id, Name = $"User {id}" };
        _cache[id] = user;
        
        Console.WriteLine($"  (Reading from database - call #{_dbCallCount})");
        return user;
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Repository Pattern with ValueTask
public class ProductRepository
{
    private readonly ConcurrentDictionary<int, Product> _productsCache = new();
    private List<Product>? _allProductsCache;
    private readonly object _allProductsLock = new();

    public ValueTask<List<Product>> GetAllAsync()
    {
        // If cached, returns without await
        if (_allProductsCache != null)
        {
            return new ValueTask<List<Product>>(_allProductsCache);
        }

        // Simulate database read
        return new ValueTask<List<Product>>(LoadAllProductsAsync());
    }

    private async Task<List<Product>> LoadAllProductsAsync()
    {
        await Task.Delay(50); // Simulate I/O
        
        lock (_allProductsLock)
        {
            if (_allProductsCache != null)
                return _allProductsCache;

            _allProductsCache = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop" },
                new Product { Id = 2, Name = "Mouse" },
                new Product { Id = 3, Name = "Keyboard" },
                new Product { Id = 4, Name = "Monitor" },
                new Product { Id = 5, Name = "Headphones" }
            };
            
            return _allProductsCache;
        }
    }

    public async ValueTask<Product?> GetByIdAsync(int id)
    {
        if (_productsCache.TryGetValue(id, out var product))
        {
            return product;
        }

        // If not cached, find from all products list
        var allProducts = await GetAllAsync();
        product = allProducts.FirstOrDefault(p => p.Id == id);
        
        if (product != null)
        {
            _productsCache[id] = product;
        }

        return product;
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Weather Service - API example with Cache
public class WeatherService
{
    private readonly ConcurrentDictionary<string, (WeatherData data, DateTime timestamp)> _cache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public async ValueTask<WeatherData> GetWeatherAsync(string city)
    {
        // Check Cache
        if (_cache.TryGetValue(city, out var cached) && 
            DateTime.UtcNow - cached.timestamp < _cacheExpiry)
        {
            return cached.data;
        }

        // Simulate API call
        await Task.Delay(200);
        
        var weather = new WeatherData
        {
            City = city,
            Temperature = Random.Shared.Next(15, 30),
            Condition = "Sunny"
        };

        _cache[city] = (weather, DateTime.UtcNow);
        return weather;
    }
}

public class WeatherData
{
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
}

// File Service - example with CancellationToken
public class FileService
{
    public async ValueTask<string> ReadFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        // Simulate file read
        await Task.Delay(200, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return $"Content of file {fileName}";
    }
}

// Data Service - example for LINQ
public class DataService
{
    private readonly ConcurrentDictionary<int, DataItem> _cache = new();

    public ValueTask<DataItem> GetDataAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            return new ValueTask<DataItem>(cached);
        }

        return new ValueTask<DataItem>(LoadDataAsync(id));
    }

    private async Task<DataItem> LoadDataAsync(int id)
    {
        await Task.Delay(10); // Simulate I/O
        var item = new DataItem { Id = id, Name = $"Item {id}", Value = id * 10 };
        _cache[id] = item;
        return item;
    }
}

public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Stream Service - example with IAsyncEnumerable
public class StreamService
{
    public async IAsyncEnumerable<string> GetItemsAsync()
    {
        for (int i = 1; i <= 10; i++)
        {
            // Using ValueTask for each item
            var item = await GetItemAsync(i);
            yield return item;
        }
    }

    private ValueTask<string> GetItemAsync(int index)
    {
        // If index is even, uses Cache (simulation)
        if (index % 2 == 0)
        {
            return new ValueTask<string>($"Cached Item {index}");
        }

        return new ValueTask<string>(LoadItemAsync(index));
    }

    private async Task<string> LoadItemAsync(int index)
    {
        await Task.Delay(20); // Simulate I/O
        return $"Loaded Item {index}";
    }
}
