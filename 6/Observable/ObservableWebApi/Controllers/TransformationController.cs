using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableWebApi.Controllers;

/// <summary>
/// کنترلر نمایش مثال های تبدیل داده با استفاده از عملگرهای واکنشی
/// Controller demonstrating data transformation examples using reactive operators
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransformationController : ControllerBase
{
    private static readonly Subject<Product> _productSubject = new();
    private static readonly Subject<RawSensorReading> _rawSensorSubject = new();
    private static readonly Subject<UserActivity> _userActivitySubject = new();

    static TransformationController()
    {
        // تولید داده‌های محصول
        // Generate product data
        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var product = GenerateProduct();
                _productSubject.OnNext(product);
            });

        // تولید داده‌های سنسور خام
        // Generate raw sensor data
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var reading = GenerateRawSensorReading();
                _rawSensorSubject.OnNext(reading);
            });

        // تولید فعالیت های کاربر
        // Generate user activities
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var activity = GenerateUserActivity();
                _userActivitySubject.OnNext(activity);
            });
    }

    /// <summary>
    /// دریافت محصولات با قیمت بالاتر از حد آستانه (Filter)
    /// Get products with price above threshold (Filter operator)
    /// </summary>
    [HttpGet("products/expensive/{minPrice}")]
    public IObservable<Product> GetExpensiveProducts(decimal minPrice)
    {
        return _productSubject
            .AsObservable()
            .Where(p => p.Price > minPrice);
    }

    /// <summary>
    /// دریافت محصولات با تبدیل قیمت به ارز دیگر (Map/Select)
    /// Get products with price converted to another currency (Map/Select operator)
    /// </summary>
    [HttpGet("products/convert-currency/{rate}")]
    public IObservable<ProductEUR> GetProductsInEUR(decimal rate)
    {
        return _productSubject
            .AsObservable()
            .Select(p => new ProductEUR
            {
                Id = p.Id,
                Name = p.Name,
                PriceEUR = Math.Round(p.Price / rate, 2),
                Category = p.Category,
                Timestamp = p.Timestamp
            });
    }

    /// <summary>
    /// دریافت داده‌های سنسور پاک‌سازی شده (Distinct)
    /// Get cleaned sensor data (Distinct operator - remove duplicates)
    /// </summary>
    [HttpGet("sensor/cleaned")]
    public IObservable<ProcessedSensorData> GetCleanedSensorData()
    {
        return _rawSensorSubject
            .AsObservable()
            .Distinct(r => r.SensorId) // Remove duplicates based on sensor ID
            .Where(r => r.Temperature > -50 && r.Temperature < 100) // Filter invalid readings
            .Select(r => new ProcessedSensorData
            {
                SensorId = r.SensorId,
                Temperature = Math.Round(r.Temperature, 1),
                Humidity = Math.Round(r.Humidity, 1),
                Quality = r.Temperature > -50 && r.Temperature < 100 ? "Valid" : "Invalid",
                Timestamp = r.Timestamp
            });
    }

    /// <summary>
    /// دریافت آمار فعالیت کاربران (GroupBy)
    /// Get user activity statistics (GroupBy operator)
    /// </summary>
    [HttpGet("user-activity/stats")]
    public IObservable<UserActivityStats> GetUserActivityStats()
    {
        return _userActivitySubject
            .AsObservable()
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
    }

    /// <summary>
    /// دریافت داده‌های سنسور تجمیع شده (Scan - running total)
    /// Get aggregated sensor data (Scan operator for running totals)
    /// </summary>
    [HttpGet("sensor/aggregated")]
    public IObservable<AggregatedSensorData> GetAggregatedSensorData()
    {
        return _rawSensorSubject
            .AsObservable()
            .Where(r => r.SensorId == "TEMP001") // Focus on one sensor
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
    }

    /// <summary>
    /// دریافت محصولات با تخفیف (Map with condition)
    /// Get products with discount applied (Conditional Map)
    /// </summary>
    [HttpGet("products/discount/{discountPercent}")]
    public IObservable<DiscountedProduct> GetDiscountedProducts(int discountPercent)
    {
        return _productSubject
            .AsObservable()
            .Select(p =>
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
    }

    /// <summary>
    /// دریافت فعالیت های کاربر با تبدیل به خلاصه (Complex Select)
    /// Get user activities transformed to summary (Complex Select)
    /// </summary>
    [HttpGet("user-activity/summary")]
    public IObservable<ActivitySummary> GetActivitySummary()
    {
        return _userActivitySubject
            .AsObservable()
            .Select(activity => new ActivitySummary
            {
                UserId = activity.UserId,
                Action = activity.Action,
                Duration = activity.DurationMs,
                Category = GetActionCategory(activity.Action),
                IsHighValue = activity.DurationMs > 5000 || activity.Action.Contains("Purchase"),
                Timestamp = activity.Timestamp
            });
    }

    /// <summary>
    /// دریافت محصولات مرتب شده بر اساس قیمت (OrderBy)
    /// Get products sorted by price (OrderBy - Note: This buffers data)
    /// </summary>
    [HttpGet("products/sorted-by-price")]
    public IObservable<Product> GetProductsSortedByPrice()
    {
        // Note: OrderBy requires buffering, use carefully with infinite streams
        return _productSubject
            .AsObservable()
            .Buffer(10) // Buffer 10 items
            .SelectMany(buffer => buffer.OrderBy(p => p.Price));
    }

    private static Product GenerateProduct()
    {
        var names = new[]
        {
            "Laptop", "Smartphone", "Tablet", "Headphones", "Mouse",
            "Keyboard", "Monitor", "Printer", "Router", "Webcam"
        };
        var categories = new[] { "Electronics", "Accessories", "Computers", "Audio" };
        var random = new Random();

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = names[random.Next(names.Length)],
            Price = Math.Round((decimal)(random.NextDouble() * 2000 + 50), 2),
            Category = categories[random.Next(categories.Length)],
            Timestamp = DateTime.UtcNow
        };
    }

    private static RawSensorReading GenerateRawSensorReading()
    {
        var sensors = new[] { "TEMP001", "TEMP002", "HUM001", "PRES001" };
        var random = new Random();

        return new RawSensorReading
        {
            SensorId = sensors[random.Next(sensors.Length)],
            Temperature = Math.Round(random.NextDouble() * 50 + 10, 1),
            Humidity = Math.Round(random.NextDouble() * 100, 1),
            Pressure = Math.Round(random.NextDouble() * 50 + 950, 1),
            Timestamp = DateTime.UtcNow
        };
    }

    private static UserActivity GenerateUserActivity()
    {
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var actions = new[]
        {
            "ViewProduct", "AddToCart", "RemoveFromCart", "Purchase",
            "Search", "Login", "Logout", "ViewProfile", "UpdateSettings"
        };
        var random = new Random();

        return new UserActivity
        {
            UserId = users[random.Next(users.Length)],
            Action = actions[random.Next(actions.Length)],
            DurationMs = random.Next(100, 10000),
            PageUrl = $"/page/{random.Next(1, 21)}",
            Timestamp = DateTime.UtcNow
        };
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
