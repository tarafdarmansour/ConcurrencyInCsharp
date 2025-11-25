# ValueTask Demo - Practical ValueTask Usage Project

This project demonstrates practical examples of using `ValueTask` in .NET 8.

## What is ValueTask?

`ValueTask` is a struct that works similarly to `Task` but has the following advantages:

1. **Reduced Memory Allocation**: When the result is already available (e.g., from Cache), there's no need to allocate memory for a Task
2. **Performance Improvement**: In scenarios with Cache or synchronous results, it's faster than Task
3. **Reduced GC Pressure**: Causes less Garbage Collection

## When to Use ValueTask?

✅ **Use when:**
- The result may already be in memory (Cache)
- Your method often completes synchronously
- Your method is called millions of times (hot path)

❌ **Don't use when:**
- An async operation must always be performed
- You need to await multiple ValueTasks (must convert to Task)

## Examples in the Project

### 1. Cache with ValueTask
Demonstrates using ValueTask in services that have Cache. When the result comes from Cache, it returns without memory allocation.

### 2. Performance Comparison
Compares the performance of Task and ValueTask in different scenarios.

### 3. Repository Pattern
Using ValueTask in the Repository pattern to reduce memory allocation.

### 4. API Service with Cache
Demonstrates usage in API services that have Cache.

### 5. Usage with CancellationToken
Shows how to use ValueTask with CancellationToken to cancel operations.

### 6. Usage in LINQ Operations
Demonstrates converting ValueTask to Task for use in LINQ operations and WhenAll.

### 7. Usage in IAsyncEnumerable
Demonstrates using ValueTask in IAsyncEnumerable for processing data streams.

## Running the Project

```bash
dotnet run
```

## Important Notes

- Only await `ValueTask` once
- If you need to await multiple ValueTasks, first convert them to `Task` (using `.AsTask()`)
- Use `ValueTask` in public APIs that may have Cache
- `ValueTask` can be used with `CancellationToken`
- For use in LINQ, you must convert ValueTask to Task
- `ValueTask` is very useful in IAsyncEnumerable for performance optimization

## Advanced Examples

### Converting ValueTask to Task
```csharp
var valueTask = service.GetDataAsync(id);
var task = valueTask.AsTask(); // For use in Task.WhenAll
```

### Usage in LINQ
```csharp
var tasks = ids.Select(id => service.GetDataAsync(id).AsTask());
var results = await Task.WhenAll(tasks);
```

### Usage with CancellationToken
```csharp
public async ValueTask<string> ReadAsync(string path, CancellationToken ct = default)
{
    await Task.Delay(100, ct);
    return "content";
}
```
