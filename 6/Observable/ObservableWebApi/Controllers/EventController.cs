using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableWebApi.Controllers;

/// <summary>
/// کنترلر نمایش مثال های مدیریت رویدادها به صورت واکنشی
/// Controller demonstrating reactive event handling examples
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private static readonly Subject<UserAction> _userActionSubject = new();
    private static readonly Subject<SystemEvent> _systemEventSubject = new();
    private static readonly Subject<OrderEvent> _orderEventSubject = new();
    private static readonly Subject<ButtonClick> _buttonClickSubject = new();

    static EventController()
    {
        // شبیه‌سازی رویدادهای کاربر
        // Simulate user actions
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var action = GenerateUserAction();
                _userActionSubject.OnNext(action);
            });

        // شبیه‌سازی رویدادهای سیستم
        // Simulate system events
        Observable.Interval(TimeSpan.FromSeconds(5))
            .Subscribe(_ =>
            {
                var systemEvent = GenerateSystemEvent();
                _systemEventSubject.OnNext(systemEvent);
            });

        // شبیه‌سازی رویدادهای سفارش
        // Simulate order events
        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ =>
            {
                var orderEvent = GenerateOrderEvent();
                _orderEventSubject.OnNext(orderEvent);
            });
    }

    /// <summary>
    /// دریافت جریان رویدادهای کاربر
    /// Get user action events stream
    /// </summary>
    [HttpGet("user-actions")]
    public IObservable<UserAction> GetUserActions()
    {
        return _userActionSubject.AsObservable();
    }

    /// <summary>
    /// دریافت رویدادهای سیستم با فیلتر خطاها
    /// Get system events filtered for errors only
    /// </summary>
    [HttpGet("system-errors")]
    public IObservable<SystemEvent> GetSystemErrors()
    {
        return _systemEventSubject
            .AsObservable()
            .Where(e => e.Type == "ERROR");
    }

    /// <summary>
    /// دریافت رویدادهای سفارش با تبدیل به خلاصه
    /// Get order events transformed to summary
    /// </summary>
    [HttpGet("order-summaries")]
    public IObservable<OrderSummary> GetOrderSummaries()
    {
        return _orderEventSubject
            .AsObservable()
            .Select(order => new OrderSummary
            {
                OrderId = order.OrderId,
                Status = order.Status,
                Amount = order.Amount,
                Timestamp = order.Timestamp
            });
    }

    /// <summary>
    /// دریافت آمار رویدادهای کاربر در بازه زمانی
    /// Get user action statistics over time window
    /// </summary>
    [HttpGet("user-stats/{windowSeconds}")]
    public IObservable<UserStats> GetUserStats(int windowSeconds)
    {
        return _userActionSubject
            .AsObservable()
            .Buffer(TimeSpan.FromSeconds(windowSeconds))
            .Select(actions => actions
                .GroupBy(action => action.ActionType)
                .Select(group => new UserStats
                {
                    ActionType = group.Key,
                    Count = group.Count(),
                    WindowSeconds = windowSeconds
                })
            )
            .SelectMany(stats => stats);
    }

    /// <summary>
    /// شبیه‌سازی کلیک دکمه (برای تست)
    /// Simulate button click (for testing)
    /// </summary>
    [HttpPost("button-click")]
    public IActionResult SimulateButtonClick([FromBody] ButtonClick click)
    {
        _buttonClickSubject.OnNext(click);
        return Ok(new { Message = "Button click registered", Click = click });
    }

    /// <summary>
    /// دریافت جریان کلیک های دکمه با debounce
    /// Get button click stream with debounce (prevents rapid clicks)
    /// </summary>
    [HttpGet("button-clicks/debounced")]
    public IObservable<ButtonClick> GetDebouncedButtonClicks()
    {
        return _buttonClickSubject
            .AsObservable()
            .Throttle(TimeSpan.FromMilliseconds(500)); // Debounce for 500ms
    }

    /// <summary>
    /// دریافت رویدادهای ترکیبی (کاربر + سیستم)
    /// Get combined events (user + system)
    /// </summary>
    [HttpGet("combined-events")]
    public IObservable<EventBase> GetCombinedEvents()
    {
        var userEvents = _userActionSubject
            .AsObservable()
            .Select(action => new EventBase
            {
                Type = "USER_ACTION",
                Description = $"{action.UserId} performed {action.ActionType}",
                Timestamp = action.Timestamp
            });

        var systemEvents = _systemEventSubject
            .AsObservable()
            .Select(sysEvent => new EventBase
            {
                Type = "SYSTEM_EVENT",
                Description = $"{sysEvent.Type}: {sysEvent.Message}",
                Timestamp = sysEvent.Timestamp
            });

        return Observable.Merge(userEvents, systemEvents);
    }

    private static UserAction GenerateUserAction()
    {
        var actions = new[] { "LOGIN", "LOGOUT", "VIEW_PAGE", "ADD_TO_CART", "PURCHASE" };
        var users = new[] { "user1", "user2", "user3", "user4", "user5" };
        var random = new Random();

        return new UserAction
        {
            UserId = users[random.Next(users.Length)],
            ActionType = actions[random.Next(actions.Length)],
            PageUrl = $"/page/{random.Next(1, 11)}",
            Timestamp = DateTime.UtcNow,
            IpAddress = $"192.168.1.{random.Next(1, 255)}"
        };
    }

    private static SystemEvent GenerateSystemEvent()
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
        return new SystemEvent
        {
            Type = types[random.Next(types.Length)],
            Message = messages[random.Next(messages.Length)],
            Component = $"Component-{random.Next(1, 6)}",
            Timestamp = DateTime.UtcNow
        };
    }

    private static OrderEvent GenerateOrderEvent()
    {
        var statuses = new[] { "CREATED", "PROCESSING", "SHIPPED", "DELIVERED", "CANCELLED" };
        var random = new Random();

        return new OrderEvent
        {
            OrderId = $"ORD-{random.Next(1000, 9999)}",
            CustomerId = $"CUST-{random.Next(100, 999)}",
            Status = statuses[random.Next(statuses.Length)],
            Amount = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
            Timestamp = DateTime.UtcNow
        };
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

public record UserStats
{
    public string ActionType { get; init; } = string.Empty;
    public int Count { get; init; }
    public int WindowSeconds { get; init; }
}

public record OrderSummary
{
    public string OrderId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Timestamp { get; init; }
}

public record EventBase
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
