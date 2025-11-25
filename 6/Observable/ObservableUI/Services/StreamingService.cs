using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ارتباط با StreamingController API
/// Service for communicating with StreamingController API
/// </summary>
public class StreamingService
{
    private readonly HttpClient _httpClient;
    private readonly Subject<StockPrice> _stockPriceSubject = new();
    private readonly Subject<SensorData> _sensorDataSubject = new();
    private readonly Subject<LogEntry> _logEntrySubject = new();

    public StreamingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// شروع دریافت جریان قیمت های بورس
    /// Start receiving stock price stream
    /// </summary>
    public async Task StartStockPriceStream()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://localhost:5001/api/streaming/stock-prices");

            if (response.IsSuccessStatusCode)
            {
                // در یک برنامه واقعی، اینجا باید از Server-Sent Events یا WebSocket استفاده کنیم
                // In a real application, we should use Server-Sent Events or WebSocket here
                // برای سادگی، داده‌های نمونه تولید می‌کنیم
                // For simplicity, we generate sample data
                StartSimulatedStockPriceStream();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting stock price stream: {ex.Message}");
        }
    }

    /// <summary>
    /// شروع دریافت جریان داده های سنسور
    /// Start receiving sensor data stream
    /// </summary>
    public async Task StartSensorDataStream()
    {
        try
        {
            StartSimulatedSensorDataStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting sensor data stream: {ex.Message}");
        }
    }

    /// <summary>
    /// شروع دریافت جریان لاگ های سیستم
    /// Start receiving system log stream
    /// </summary>
    public async Task StartSystemLogStream()
    {
        try
        {
            StartSimulatedLogStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting system log stream: {ex.Message}");
        }
    }

    /// <summary>
    /// Observable برای قیمت های بورس
    /// Observable for stock prices
    /// </summary>
    public IObservable<StockPrice> StockPrices => _stockPriceSubject;

    /// <summary>
    /// Observable برای داده های سنسور
    /// Observable for sensor data
    /// </summary>
    public IObservable<SensorData> SensorData => _sensorDataSubject;

    /// <summary>
    /// Observable برای لاگ های سیستم
    /// Observable for system logs
    /// </summary>
    public IObservable<LogEntry> SystemLogs => _logEntrySubject;

    /// <summary>
    /// متوقف کردن همه جریان‌ها
    /// Stop all streams
    /// </summary>
    public void StopAllStreams()
    {
        // در یک برنامه واقعی، اینجا باید اتصالات را قطع کنیم
        // In a real application, we should disconnect the streams
        Console.WriteLine("Stopping all streams...");
    }

    private void StartSimulatedStockPriceStream()
    {
        var symbols = new[] { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA" };
        var random = new Random();

        // Simulate observable behavior with a simple timer
        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += (sender, e) =>
        {
            var stock = new StockPrice
            {
                Symbol = symbols[random.Next(symbols.Length)],
                Price = Math.Round((decimal)(random.NextDouble() * 1000 + 100), 2),
                Change = Math.Round((decimal)(random.NextDouble() * 20 - 10), 2),
                Timestamp = DateTime.UtcNow
            };
            _stockPriceSubject.OnNext(stock);
        };
        timer.Start();
    }

    private void StartSimulatedSensorDataStream()
    {
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var sensorData = new SensorData
                {
                    SensorId = $"sensor-{random.Next(1, 11)}",
                    TemperatureC = Math.Round(random.NextDouble() * 40 + 10, 1),
                    Humidity = Math.Round(random.NextDouble() * 100, 1),
                    Timestamp = DateTime.UtcNow
                };
                _sensorDataSubject.OnNext(sensorData);
            });
    }

    private void StartSimulatedLogStream()
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

        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var logEntry = new LogEntry
                {
                    Level = levels[random.Next(levels.Length)],
                    Message = messages[random.Next(messages.Length)],
                    Timestamp = DateTime.UtcNow,
                    Source = $"Component-{random.Next(1, 6)}"
                };
                _logEntrySubject.OnNext(logEntry);
            });
    }
}

// مدل های داده - باید با Web API سازگار باشند
// Data models - must match the Web API models
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

public record LogEntry
{
    public string Level { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string Source { get; init; } = string.Empty;
}
