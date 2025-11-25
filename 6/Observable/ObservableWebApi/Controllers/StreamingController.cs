using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableWebApi.Controllers;

/// <summary>
/// کنترلر نمایش مثال های جریان داده بلادرنگ با استفاده از IObservable
/// Controller demonstrating real-time data streaming examples using IObservable
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private static readonly Subject<StockPrice> _stockPriceSubject = new();
    private static readonly Subject<SensorData> _sensorDataSubject = new();
    private static readonly Subject<LogEntry> _logEntrySubject = new();

    // شبیه‌سازی تولید داده‌های بورس
    // Simulating stock price data generation
    static StreamingController()
    {
        // تولید داده‌های بورس هر 2 ثانیه
        // Generate stock prices every 2 seconds
        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var stock = GenerateRandomStockPrice();
                _stockPriceSubject.OnNext(stock);
            });

        // تولید داده‌های سنسور هر 1 ثانیه
        // Generate sensor data every 1 second
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var sensorData = GenerateSensorData();
                _sensorDataSubject.OnNext(sensorData);
            });

        // تولید لاگ‌های سیستم هر 3 ثانیه
        // Generate system logs every 3 seconds
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var logEntry = GenerateLogEntry();
                _logEntrySubject.OnNext(logEntry);
            });
    }

    /// <summary>
    /// دریافت جریان قیمت های بورس بلادرنگ
    /// Get real-time stock price stream
    /// </summary>
    [HttpGet("stock-prices")]
    public IObservable<StockPrice> GetStockPrices()
    {
        return _stockPriceSubject.AsObservable();
    }

    /// <summary>
    /// دریافت جریان داده های سنسور بلادرنگ
    /// Get real-time sensor data stream
    /// </summary>
    [HttpGet("sensor-data")]
    public IObservable<SensorData> GetSensorData()
    {
        return _sensorDataSubject.AsObservable();
    }

    /// <summary>
    /// دریافت جریان لاگ های سیستم بلادرنگ
    /// Get real-time system log stream
    /// </summary>
    [HttpGet("system-logs")]
    public IObservable<LogEntry> GetSystemLogs()
    {
        return _logEntrySubject.AsObservable();
    }

    /// <summary>
    /// دریافت قیمت های بورس با فیلتر (فقط قیمت های بالاتر از حد آستانه)
    /// Get filtered stock prices (only prices above threshold)
    /// </summary>
    [HttpGet("stock-prices/filtered/{threshold}")]
    public IObservable<StockPrice> GetFilteredStockPrices(decimal threshold)
    {
        return _stockPriceSubject
            .AsObservable()
            .Where(stock => stock.Price > threshold);
    }

    /// <summary>
    /// دریافت داده های سنسور با تبدیل (تبدیل واحد ها)
    /// Get sensor data with transformation (unit conversion)
    /// </summary>
    [HttpGet("sensor-data/transformed")]
    public IObservable<SensorDataFahrenheit> GetTransformedSensorData()
    {
        return _sensorDataSubject
            .AsObservable()
            .Select(sensor => new SensorDataFahrenheit
            {
                SensorId = sensor.SensorId,
                TemperatureF = sensor.TemperatureC * 9 / 5 + 32,
                Humidity = sensor.Humidity,
                Timestamp = sensor.Timestamp
            });
    }

    private static StockPrice GenerateRandomStockPrice()
    {
        var symbols = new[] { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA" };
        var random = new Random();

        return new StockPrice
        {
            Symbol = symbols[random.Next(symbols.Length)],
            Price = Math.Round((decimal)(random.NextDouble() * 1000 + 100), 2),
            Change = Math.Round((decimal)(random.NextDouble() * 20 - 10), 2),
            Timestamp = DateTime.UtcNow
        };
    }

    private static SensorData GenerateSensorData()
    {
        var random = new Random();
        return new SensorData
        {
            SensorId = $"sensor-{random.Next(1, 11)}",
            TemperatureC = Math.Round(random.NextDouble() * 40 + 10, 1),
            Humidity = Math.Round(random.NextDouble() * 100, 1),
            Timestamp = DateTime.UtcNow
        };
    }

    private static LogEntry GenerateLogEntry()
    {
        var levels = new[] { "INFO", "WARN", "ERROR", "DEBUG" };
        var messages = new[]
        {
            "User login successful",
            "Database connection established",
            "Cache miss occurred",
            "API request processed",
            "Background task completed",
            "Memory usage warning",
            "File upload failed"
        };

        var random = new Random();
        return new LogEntry
        {
            Level = levels[random.Next(levels.Length)],
            Message = messages[random.Next(messages.Length)],
            Timestamp = DateTime.UtcNow,
            Source = $"Component-{random.Next(1, 6)}"
        };
    }
}

// مدل های داده
// Data models
public record StockPrice
{
    public string Symbol { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Change { get; init; }
    public DateTime Timestamp { get; init; }
}

public record SensorData
{
    public string SensorId { get; init; } = string.Empty;
    public double TemperatureC { get; init; }
    public double Humidity { get; init; }
    public DateTime Timestamp { get; init; }
}

public record SensorDataFahrenheit
{
    public string SensorId { get; init; } = string.Empty;
    public double TemperatureF { get; init; }
    public double Humidity { get; init; }
    public DateTime Timestamp { get; init; }
}

public record LogEntry
{
    public string Level { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string Source { get; init; } = string.Empty;
}

