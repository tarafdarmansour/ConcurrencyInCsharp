using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableWebApi.Controllers;

/// <summary>
/// کنترلر نمایش مثال های ترکیب چندین observable
/// Controller demonstrating examples of combining multiple observables
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CombiningController : ControllerBase
{
    private static readonly Subject<TemperatureReading> _tempSensor1 = new();
    private static readonly Subject<TemperatureReading> _tempSensor2 = new();
    private static readonly Subject<HumidityReading> _humiditySensor = new();
    private static readonly Subject<UserEvent> _userEvents = new();
    private static readonly Subject<SystemMetric> _systemMetrics = new();
    private static readonly Subject<OrderCreatedEvent> _orderEvents = new();
    private static readonly Subject<PaymentEvent> _paymentEvents = new();

    static CombiningController()
    {
        // سنسور دما 1
        // Temperature sensor 1
        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var reading = new TemperatureReading
                {
                    SensorId = "TEMP_001",
                    Value = Math.Round(20 + new Random().NextDouble() * 10, 1),
                    Timestamp = DateTime.UtcNow
                };
                _tempSensor1.OnNext(reading);
            });

        // سنسور دما 2
        // Temperature sensor 2
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var reading = new TemperatureReading
                {
                    SensorId = "TEMP_002",
                    Value = Math.Round(18 + new Random().NextDouble() * 12, 1),
                    Timestamp = DateTime.UtcNow
                };
                _tempSensor2.OnNext(reading);
            });

        // سنسور رطوبت
        // Humidity sensor
        Observable.Interval(TimeSpan.FromSeconds(2.5))
            .Subscribe(_ =>
            {
                var reading = new HumidityReading
                {
                    SensorId = "HUM_001",
                    Value = Math.Round(30 + new Random().NextDouble() * 40, 1),
                    Timestamp = DateTime.UtcNow
                };
                _humiditySensor.OnNext(reading);
            });

        // رویدادهای کاربر
        // User events
        Observable.Interval(TimeSpan.FromSeconds(1.5))
            .Subscribe(_ => GenerateUserEvent());

        // متریک های سیستم
        // System metrics
        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ => GenerateSystemMetric());

        // رویدادهای سفارش
        // Order events
        Observable.Interval(TimeSpan.FromSeconds(5))
            .Subscribe(_ => GenerateOrderCreatedEvent());

        // رویدادهای پرداخت
        // Payment events
        Observable.Interval(TimeSpan.FromSeconds(6))
            .Subscribe(_ => GeneratePaymentEvent());
    }

    /// <summary>
    /// ترکیب سنسورهای دما با Merge (ترتیب زمانی حفظ می‌شود)
    /// Combine temperature sensors using Merge (preserves time ordering)
    /// </summary>
    [HttpGet("temperature/merged")]
    public IObservable<TemperatureReading> GetMergedTemperatureReadings()
    {
        return _tempSensor1.Merge(_tempSensor2);
    }

    /// <summary>
    /// ترکیب سنسورهای محیطی با CombineLatest (آخرین مقدار هر سنسور)
    /// Combine environmental sensors using CombineLatest (latest value from each)
    /// </summary>
    [HttpGet("environment/combined-latest")]
    public IObservable<EnvironmentalData> GetCombinedEnvironmentalData()
    {
        return _tempSensor1.CombineLatest(_tempSensor2, (t1, t2) => (t1, t2))
            .CombineLatest(_humiditySensor, (tempPair, hum) => new EnvironmentalData
            {
                Temperature1 = tempPair.t1.Value,
                Temperature2 = tempPair.t2.Value,
                AverageTemperature = (tempPair.t1.Value + tempPair.t2.Value) / 2,
                Humidity = hum.Value,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// ترکیب رویدادهای سفارش و پرداخت با Zip (جفت کردن بر اساس ترتیب)
    /// Combine order and payment events using Zip (pair by order)
    /// </summary>
    [HttpGet("orders/payments/zipped")]
    public IObservable<OrderPaymentPair> GetZippedOrderPayments()
    {
        return _orderEvents.Zip(_paymentEvents, (order, payment) => new OrderPaymentPair
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Amount = order.Amount,
            PaymentMethod = payment.Method,
            PaymentStatus = payment.Status,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// آمار فعالیت کاربران در بازه زمانی با Window و CombineLatest
    /// User activity statistics over time windows with CombineLatest
    /// </summary>
    [HttpGet("user-activity/combined-stats")]
    public IObservable<UserActivityCombined> GetCombinedUserActivityStats()
    {
        var loginEvents = _userEvents.Where(e => e.Action == "LOGIN");
        var purchaseEvents = _userEvents.Where(e => e.Action == "PURCHASE");

        return loginEvents.CombineLatest(purchaseEvents, (login, purchase) =>
        {
            var timeDiff = purchase.Timestamp - login.Timestamp;
            return new UserActivityCombined
            {
                UserId = login.UserId,
                LastLogin = login.Timestamp,
                LastPurchase = purchase.Timestamp,
                TimeToPurchase = timeDiff.TotalMinutes,
                Timestamp = DateTime.UtcNow
            };
        });
    }

    /// <summary>
    /// متریک های سیستم ترکیبی با Concat (اجرای متوالی)
    /// Combined system metrics using Concat (sequential execution)
    /// </summary>
    [HttpGet("system/concatenated")]
    public IObservable<SystemMetric> GetConcatenatedSystemMetrics()
    {
        // Create finite sequences for demonstration
        var cpuMetrics = _systemMetrics.Where(m => m.Type == "CPU").Take(3);
        var memoryMetrics = _systemMetrics.Where(m => m.Type == "MEMORY").Take(3);
        var diskMetrics = _systemMetrics.Where(m => m.Type == "DISK").Take(3);

        return cpuMetrics.Concat(memoryMetrics).Concat(diskMetrics);
    }

    /// <summary>
    /// رویدادهای چندگانه سیستم با Merge
    /// Multiple system events using Merge
    /// </summary>
    [HttpGet("system-events/merged")]
    public IObservable<SystemEventCombined> GetMergedSystemEvents()
    {
        var errorEvents = _systemMetrics
            .Where(m => m.Value > 90)
            .Select(m => new SystemEventCombined
            {
                Type = "ALERT",
                Source = m.Type,
                Message = $"{m.Type} usage is high: {m.Value}%",
                Severity = "HIGH",
                Timestamp = m.Timestamp
            });

        var normalEvents = _systemMetrics
            .Where(m => m.Value <= 90)
            .Select(m => new SystemEventCombined
            {
                Type = "INFO",
                Source = m.Type,
                Message = $"{m.Type} usage: {m.Value}%",
                Severity = "NORMAL",
                Timestamp = m.Timestamp
            });

        return errorEvents.Merge(normalEvents);
    }

    /// <summary>
    /// سنسورهای دما با Amb (اولین سنسور که مقدار ارسال کند)
    /// Temperature sensors with Amb (first sensor to emit wins)
    /// </summary>
    [HttpGet("temperature/amb")]
    public IObservable<TemperatureReading> GetAmbTemperatureReadings()
    {
        return _tempSensor1.Amb(_tempSensor2);
    }

    /// <summary>
    /// داده‌های محیطی با Switch (تغییر به observable جدید)
    /// Environmental data with Switch (switch to new observable)
    /// </summary>
    [HttpGet("environment/switched")]
    public IObservable<EnvironmentalData> GetSwitchedEnvironmentalData()
    {
        // Switch between different environmental data sources
        return Observable.Interval(TimeSpan.FromSeconds(10))
            .Select(_ => _ % 2 == 0 ?
                _tempSensor1.CombineLatest(_humiditySensor, (t, h) => new EnvironmentalData
                {
                    Temperature1 = t.Value,
                    Temperature2 = 0,
                    AverageTemperature = t.Value,
                    Humidity = h.Value,
                    Timestamp = DateTime.UtcNow
                }) :
                _tempSensor2.CombineLatest(_humiditySensor, (t, h) => new EnvironmentalData
                {
                    Temperature1 = 0,
                    Temperature2 = t.Value,
                    AverageTemperature = t.Value,
                    Humidity = h.Value,
                    Timestamp = DateTime.UtcNow
                })
            )
            .Switch();
    }

    /// <summary>
    /// رویدادهای کاربر و متریک های سیستم با Join
    /// User events and system metrics with Join (correlate by time)
    /// </summary>
    [HttpGet("user-system/joined")]
    public IObservable<UserSystemCorrelation> GetJoinedUserSystemData()
    {
        return _userEvents.Join(
            _systemMetrics,
            _ => Observable.Timer(TimeSpan.FromSeconds(5)), // User events valid for 5 seconds
            _ => Observable.Timer(TimeSpan.FromSeconds(5)), // System metrics valid for 5 seconds
            (userEvent, systemMetric) => new UserSystemCorrelation
            {
                UserId = userEvent.UserId,
                Action = userEvent.Action,
                MetricType = systemMetric.Type,
                MetricValue = systemMetric.Value,
                CorrelationTimestamp = DateTime.UtcNow,
                UserTimestamp = userEvent.Timestamp,
                MetricTimestamp = systemMetric.Timestamp
            });
    }

    private static void GenerateUserEvent()
    {
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var actions = new[] { "LOGIN", "VIEW_PAGE", "PURCHASE", "LOGOUT", "SEARCH" };
        var random = new Random();

        var userEvent = new UserEvent
        {
            UserId = users[random.Next(users.Length)],
            Action = actions[random.Next(actions.Length)],
            Timestamp = DateTime.UtcNow
        };

        _userEvents.OnNext(userEvent);
    }

    private static void GenerateSystemMetric()
    {
        var types = new[] { "CPU", "MEMORY", "DISK", "NETWORK" };
        var random = new Random();

        var metric = new SystemMetric
        {
            Type = types[random.Next(types.Length)],
            Value = random.Next(0, 101),
            Timestamp = DateTime.UtcNow
        };

        _systemMetrics.OnNext(metric);
    }

    private static void GenerateOrderCreatedEvent()
    {
        var random = new Random();
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = $"ORD-{random.Next(1000, 9999)}",
            CustomerId = $"CUST-{random.Next(100, 999)}",
            Amount = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
            Timestamp = DateTime.UtcNow
        };

        _orderEvents.OnNext(orderEvent);
    }

    private static void GeneratePaymentEvent()
    {
        var methods = new[] { "CREDIT_CARD", "PAYPAL", "BANK_TRANSFER", "CRYPTO" };
        var statuses = new[] { "PENDING", "COMPLETED", "FAILED", "REFUNDED" };
        var random = new Random();

        var paymentEvent = new PaymentEvent
        {
            PaymentId = $"PAY-{random.Next(1000, 9999)}",
            OrderId = $"ORD-{random.Next(1000, 9999)}",
            Method = methods[random.Next(methods.Length)],
            Amount = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
            Status = statuses[random.Next(statuses.Length)],
            Timestamp = DateTime.UtcNow
        };

        _paymentEvents.OnNext(paymentEvent);
    }
}

// مدل های داده برای ترکیب observables
// Combining observables data models
public record TemperatureReading
{
    public string SensorId { get; init; } = string.Empty;
    public double Value { get; init; }
    public DateTime Timestamp { get; init; }
}

public record HumidityReading
{
    public string SensorId { get; init; } = string.Empty;
    public double Value { get; init; }
    public DateTime Timestamp { get; init; }
}

public record UserEvent
{
    public string UserId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record SystemMetric
{
    public string Type { get; init; } = string.Empty;
    public int Value { get; init; }
    public DateTime Timestamp { get; init; }
}

public record OrderCreatedEvent
{
    public string OrderId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Timestamp { get; init; }
}

public record PaymentEvent
{
    public string PaymentId { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record EnvironmentalData
{
    public double Temperature1 { get; init; }
    public double Temperature2 { get; init; }
    public double AverageTemperature { get; init; }
    public double Humidity { get; init; }
    public DateTime Timestamp { get; init; }
}

public record OrderPaymentPair
{
    public string OrderId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record UserActivityCombined
{
    public string UserId { get; init; } = string.Empty;
    public DateTime LastLogin { get; init; }
    public DateTime LastPurchase { get; init; }
    public double TimeToPurchase { get; init; }
    public DateTime Timestamp { get; init; }
}

public record SystemEventCombined
{
    public string Type { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record UserSystemCorrelation
{
    public string UserId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string MetricType { get; init; } = string.Empty;
    public int MetricValue { get; init; }
    public DateTime CorrelationTimestamp { get; init; }
    public DateTime UserTimestamp { get; init; }
    public DateTime MetricTimestamp { get; init; }
}
