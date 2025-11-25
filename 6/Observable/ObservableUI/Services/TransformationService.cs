using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ارتباط با TransformationController API
/// Service for communicating with TransformationController API
/// </summary>
public class TransformationService
{
    private readonly Subject<Product> _productSubject = new();
    private readonly Subject<RawSensorReading> _rawSensorSubject = new();
    private readonly Subject<UserActivity> _userActivitySubject = new();

    /// <summary>
    /// شروع جریان محصولات
    /// Start products stream
    /// </summary>
    public void StartProductsStream()
    {
        StartSimulatedProducts();
    }

    /// <summary>
    /// شروع جریان داده های سنسور خام
    /// Start raw sensor data stream
    /// </summary>
    public void StartSensorDataStream()
    {
        StartSimulatedSensorData();
    }

    /// <summary>
    /// شروع جریان فعالیت های کاربر
    /// Start user activities stream
    /// </summary>
    public void StartUserActivitiesStream()
    {
        StartSimulatedUserActivities();
    }

    /// <summary>
    /// Observable برای محصولات
    /// Observable for products
    /// </summary>
    public IObservable<Product> Products => _productSubject.AsObservable();

    /// <summary>
    /// Observable برای داده های سنسور خام
    /// Observable for raw sensor readings
    /// </summary>
    public IObservable<RawSensorReading> RawSensorReadings => _rawSensorSubject.AsObservable();

    /// <summary>
    /// Observable برای فعالیت های کاربر
    /// Observable for user activities
    /// </summary>
    public IObservable<UserActivity> UserActivities => _userActivitySubject.AsObservable();

    /// <summary>
    /// محصولات گران‌قیمت (فیلتر شده)
    /// Expensive products (filtered)
    /// </summary>
    public IObservable<Product> ExpensiveProducts(decimal minPrice) =>
        Products.Where(p => p.Price > minPrice);

    /// <summary>
    /// محصولات با قیمت تبدیل شده به یورو
    /// Products with price converted to EUR
    /// </summary>
    public IObservable<ProductEUR> ProductsInEUR(decimal rate) =>
        Products.Select(p => new ProductEUR
        {
            Id = p.Id,
            Name = p.Name,
            PriceEUR = Math.Round(p.Price / rate, 2),
            Category = p.Category,
            Timestamp = p.Timestamp
        });

    /// <summary>
    /// داده های سنسور پاک‌سازی شده
    /// Cleaned sensor data
    /// </summary>
    public IObservable<ProcessedSensorData> CleanedSensorData =>
        RawSensorReadings
            .Distinct(r => r.SensorId)
            .Where(r => r.Temperature > -50 && r.Temperature < 100)
            .Select(r => new ProcessedSensorData
            {
                SensorId = r.SensorId,
                Temperature = Math.Round(r.Temperature, 1),
                Humidity = Math.Round(r.Humidity, 1),
                Quality = r.Temperature > -50 && r.Temperature < 100 ? "Valid" : "Invalid",
                Timestamp = r.Timestamp
            });

    /// <summary>
    /// آمار فعالیت کاربران
    /// User activity statistics
    /// </summary>
    public IObservable<UserActivityStats> UserActivityStats =>
        UserActivities
            .GroupBy(activity => activity.UserId)
            .Select(group =>
                group.Scan(new UserActivityStats
                {
                    UserId = group.Key,
                    TotalActions = 0,
                    LastActivity = DateTime.MinValue
                }, (stats, activity) => new UserActivityStats
                {
                    UserId = group.Key,
                    TotalActions = stats.TotalActions + 1,
                    LastActivity = activity.Timestamp
                })
            )
            .Merge();

    /// <summary>
    /// داده های سنسور تجمیع شده
    /// Aggregated sensor data
    /// </summary>
    public IObservable<AggregatedSensorData> AggregatedSensorData =>
        RawSensorReadings
            .Where(r => r.SensorId == "TEMP001")
            .Scan((Count: 0, SumTemperature: 0.0, MinTemperature: double.MaxValue, MaxTemperature: double.MinValue),
                  (agg, reading) => (
                      Count: agg.Count + 1,
                      SumTemperature: agg.SumTemperature + reading.Temperature,
                      MinTemperature: Math.Min(agg.MinTemperature, reading.Temperature),
                      MaxTemperature: Math.Max(agg.MaxTemperature, reading.Temperature)
                  ))
            .Select(agg => new AggregatedSensorData
            {
                SensorId = "TEMP001",
                Count = agg.Count,
                SumTemperature = agg.SumTemperature,
                AverageTemperature = agg.Count > 0 ? agg.SumTemperature / agg.Count : 0,
                MinTemperature = agg.MinTemperature,
                MaxTemperature = agg.MaxTemperature,
                LastUpdate = DateTime.UtcNow
            });

