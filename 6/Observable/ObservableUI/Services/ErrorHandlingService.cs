using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableUI.Services;

/// <summary>
/// سرویس ارتباط با ErrorHandlingController API
/// Service for communicating with ErrorHandlingController API
/// </summary>
public class ErrorHandlingService
{
    private readonly Subject<ApiRequest> _apiRequestSubject = new();
    private readonly Subject<DatabaseOperation> _dbOperationSubject = new();
    private readonly Subject<FileOperation> _fileOperationSubject = new();
    private readonly Random _random = new();

    /// <summary>
    /// شروع جریان درخواست های API با خطا
    /// Start API requests stream with errors
    /// </summary>
    public void StartApiRequestsWithErrors()
    {
        StartSimulatedApiRequests();
    }

    /// <summary>
    /// شروع جریان عملیات دیتابیس
    /// Start database operations stream
    /// </summary>
    public void StartDatabaseOperations()
    {
        StartSimulatedDbOperations();
    }

    /// <summary>
    /// شروع جریان عملیات فایل
    /// Start file operations stream
    /// </summary>
    public void StartFileOperations()
    {
        StartSimulatedFileOperations();
    }

    /// <summary>
    /// Observable برای درخواست های API با مدیریت خطای ساده
    /// Observable for API requests with simple error handling
    /// </summary>
    public IObservable<ApiResult> ApiRequestsWithCatch =>
        _apiRequestSubject
            .AsObservable()
            .Select(request => ProcessApiRequest(request))
            .Catch<ApiResult, Exception>(ex => Observable.Return(new ApiResult
            {
                RequestId = "ERROR",
                StatusCode = 500,
                Message = $"Error caught: {ex.Message}",
                Timestamp = DateTime.UtcNow
            }));

    /// <summary>
    /// Observable برای عملیات دیتابیس با fallback
    /// Observable for database operations with fallback
    /// </summary>
    public IObservable<DbResult> DbOperationsWithFallback =>
        Observable.Concat(
            _dbOperationSubject
                .AsObservable()
                .Select(op => ProcessDbOperationPrimary(op))
                .Catch<DbResult, Exception>(ex => Observable.Return(new DbResult
                {
                    OperationId = "PRIMARY_FAILED",
                    Success = false,
                    Message = $"Primary operation failed: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                })),
            _dbOperationSubject
                .AsObservable()
                .Select(op => ProcessDbOperationFallback(op))
        );

    /// <summary>
    /// Observable برای عملیات فایل با مدیریت خطای پیشرفته
    /// Observable for file operations with advanced error handling
    /// </summary>
    public IObservable<FileResult> FileOperationsAdvanced =>
        _fileOperationSubject
            .AsObservable()
            .Select(op => ProcessFileOperation(op))
            .Catch<FileResult, IOException>(ioEx => Observable.Return(new FileResult
            {
                OperationId = "IO_ERROR",
                Success = false,
                Message = $"IO Error: {ioEx.Message}",
                Timestamp = DateTime.UtcNow
            }))
            .Catch<FileResult, UnauthorizedAccessException>(authEx => Observable.Return(new FileResult
            {
                OperationId = "AUTH_ERROR",
                Success = false,
                Message = $"Access denied: {authEx.Message}",
                Timestamp = DateTime.UtcNow
            }))
            .Catch<FileResult, Exception>(ex => Observable.Return(new FileResult
            {
                OperationId = "UNKNOWN_ERROR",
                Success = false,
                Message = $"Unexpected error: {ex.Message}",
                Timestamp = DateTime.UtcNow
            }));

    /// <summary>
    /// Observable برای عملیات ترکیبی با زنجیره مدیریت خطا
    /// Observable for combined operations with chained error handling
    /// </summary>
    public IObservable<OperationResult> CombinedOperationsWithChain =>
        Observable.Merge(
            ApiRequestsWithCatch.Select(result => new OperationResult
            {
                Type = "API",
                Id = result.RequestId,
                Success = result.StatusCode < 400,
                Message = result.Message,
                Timestamp = result.Timestamp
            }),
            DbOperationsWithFallback.Select(result => new OperationResult
            {
                Type = "DATABASE",
                Id = result.OperationId,
                Success = result.Success,
                Message = result.Message,
                Timestamp = result.Timestamp
            })
        );

    private void StartSimulatedApiRequests()
    {
        var endpoints = new[] { "/api/users", "/api/products", "/api/orders", "/api/inventory" };
        var methods = new[] { "GET", "POST", "PUT", "DELETE" };

        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var request = new ApiRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    Method = methods[_random.Next(methods.Length)],
                    Endpoint = endpoints[_random.Next(endpoints.Length)],
                    Timestamp = DateTime.UtcNow
                };
                _apiRequestSubject.OnNext(request);
            });
    }

