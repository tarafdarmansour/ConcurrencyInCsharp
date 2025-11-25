using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ObservableWebApi.Controllers;

/// <summary>
/// کنترلر نمایش مثال های مدیریت خطا در observables
/// Controller demonstrating error handling examples in observables
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ErrorHandlingController : ControllerBase
{
    private static readonly Subject<ApiRequest> _apiRequestSubject = new();
    private static readonly Subject<DatabaseOperation> _dbOperationSubject = new();
    private static readonly Subject<FileOperation> _fileOperationSubject = new();
    private static readonly Random _random = new();

    static ErrorHandlingController()
    {
        // شبیه‌سازی درخواست های API (بعضی با خطا)
        // Simulate API requests (some with errors)
        Observable.Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var request = GenerateApiRequest();
                _apiRequestSubject.OnNext(request);

                // Simulate random errors
                if (_random.Next(10) < 2) // 20% chance of error
                {
                    _apiRequestSubject.OnError(new HttpRequestException("API request failed"));
                }
            });

        // شبیه‌سازی عملیات دیتابیس
        // Simulate database operations
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                var operation = GenerateDbOperation();
                _dbOperationSubject.OnNext(operation);
            });

        // شبیه‌سازی عملیات فایل
        // Simulate file operations
        Observable.Interval(TimeSpan.FromSeconds(4))
            .Subscribe(_ =>
            {
                var operation = GenerateFileOperation();
                _fileOperationSubject.OnNext(operation);
            });
    }

    /// <summary>
    /// دریافت درخواست های API با مدیریت خطا ساده (Catch)
    /// Get API requests with simple error handling (Catch operator)
    /// </summary>
    [HttpGet("api-requests/catch")]
    public IObservable<ApiResult> GetApiRequestsWithCatch()
    {
        return _apiRequestSubject
            .AsObservable()
            .Select(request => ProcessApiRequest(request))
            .Catch<ApiResult, Exception>(ex => Observable.Return(new ApiResult
            {
                RequestId = "ERROR",
                StatusCode = 500,
                Message = $"Error caught: {ex.Message}",
                Timestamp = DateTime.UtcNow
            }));
    }

    /// <summary>
    /// دریافت درخواست های API با retry (تا 3 بار تلاش مجدد)
    /// Get API requests with retry (up to 3 attempts)
    /// </summary>
    [HttpGet("api-requests/retry")]
    public IObservable<ApiResult> GetApiRequestsWithRetry()
    {
        return _apiRequestSubject
            .AsObservable()
            .Select(request => ProcessApiRequestWithRetry(request, 3))
            .Catch<ApiResult, Exception>(ex => Observable.Return(new ApiResult
            {
                RequestId = "FINAL_ERROR",
                StatusCode = 500,
                Message = $"All retries failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            }));
    }

    /// <summary>
    /// دریافت عملیات دیتابیس با fallback
    /// Get database operations with fallback (OnErrorResumeNext)
    /// </summary>
    [HttpGet("db-operations/fallback")]
    public IObservable<DbResult> GetDbOperationsWithFallback()
    {
        var primaryOperations = _dbOperationSubject
            .AsObservable()
            .Select(op => ProcessDbOperationPrimary(op));

        var fallbackOperations = _dbOperationSubject
            .AsObservable()
            .Select(op => ProcessDbOperationFallback(op));

        return primaryOperations.OnErrorResumeNext(fallbackOperations);
    }

    /// <summary>
    /// دریافت عملیات فایل با مدیریت خطای پیشرفته
    /// Get file operations with advanced error handling
    /// </summary>
    [HttpGet("file-operations/advanced")]
    public IObservable<FileResult> GetFileOperationsAdvanced()
    {
        return _fileOperationSubject
            .AsObservable()
            .Select(op => ProcessFileOperation(op))
            .Catch<FileResult, IOException>(ioEx =>
                Observable.Return(new FileResult
                {
                    OperationId = "IO_ERROR",
                    Success = false,
                    Message = $"IO Error: {ioEx.Message}",
                    Timestamp = DateTime.UtcNow
                }))
            .Catch<FileResult, UnauthorizedAccessException>(authEx =>
                Observable.Return(new FileResult
                {
                    OperationId = "AUTH_ERROR",
                    Success = false,
                    Message = $"Access denied: {authEx.Message}",
                    Timestamp = DateTime.UtcNow
                }))
            .Catch<FileResult, Exception>(ex =>
                Observable.Return(new FileResult
                {
                    OperationId = "UNKNOWN_ERROR",
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                }));
    }

    /// <summary>
    /// دریافت عملیات ترکیبی با مدیریت خطای زنجیره ای
    /// Get combined operations with chained error handling
    /// </summary>
    [HttpGet("combined/chain")]
    public IObservable<OperationResult> GetCombinedOperationsWithChain()
    {
        var apiResults = _apiRequestSubject
            .AsObservable()
            .Select(req => ProcessApiRequest(req))
            .Select(result => new OperationResult
            {
                Type = "API",
                Id = result.RequestId,
                Success = result.StatusCode < 400,
                Message = result.Message,
                Timestamp = result.Timestamp
            })
            .Catch<OperationResult, Exception>(ex => Observable.Return(new OperationResult
            {
                Type = "API_ERROR",
                Id = "API_FAIL",
                Success = false,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            }));

        var dbResults = _dbOperationSubject
            .AsObservable()
            .Select(op => ProcessDbOperationPrimary(op))
            .Select(result => new OperationResult
            {
                Type = "DATABASE",
                Id = result.OperationId,
                Success = result.Success,
                Message = result.Message,
                Timestamp = result.Timestamp
            })
            .Catch<OperationResult, Exception>(ex => Observable.Return(new OperationResult
            {
                Type = "DB_ERROR",
                Id = "DB_FAIL",
                Success = false,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            }));

        return Observable.Merge(apiResults, dbResults);
    }

    /// <summary>
    /// دریافت عملیات با timeout و مدیریت خطا
    /// Get operations with timeout and error handling
    /// </summary>
    [HttpGet("operations/timeout")]
    public IObservable<TimedOperationResult> GetOperationsWithTimeout()
    {
        return _apiRequestSubject
            .AsObservable()
            .Select(req => ProcessApiRequest(req))
            .Timeout(TimeSpan.FromSeconds(5)) // 5 second timeout
            .Select(result => new TimedOperationResult
            {
                Id = result.RequestId,
                Success = result.StatusCode < 400,
                Message = result.Message,
                Duration = TimeSpan.Zero, // Would need more complex tracking
                Timestamp = result.Timestamp
            })
            .Catch<TimedOperationResult, TimeoutException>(timeoutEx =>
                Observable.Return(new TimedOperationResult
                {
                    Id = "TIMEOUT",
                    Success = false,
                    Message = "Operation timed out",
                    Duration = TimeSpan.FromSeconds(5),
                    Timestamp = DateTime.UtcNow
                }))
            .Catch<TimedOperationResult, Exception>(ex =>
                Observable.Return(new TimedOperationResult
                {
                    Id = "ERROR",
                    Success = false,
                    Message = ex.Message,
                    Duration = TimeSpan.Zero,
                    Timestamp = DateTime.UtcNow
                }));
    }

    // متدهای پردازش شبیه‌سازی شده با خطا
    // Simulated processing methods with errors
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

    private ApiResult ProcessApiRequestWithRetry(ApiRequest request, int maxRetries)
    {
        var attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                return ProcessApiRequest(request);
            }
            catch (Exception)
            {
                attempt++;
                if (attempt >= maxRetries)
                    throw;
                Thread.Sleep(100 * attempt); // Exponential backoff
            }
        }
        throw new Exception("Max retries exceeded");
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

    private static ApiRequest GenerateApiRequest()
    {
        var endpoints = new[] { "/api/users", "/api/products", "/api/orders", "/api/inventory" };
        var methods = new[] { "GET", "POST", "PUT", "DELETE" };

        return new ApiRequest
        {
            Id = Guid.NewGuid().ToString(),
            Method = methods[_random.Next(methods.Length)],
            Endpoint = endpoints[_random.Next(endpoints.Length)],
            Timestamp = DateTime.UtcNow
        };
    }

    private static DatabaseOperation GenerateDbOperation()
    {
        var types = new[] { "SELECT", "INSERT", "UPDATE", "DELETE" };
        var tables = new[] { "Users", "Products", "Orders", "Inventory" };

        return new DatabaseOperation
        {
            Id = Guid.NewGuid().ToString(),
            Type = types[_random.Next(types.Length)],
            Table = tables[_random.Next(tables.Length)],
            Timestamp = DateTime.UtcNow
        };
    }

    private static FileOperation GenerateFileOperation()
    {
        var operations = new[] { "Read", "Write", "Delete", "Copy" };
        var files = new[] { "config.json", "data.txt", "image.jpg", "document.pdf" };

        return new FileOperation
        {
            Id = Guid.NewGuid().ToString(),
            OperationType = operations[_random.Next(operations.Length)],
            FilePath = $"/files/{files[_random.Next(files.Length)]}",
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

public record TimedOperationResult
{
    public string Id { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTime Timestamp { get; init; }
}