    /// <summary>
    /// محصولات با تخفیف
    /// Discounted products
    /// </summary>
    public IObservable<DiscountedProduct> DiscountedProducts(int discountPercent) =>
        Products.Select(p =>
        {
            var discount = p.Category == "Electronics" ? discountPercent : discountPercent / 2;
            var discountedPrice = p.Price * (100 - discount) / 100;
            return new DiscountedProduct
            {
                Id = p.Id,
                Name = p.Name,
                OriginalPrice = p.Price,
                DiscountedPrice = Math.Round(discountedPrice, 2),
                DiscountPercent = discount,
                Category = p.Category,
                Timestamp = p.Timestamp
            };
        });

    /// <summary>
    /// خلاصه فعالیت کاربران
    /// User activity summary
    /// </summary>
    public IObservable<ActivitySummary> ActivitySummary =>
        UserActivities.Select(activity => new ActivitySummary
        {
            UserId = activity.UserId,
            Action = activity.Action,
            Duration = activity.DurationMs,
            Category = GetActionCategory(activity.Action),
            IsHighValue = activity.DurationMs > 5000 || activity.Action.Contains("Purchase"),
            Timestamp = activity.Timestamp
        });

    /// <summary>
    /// محصولات مرتب شده بر اساس قیمت
    /// Products sorted by price
    /// </summary>
    public IObservable<Product> ProductsSortedByPrice =>
        Products.Buffer(10).SelectMany(buffer => buffer.OrderBy(p => p.Price));

    private void StartSimulatedProducts()
    {
        var names = new[]
        {
            "Laptop", "Smartphone", "Tablet", "Headphones", "Mouse",
            "Keyboard", "Monitor", "Printer", "Router", "Webcam"
        };
        var categories = new[] { "Electronics", "Accessories", "Computers", "Audio" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = names[random.Next(names.Length)],
                    Price = Math.Round((decimal)(random.NextDouble() * 2000 + 50), 2),
                    Category = categories[random.Next(categories.Length)],
                    Timestamp = DateTime.UtcNow
                };
                _productSubject.OnNext(product);
            });
    }

    private void StartSimulatedSensorData()
    {
        var sensors = new[] { "TEMP001", "TEMP002", "HUM001", "PRES001" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var reading = new RawSensorReading
                {
                    SensorId = sensors[random.Next(sensors.Length)],
                    Temperature = Math.Round(random.NextDouble() * 50 + 10, 1),
                    Humidity = Math.Round(random.NextDouble() * 100, 1),
                    Pressure = Math.Round(random.NextDouble() * 50 + 950, 1),
                    Timestamp = DateTime.UtcNow
                };
                _rawSensorSubject.OnNext(reading);
            });
    }

    private void StartSimulatedUserActivities()
    {
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var actions = new[]
        {
            "ViewProduct", "AddToCart", "RemoveFromCart", "Purchase",
            "Search", "Login", "Logout", "ViewProfile", "UpdateSettings"
        };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var activity = new UserActivity
                {
                    UserId = users[random.Next(users.Length)],
                    Action = actions[random.Next(actions.Length)],
                    DurationMs = random.Next(100, 10000),
                    PageUrl = $"/page/{random.Next(1, 21)}",
                    Timestamp = DateTime.UtcNow
                };
                _userActivitySubject.OnNext(activity);
            });
    }

    private static string GetActionCategory(string action)
    {
        return action switch
        {
            "Purchase" or "AddToCart" or "RemoveFromCart" => "Commerce",
            "Login" or "Logout" or "ViewProfile" or "UpdateSettings" => "Account",
            "Search" or "ViewProduct" => "Discovery",
            _ => "Other"
        };
    }
}

// مدل های داده برای تبدیل
// Transformation data models
public record Product
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record ProductEUR
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal PriceEUR { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record RawSensorReading
{
    public string SensorId { get; init; } = string.Empty;
    public double Temperature { get; init; }
    public double Humidity { get; init; }
    public double Pressure { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProcessedSensorData
{
    public string SensorId { get; init; } = string.Empty;
    public double Temperature { get; init; }
    public double Humidity { get; init; }
    public string Quality { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record UserActivity
{
    public string UserId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int DurationMs { get; init; }
    public string PageUrl { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record UserActivityStats
{
    public string UserId { get; init; } = string.Empty;
    public int TotalActions { get; init; }
    public DateTime LastActivity { get; init; }
}

public record AggregatedSensorData
{
    public string SensorId { get; init; } = string.Empty;
    public int Count { get; init; }
    public double SumTemperature { get; init; }
    public double AverageTemperature { get; init; }
    public double MinTemperature { get; init; }
    public double MaxTemperature { get; init; }
    public DateTime LastUpdate { get; init; }
}

public record DiscountedProduct
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal DiscountedPrice { get; init; }
    public int DiscountPercent { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record ActivitySummary
{
    public string UserId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsHighValue { get; init; }
    public DateTime Timestamp { get; init; }
}

