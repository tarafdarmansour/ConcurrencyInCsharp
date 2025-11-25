# Task.WhenAll Exception Behavior Example

This example demonstrates how `Task.WhenAll` behaves when some tasks raise exceptions.

## Key Points Demonstrated

1. **Task.WhenAll throws on first exception**: When any task in the collection throws an exception, `Task.WhenAll` will throw that exception immediately, potentially losing information about other exceptions.

2. **Exception aggregation**: In some cases, you might get an `AggregateException` that contains multiple inner exceptions.

3. **Individual task handling**: To preserve all exceptions and results, you should check each task individually after `Task.WhenAll` completes.

## Running the Example

```bash
dotnet run
```

## What You'll See

The program runs three examples:

1. **Basic exception behavior**: Shows how `Task.WhenAll` throws when any task fails
2. **Proper exception handling**: Demonstrates catching and examining exceptions
3. **Individual task handling**: Shows how to preserve all results and exceptions

## Important Behaviors

- `Task.WhenAll` waits for ALL tasks to complete, even if some fail
- The method throws the FIRST exception it encounters
- Other exceptions may be lost unless handled properly
- Individual task status checking preserves all exception information
- Successful tasks still complete and can be accessed via `task.Result`

## Best Practices

1. Always wrap `Task.WhenAll` in try-catch
2. Check individual task status after completion
3. Use `task.IsCompletedSuccessfully` and `task.IsFaulted` to determine outcome
4. Access `task.Exception` for detailed exception information
