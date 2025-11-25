using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ارتباط با EventController API
/// Service for communicating with EventController API
/// </summary>
public class EventService
{
    private readonly Subject<UserAction> _userActionSubject = new();
    private readonly Subject<SystemEvent> _systemEventSubject = new();
    private readonly Subject<OrderEvent> _orderEventSubject = new();
    private readonly Subject<ButtonClick> _buttonClickSubject = new();

    /// <summary>
    /// شروع جریان رویدادهای کاربر
    /// Start user actions stream
    /// </summary>
    public void StartUserActionsStream()
    {
        StartSimulatedUserActions();
    }

    /// <summary>
    /// شروع جریان رویدادهای سیستم
    /// Start system events stream
    /// </summary>
    public void StartSystemEventsStream()
    {
        StartSimulatedSystemEvents();
    }

    /// <summary>
    /// شروع جریان رویدادهای سفارش
    /// Start order events stream
    /// </summary>
    public void StartOrderEventsStream()
    {
        StartSimulatedOrderEvents();
    }

    /// <summary>
    /// شبیه‌سازی کلیک دکمه
    /// Simulate button click
    /// </summary>
    public void SimulateButtonClick(string buttonId, string userId)
    {
        var click = new ButtonClick
        {
            ButtonId = buttonId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };
        _buttonClickSubject.OnNext(click);
    }

    /// <summary>
    /// Observable برای رویدادهای کاربر
    /// Observable for user actions
    /// </summary>
    public IObservable<UserAction> UserActions => _userActionSubject.AsObservable();

    /// <summary>
    /// Observable برای رویدادهای سیستم
    /// Observable for system events
    /// </summary>
    public IObservable<SystemEvent> SystemEvents => _systemEventSubject.AsObservable();

    /// <summary>
    /// Observable برای رویدادهای سفارش
    /// Observable for order events
    /// </summary>
    public IObservable<OrderEvent> OrderEvents => _orderEventSubject.AsObservable();

    /// <summary>
    /// Observable برای کلیک های دکمه (با debounce)
    /// Observable for button clicks (with debounce)
    /// </summary>
    public IObservable<ButtonClick> DebouncedButtonClicks =>
        _buttonClickSubject.Throttle(TimeSpan.FromMilliseconds(500));

    /// <summary>
    /// Observable برای رویدادهای ترکیبی
    /// Observable for combined events
    /// </summary>
    public IObservable<EventBase> CombinedEvents =>
        Observable.Merge(
            UserActions.Select(action => new EventBase
            {
                Type = "USER_ACTION",
                Description = $"{action.UserId} performed {action.ActionType}",
                Timestamp = action.Timestamp
            }),
            SystemEvents.Select(sysEvent => new EventBase
            {
                Type = "SYSTEM_EVENT",
                Description = $"{sysEvent.Type}: {sysEvent.Message}",
                Timestamp = sysEvent.Timestamp
            })
        );

    private void StartSimulatedUserActions()
    {
        var actions = new[] { "LOGIN", "LOGOUT", "VIEW_PAGE", "ADD_TO_CART", "PURCHASE" };
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var action = new UserAction
                {
                    UserId = users[random.Next(users.Length)],
                    ActionType = actions[random.Next(actions.Length)],
                    PageUrl = $"/page/{random.Next(1, 11)}",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = $"192.168.1.{random.Next(1, 255)}"
                };
                _userActionSubject.OnNext(action);
            });
    }

    private void StartSimulatedSystemEvents()
    {
        var types = new[] { "INFO", "WARN", "ERROR", "DEBUG" };
        var messages = new[]
        {
            "Cache cleared successfully",
            "Database connection timeout",
            "Memory usage above threshold",
            "API rate limit exceeded",
            "Background job completed",
            "Disk space running low"
        };

        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(5))
            .Subscribe(_ =>
            {
                var systemEvent = new SystemEvent
                {
                    Type = types[random.Next(types.Length)],
                    Message = messages[random.Next(messages.Length)],
                    Component = $"Component-{random.Next(1, 6)}",
                    Timestamp = DateTime.UtcNow
                };
                _systemEventSubject.OnNext(systemEvent);
            });
    }

    private void StartSimulatedOrderEvents()
    {
        var statuses = new[] { "CREATED", "PROCESSING", "SHIPPED", "DELIVERED", "CANCELLED" };
        var random = new Random();

        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ =>
            {
                var orderEvent = new OrderEvent
                {
                    OrderId = $"ORD-{random.Next(1000, 9999)}",
                    CustomerId = $"CUST-{random.Next(100, 999)}",
                    Status = statuses[random.Next(statuses.Length)],
                    Amount = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
                    Timestamp = DateTime.UtcNow
                };
                _orderEventSubject.OnNext(orderEvent);
            });
    }
}

// مدل های داده برای رویدادها
// Event data models
public record UserAction
{
    public string UserId { get; init; } = string.Empty;
    public string ActionType { get; init; } = string.Empty;
    public string PageUrl { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string IpAddress { get; init; } = string.Empty;
}

public record SystemEvent
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record OrderEvent
{
    public string OrderId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ButtonClick
{
    public string ButtonId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record EventBase
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

