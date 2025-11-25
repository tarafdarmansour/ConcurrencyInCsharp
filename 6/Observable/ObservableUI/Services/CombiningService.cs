using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ارتباط با CombiningController API
/// Service for communicating with CombiningController API
/// </summary>
public class CombiningService
{
    private readonly Subject<TemperatureReading> _tempSensor1 = new();
    private readonly Subject<TemperatureReading> _tempSensor2 = new();
    private readonly Subject<HumidityReading> _humiditySensor = new();
    private readonly Subject<UserEvent> _userEvents = new();
    private readonly Subject<SystemMetric> _systemMetrics = new();
    private readonly Subject<OrderCreatedEvent> _orderEvents = new();
    private readonly Subject<PaymentEvent> _paymentEvents = new();

    /// <summary>
    /// شروع همه سنسورها
    /// Start all sensors
    /// </summary>
    public void StartAllSensors()
    {
        StartTemperatureSensor1();
        StartTemperatureSensor2();
        StartHumiditySensor();
    }

    /// <summary>
    /// شروع رویدادهای کاربر و سیستم
    /// Start user and system events
    /// </summary>
    public void StartEvents()
    {
        StartUserEvents();
        StartSystemMetrics();
        StartOrderEvents();
        StartPaymentEvents();
    }

    /// <summary>
    /// سنسورهای دما با Merge
    /// Temperature sensors with Merge
    /// </summary>
    public IObservable<TemperatureReading> MergedTemperatureReadings =>
        _tempSensor1.Merge(_tempSensor2);

    /// <summary>
    /// داده‌های محیطی با CombineLatest
    /// Environmental data with CombineLatest
    /// </summary>
    public IObservable<EnvironmentalData> CombinedEnvironmentalData =>
        _tempSensor1.CombineLatest(_tempSensor2, (t1, t2) => (t1, t2))
            .CombineLatest(_humiditySensor, (tempPair, hum) => new EnvironmentalData
            {
                Temperature1 = tempPair.t1.Value,
                Temperature2 = tempPair.t2.Value,
                AverageTemperature = (tempPair.t1.Value + tempPair.t2.Value) / 2,
                Humidity = hum.Value,
                Timestamp = DateTime.UtcNow
            });

    /// <summary>
    /// سفارشات و پرداخت‌ها با Zip
    /// Orders and payments with Zip
    /// </summary>
    public IObservable<OrderPaymentPair> ZippedOrderPayments =>
        _orderEvents.Zip(_paymentEvents, (order, payment) => new OrderPaymentPair
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Amount = order.Amount,
            PaymentMethod = payment.Method,
            PaymentStatus = payment.Status,
            Timestamp = DateTime.UtcNow
        });

    /// <summary>
    /// آمار فعالیت ترکیبی کاربران
    /// Combined user activity statistics
    /// </summary>
    public IObservable<UserActivityCombined> CombinedUserActivityStats =>
        _userEvents.Where(e => e.Action == "LOGIN").CombineLatest(
            _userEvents.Where(e => e.Action == "PURCHASE"),
            (login, purchase) =>
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

    /// <summary>
    /// متریک های سیستم با Concat
    /// System metrics with Concat
    /// </summary>
    public IObservable<SystemMetric> ConcatenatedSystemMetrics =>
        Observable.Concat(
            _systemMetrics.Where(m => m.Type == "CPU").Take(3),
            _systemMetrics.Where(m => m.Type == "MEMORY").Take(3),
            _systemMetrics.Where(m => m.Type == "DISK").Take(3)
        );

    /// <summary>
    /// رویدادهای سیستم ترکیبی با Merge
    /// Merged system events
    /// </summary>
    public IObservable<SystemEventCombined> MergedSystemEvents =>
        Observable.Merge(
            _systemMetrics.Where(m => m.Value > 90).Select(m => new SystemEventCombined
            {
                Type = "ALERT",
                Source = m.Type,
                Message = $"{m.Type} usage is high: {m.Value}%",
                Severity = "HIGH",
                Timestamp = m.Timestamp
            }),
            _systemMetrics.Where(m => m.Value <= 90).Select(m => new SystemEventCombined
            {
                Type = "INFO",
                Source = m.Type,
                Message = $"{m.Type} usage: {m.Value}%",
                Severity = "NORMAL",
                Timestamp = m.Timestamp
            })
        );

    /// <summary>
    /// سنسورها با Amb
    /// Sensors with Amb
    /// </summary>
    public IObservable<TemperatureReading> AmbTemperatureReadings =>
        _tempSensor1.Amb(_tempSensor2);

    /// <summary>
    /// همبستگی داده‌ها با Join
    /// Data correlation with Join
    /// </summary>
    public IObservable<UserSystemCorrelation> JoinedUserSystemData =>
        _userEvents.Join(
            _systemMetrics,
            _ => Observable.Timer(TimeSpan.FromSeconds(5)),
            _ => Observable.Timer(TimeSpan.FromSeconds(5)),
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

    private void StartTemperatureSensor1()
    {
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
    }

    private void StartTemperatureSensor2()
    {
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
    }

    private void StartHumiditySensor()
    {
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
    }

    private void StartUserEvents()
    {
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var actions = new[] { "LOGIN", "VIEW_PAGE", "PURCHASE", "LOGOUT", "SEARCH" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(1.5))
            .Subscribe(_ =>
            {
                var userEvent = new UserEvent
                {
                    UserId = users[random.Next(users.Length)],
                    Action = actions[random.Next(actions.Length)],
                    Timestamp = DateTime.UtcNow
                };
                _userEvents.OnNext(userEvent);
            });
    }

    private void StartSystemMetrics()
    {
        var types = new[] { "CPU", "MEMORY", "DISK", "NETWORK" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ =>
            {
                var metric = new SystemMetric
                {
                    Type = types[random.Next(types.Length)],
                    Value = random.Next(0, 101),
                    Timestamp = DateTime.UtcNow
                };
                _systemMetrics.OnNext(metric);
            });
    }

    private void StartOrderEvents()
    {
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(5))
            .Subscribe(_ =>
            {
                var orderEvent = new OrderCreatedEvent
                {
                    OrderId = $"ORD-{random.Next(1000, 9999)}",
                    CustomerId = $"CUST-{random.Next(100, 999)}",
                    Amount = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
                    Timestamp = DateTime.UtcNow
                };
                _orderEvents.OnNext(orderEvent);
            });
    }

    private void StartPaymentEvents()
    {
        var methods = new[] { "CREDIT_CARD", "PAYPAL", "BANK_TRANSFER", "CRYPTO" };
        var statuses = new[] { "PENDING", "COMPLETED", "FAILED", "REFUNDED" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(6))
            .Subscribe(_ =>
            {
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
            });
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

