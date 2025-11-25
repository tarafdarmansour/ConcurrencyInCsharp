# Progress<T> in Asynchronous Programming - C# Example

This project demonstrates the usage of `Progress<T>` in C# asynchronous programming scenarios.

## What is Progress<T>?

`Progress<T>` is a class in .NET that provides a thread-safe way to report progress from asynchronous operations back to the UI thread or any other thread. It's particularly useful for:

- File downloads/uploads
- Data processing operations
- Long-running background tasks
- Any operation where you want to provide user feedback

## Key Features Demonstrated

### 1. Basic Progress Reporting

- Simple integer-based progress reporting (0-100%)
- Visual progress bar in console
- Thread-safe progress updates

### 2. Complex Progress with Custom Types

- Custom `OperationStatus` class for detailed progress information
- Multiple operations with different progress tracking
- Timestamped progress reports

### 3. Progress with Cancellation

- Integration with `CancellationToken`
- Graceful cancellation handling
- Progress reporting during cancellation

### 4. Multiple Progress Types

- Different progress types: `int`, `string`, and custom objects
- Multiple progress reporters for the same operation
- Detailed processing information with elapsed time

### 5. Progress Aggregation

- Aggregating progress from multiple concurrent operations
- Real-time overall progress calculation
- Thread-safe progress sharing between tasks

### 6. Parallel Operations Progress

- Individual progress tracking for parallel tasks
- Concurrent task execution with progress reporting
- Task identification and status tracking

### 7. Progress with Error Handling

- Progress reporting during error scenarios
- Error state communication through progress
- Graceful error handling with progress updates

### 8. Recursive Operations Progress

- Progress reporting in recursive algorithms
- Depth-based indentation for visual hierarchy
- Nested operation progress tracking

## How to Run

1. Make sure you have .NET 8.0 or later installed
2. Navigate to the project directory
3. Run the following commands:

```bash
dotnet restore
dotnet run
```

## Key Concepts Explained

### Thread Safety

`Progress<T>` automatically marshals progress reports to the thread that created it, typically the UI thread. This ensures thread-safe updates without manual synchronization.

### IProgress<T> Interface

The `IProgress<T>` interface allows for dependency injection and testing. You can easily mock progress reporting in unit tests.

### Cancellation Integration

Progress reporting works seamlessly with `CancellationToken`, allowing for responsive cancellation of long-running operations.

## Example Output

```
=== Progress<T> Examples ===

1. Basic Progress Example - File Download Simulation
=====================================================
Download Progress: 0%
[░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%
Download Progress: 1%
[░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 1%
...
Download Progress: 100%
[████████████████████████████████████████████████] 100%
Download completed!

4. Multiple Progress Types Example
==================================
Status: Starting processing of Document1.pdf
Percentage: 5%
Processing: Document1.pdf - 1/5 - 0.2s
Status: Starting processing of Image2.jpg
Percentage: 30%
Processing: Image2.jpg - 2/5 - 1.0s

5. Progress Aggregation Example
===============================
Overall Progress: 10% (Task 1: 10%, Task 2: 0%, Task 3: 0%)
Overall Progress: 15% (Task 1: 10%, Task 2: 15%, Task 3: 0%)
Overall Progress: 20% (Task 1: 20%, Task 2: 15%, Task 3: 0%)

6. Parallel Operations Progress Example
=======================================
Task 1: Data Processing - Starting - 0%
Task 2: File Compression - Starting - 0%
Task 3: Network Upload - Starting - 0%
Task 4: Database Sync - Starting - 0%
Task 1: Data Processing - In Progress - 25%
Task 2: File Compression - In Progress - 25%

7. Progress with Error Handling Example
=======================================
✅ File Download: Starting... (0%)
✅ File Download: Processing... (20%)
✅ File Download: Processing... (40%)
✅ File Download: Completed successfully (100%)
❌ Error in Data Validation: Simulated error in Data Validation

8. Recursive Operations Progress Example
========================================
Level 0: Scanning files - 0%
  Level 1: Scanning files - 0%
    Level 2: Scanning files - 0%
    Level 2: Processing data - 33%
```

## Best Practices

1. **Always check for null**: Use the null-conditional operator (`?.`) when reporting progress
2. **Use meaningful progress values**: Provide clear, incremental progress updates
3. **Handle cancellation**: Integrate with `CancellationToken` for responsive cancellation
4. **Consider performance**: Don't report progress too frequently for very fast operations
5. **Use custom types**: For complex scenarios, create custom progress types with additional context

## Use Cases

- File operations (download/upload)
- Data processing pipelines
- Database operations
- Image/video processing
- Machine learning model training
- Any long-running background task
