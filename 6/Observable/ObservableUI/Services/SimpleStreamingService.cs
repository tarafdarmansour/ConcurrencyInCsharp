using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ساده برای نمایش جریان داده بلادرنگ
/// Simple service for displaying real-time data streams
/// </summary>
public class SimpleStreamingService
{
    private readonly Subject<StockPrice> _stockPriceSubject = new();
    private readonly Subject<SensorData> _sensorDataSubject = new();
    private readonly Subject<LogEntry> _logEntrySubject = new();

    private System.Timers.Timer? _stockTimer;
    private System.Timers.Timer? _sensorTimer;
    private System.Timers.Timer? _logTimer;

    /// <summary>
    /// شروع جریان قیمت های بورس
    /// Start stock price stream
    /// </summary>
    public void StartStockPriceStream()
    {
        if (_stockTimer != null) return;

        var symbols = new[] { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA" };
        var random = new Random();

        _stockTimer = new System.Timers.Timer(10000);
        _stockTimer.Elapsed += (sender, e) =>
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
        _stockTimer.Start();
    }

    /// <summary>
    /// شروع جریان داده های سنسور
    /// Start sensor data stream
    /// </summary>
    public void StartSensorDataStream()
    {
        if (_sensorTimer != null) return;

        var random = new Random();

        _sensorTimer = new System.Timers.Timer(1000);
        _sensorTimer.Elapsed += (sender, e) =>
        {
            var sensorData = new SensorData
            {
                SensorId = $"sensor-{random.Next(1, 11)}",
                TemperatureC = Math.Round(random.NextDouble() * 40 + 10, 1),
                Humidity = Math.Round(random.NextDouble() * 100, 1),
                Timestamp = DateTime.UtcNow
            };
            _sensorDataSubject.OnNext(sensorData);
        };
        _sensorTimer.Start();
    }

    /// <summary>
    /// شروع جریان لاگ های سیستم
    /// Start system log stream
    /// </summary>
    public void StartSystemLogStream()
    {
        if (_logTimer != null) return;

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

        _logTimer = new System.Timers.Timer(3000);
        _logTimer.Elapsed += (sender, e) =>
        {
            var logEntry = new LogEntry
            {
                Level = levels[random.Next(levels.Length)],
                Message = messages[random.Next(messages.Length)],
                Timestamp = DateTime.UtcNow,
                Source = $"Component-{random.Next(1, 6)}"
            };
            _logEntrySubject.OnNext(logEntry);
        };
        _logTimer.Start();
    }

    /// <summary>
    /// متوقف کردن همه جریان‌ها
    /// Stop all streams
    /// </summary>
    public void StopAllStreams()
    {
        _stockTimer?.Stop();
        _stockTimer?.Dispose();
        _stockTimer = null;

        _sensorTimer?.Stop();
        _sensorTimer?.Dispose();
        _sensorTimer = null;

        _logTimer?.Stop();
        _logTimer?.Dispose();
        _logTimer = null;
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
}

// مدل های داده
// Data models