    private void StartSimulatedDbOperations()
    {
        var types = new[] { "SELECT", "INSERT", "UPDATE", "DELETE" };
        var tables = new[] { "Users", "Products", "Orders", "Inventory" };

        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var operation = new DatabaseOperation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = types[_random.Next(types.Length)],
                    Table = tables[_random.Next(tables.Length)],
                    Timestamp = DateTime.UtcNow
                };
                _dbOperationSubject.OnNext(operation);
            });
    }

    private void StartSimulatedFileOperations()
    {
        var operations = new[] { "Read", "Write", "Delete", "Copy" };
        var files = new[] { "config.json", "data.txt", "image.jpg", "document.pdf" };

        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ =>
            {
                var operation = new FileOperation
                {
                    Id = Guid.NewGuid().ToString(),
                    OperationType = operations[_random.Next(operations.Length)],
                    FilePath = $"/files/{files[_random.Next(files.Length)]}",
                    Timestamp = DateTime.UtcNow
                };
                _fileOperationSubject.OnNext(operation);
            });
    }

    private ApiResult ProcessApiRequest(ApiRequest request)
    {
        // Simulate processing with random failures
        if (_random.Next(100) < 15) // 15% failure rate
        {
            throw new HttpRequestException($"API call to {request.Endpoint} failed");
        }

        return new ApiResult
        {
            RequestId = request.Id,
            StatusCode = 200,
            Message = $"Successfully processed {request.Method} {request.Endpoint}",
            Timestamp = DateTime.UtcNow
        };
    }

    private DbResult ProcessDbOperationPrimary(DatabaseOperation operation)
    {
        // Simulate database operation with occasional failures
        if (_random.Next(100) < 10) // 10% failure rate
        {
            throw new InvalidOperationException($"Database operation {operation.Type} failed on {operation.Table}");
        }

        return new DbResult
        {
            OperationId = operation.Id,
            Success = true,
            Message = $"Successfully executed {operation.Type} on {operation.Table}",
            Timestamp = DateTime.UtcNow
        };
    }

    private DbResult ProcessDbOperationFallback(DatabaseOperation operation)
    {
        // Fallback always succeeds but with reduced functionality
        return new DbResult
        {
            OperationId = operation.Id + "_FALLBACK",
            Success = true,
            Message = $"Executed {operation.Type} using fallback method (reduced functionality)",
            Timestamp = DateTime.UtcNow
        };
    }

    private FileResult ProcessFileOperation(FileOperation operation)
    {
        // Simulate different types of file operation errors
        var errorType = _random.Next(10);
        if (errorType < 2)
        {
            throw new IOException($"File {operation.FilePath} not found");
        }
        else if (errorType < 3)
        {
            throw new UnauthorizedAccessException($"Access denied to {operation.FilePath}");
        }
        else if (errorType < 4)
        {
            throw new InvalidOperationException($"Invalid file operation: {operation.OperationType}");
        }

        return new FileResult
        {
            OperationId = operation.Id,
            Success = true,
            Message = $"Successfully {operation.OperationType}d file {operation.FilePath}",
            Timestamp = DateTime.UtcNow
        };
    }
}

// مدل های داده برای مدیریت خطا
// Error handling data models
public record ApiRequest
{
    public string Id { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record ApiResult
{
    public string RequestId { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record DatabaseOperation
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Table { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record DbResult
{
    public string OperationId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record FileOperation
{
    public string Id { get; init; } = string.Empty;
    public string OperationType { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record FileResult
{
    public string OperationId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record OperationResult
{
    public string Type { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

