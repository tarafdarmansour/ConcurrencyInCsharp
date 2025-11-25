using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressExample
{
    /// <summary>
    /// Demonstrates the usage of Progress<T> in asynchronous programming
    /// Progress<T> provides a thread-safe way to report progress from async operations
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("=== Progress<T> Examples ===\n");

            // Example 1: Basic Progress Reporting
            //await BasicProgressExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 2: Complex Progress with Multiple Operations
            //await ComplexProgressExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 3: Progress with Cancellation
            //await ProgressWithCancellationExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 4: Multiple Progress Types
            await MultipleProgressTypesExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 5: Progress Aggregation from Multiple Operations
            //await ProgressAggregationExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 6: Progress in Parallel Operations
            //await ParallelOperationsProgressExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 7: Progress with Error Handling
            //await ProgressWithErrorHandlingExample();

            //Console.WriteLine("\n" + new string('-', 50) + "\n");

            // Example 8: Recursive Operations with Progress
            //await RecursiveOperationsProgressExample();

            //Console.WriteLine("\nPress any key to exit...");
            //Console.ReadKey();
        }

        /// <summary>
        /// Basic example showing how to use Progress<T> for a simple async operation
        /// </summary>
        static async Task BasicProgressExample()
        {
            Console.WriteLine("1. Basic Progress Example - File Download Simulation");
            Console.WriteLine("=====================================================");

            // Create a Progress<T> instance that reports progress to the UI thread
            var progress = new Progress<int>(percent =>
            {
                // This callback runs on the UI thread (or the thread that created the Progress)
                Console.Clear();
                Console.WriteLine($"Download Progress: {percent}%");
                
                // Visual progress bar
                var bar = new string('█', percent / 2) + new string('░', 50 - percent / 2);
                Console.WriteLine($"[{bar}] {percent}%");
            });

            // Simulate a file download operation
            await SimulateFileDownloadAsync(progress);
        }

        /// <summary>
        /// Complex example showing progress reporting for multiple operations
        /// </summary>
        static async Task ComplexProgressExample()
        {
            Console.WriteLine("2. Complex Progress Example - Multiple Operations");
            Console.WriteLine("=================================================");

            // Create a progress reporter for operation status
            var progress = new Progress<OperationStatus>(status =>
            {

                Console.WriteLine($"[{status.Timestamp:HH:mm:ss}] {status.Operation}: {status.Message} ({status.Percentage}%)");
            });

            // Simulate multiple operations with progress reporting
            await SimulateMultipleOperationsAsync(progress);
        }

        /// <summary>
        /// Example showing progress reporting with cancellation support
        /// </summary>
        static async Task ProgressWithCancellationExample()
        {
            Console.WriteLine("3. Progress with Cancellation Example");
            Console.WriteLine("=====================================");

            using var cts = new CancellationTokenSource();
            
            // Create a progress reporter
            var progress = new Progress<int>(percent =>
            {
                Console.WriteLine($"Processing: {percent}%");
                if (percent >= 50)
                {
                    Console.WriteLine("Cancelling operation at 50%...");
                    cts.Cancel();
                }
            });

            try
            {
                await SimulateCancellableOperationAsync(progress, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled successfully.");
            }
        }

        /// <summary>
        /// Simulates a file download with progress reporting
        /// </summary>
        static async Task SimulateFileDownloadAsync(IProgress<int> progress)
        {
            const int totalSteps = 100;
            
            for (int i = 0; i <= totalSteps; i++)
            {
                // Simulate work
                await Task.Delay(50);
                
                // Report progress (this is thread-safe)
                progress?.Report(i);
            }
            
            Console.WriteLine("Download completed!");
        }

        /// <summary>
        /// Simulates multiple operations with detailed progress reporting
        /// </summary>
        static async Task SimulateMultipleOperationsAsync(IProgress<OperationStatus> progress)
        {
            // Operation 1: Data Processing
            progress?.Report(new OperationStatus("Data Processing", "Starting data validation", 0));
            await Task.Delay(500);
            
            for (int i = 1; i <= 3; i++)
            {
                progress?.Report(new OperationStatus("Data Processing", $"Processing batch {i}/3", i * 33));
                await Task.Delay(300);
            }
            
            progress?.Report(new OperationStatus("Data Processing", "Data validation completed", 100));

            // Operation 2: File Upload
            progress?.Report(new OperationStatus("File Upload", "Preparing files for upload", 0));
            await Task.Delay(200);
            
            for (int i = 1; i <= 5; i++)
            {
                progress?.Report(new OperationStatus("File Upload", $"Uploading file {i}/5", i * 20));
                await Task.Delay(400);
            }
            
            progress?.Report(new OperationStatus("File Upload", "All files uploaded successfully", 100));

            // Operation 3: Database Update
            progress?.Report(new OperationStatus("Database Update", "Connecting to database", 0));
            await Task.Delay(300);
            
            for (int i = 1; i <= 4; i++)
            {
                progress?.Report(new OperationStatus("Database Update", $"Updating table {i}/4", i * 25));
                await Task.Delay(250);
            }
            
            progress?.Report(new OperationStatus("Database Update", "Database update completed", 100));
        }

        /// <summary>
        /// Simulates an operation that can be cancelled
        /// </summary>
        static async Task SimulateCancellableOperationAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            const int totalSteps = 100;
            
            for (int i = 0; i <= totalSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                // Simulate work
                await Task.Delay(100, cancellationToken);
                
                // Report progress
                progress?.Report(i);
            }
        }

        /// <summary>
        /// Example showing different types of progress reporting
        /// </summary>
        static async Task MultipleProgressTypesExample()
        {
            Console.WriteLine("4. Multiple Progress Types Example");
            Console.WriteLine("==================================");

            // Integer progress for percentage
            var intProgress = new Progress<int>(percent =>
            {
                Console.WriteLine($"Percentage: {percent}%");
            });

            // String progress for status messages
            var stringProgress = new Progress<string>(message =>
            {
                Console.WriteLine($"Status: {message}");
            });

            // Custom object progress for detailed information
            var detailedProgress = new Progress<ProcessingDetails>(details =>
            {
                Console.WriteLine($"Processing: {details.ItemName} - {details.CurrentStep}/{details.TotalSteps} - {details.ElapsedTime.TotalSeconds:F1}s");
            });

            // Simulate operation with multiple progress types
            await SimulateMultiTypeProgressAsync(intProgress, stringProgress, detailedProgress);
        }

        /// <summary>
        /// Example showing progress aggregation from multiple concurrent operations
        /// </summary>
        static async Task ProgressAggregationExample()
        {
            Console.WriteLine("5. Progress Aggregation Example");
            Console.WriteLine("===============================");

            var aggregatedProgress = new Progress<AggregatedProgress>(progress =>
            {
                Console.WriteLine($"Overall Progress: {progress.OverallPercentage}% " +
                                $"(Task 1: {progress.Task1Progress}%, Task 2: {progress.Task2Progress}%, Task 3: {progress.Task3Progress}%)");
            });

            // Run multiple operations concurrently and aggregate their progress
            await RunConcurrentOperationsWithAggregationAsync(aggregatedProgress);
        }

        /// <summary>
        /// Example showing progress reporting in parallel operations
        /// </summary>
        static async Task ParallelOperationsProgressExample()
        {
            Console.WriteLine("6. Parallel Operations Progress Example");
            Console.WriteLine("=======================================");

            var parallelProgress = new Progress<ParallelTaskProgress>(progress =>
            {
                Console.WriteLine($"Task {progress.TaskId}: {progress.Status} - {progress.Progress}%");
            });

            // Run multiple parallel operations with individual progress tracking
            await RunParallelOperationsAsync(parallelProgress);
        }

        /// <summary>
        /// Example showing progress reporting with error handling
        /// </summary>
        static async Task ProgressWithErrorHandlingExample()
        {
            Console.WriteLine("7. Progress with Error Handling Example");
            Console.WriteLine("=======================================");

            var errorProgress = new Progress<ErrorHandlingProgress>(progress =>
            {
                if (progress.HasError)
                {
                    Console.WriteLine($"❌ Error in {progress.Operation}: {progress.ErrorMessage}");
                }
                else
                {
                    Console.WriteLine($"✅ {progress.Operation}: {progress.Message} ({progress.Percentage}%)");
                }
            });

            // Simulate operations that might fail
            await SimulateOperationsWithErrorsAsync(errorProgress);
        }

        /// <summary>
        /// Example showing progress reporting in recursive operations
        /// </summary>
        static async Task RecursiveOperationsProgressExample()
        {
            Console.WriteLine("8. Recursive Operations Progress Example");
            Console.WriteLine("========================================");

            var recursiveProgress = new Progress<RecursiveProgress>(progress =>
            {
                var indent = new string(' ', progress.Depth * 2);
                Console.WriteLine($"{indent}Level {progress.Depth}: {progress.Operation} - {progress.Progress}%");
            });

            // Simulate recursive directory processing
            await SimulateRecursiveOperationAsync(recursiveProgress, 0, 3);
        }

        /// <summary>
        /// Simulates operation with multiple progress types
        /// </summary>
        static async Task SimulateMultiTypeProgressAsync(IProgress<int> intProgress, IProgress<string> stringProgress, IProgress<ProcessingDetails> detailedProgress)
        {
            var items = new[] { "Document1.pdf", "Image2.jpg", "Video3.mp4", "Archive4.zip" };
            var startTime = DateTime.Now;

            for (int i = 0; i < items.Length; i++)
            {
                stringProgress?.Report($"Starting processing of {items[i]}");
                
                for (int step = 1; step <= 5; step++)
                {
                    await Task.Delay(200);
                    
                    var percentage = (int)((i * 5 + step) * 100.0 / (items.Length * 5));
                    intProgress?.Report(percentage);
                    
                    detailedProgress?.Report(new ProcessingDetails
                    {
                        ItemName = items[i],
                        CurrentStep = step,
                        TotalSteps = 5,
                        ElapsedTime = DateTime.Now - startTime
                    });
                }
                
                stringProgress?.Report($"Completed processing of {items[i]}");
            }
        }

        /// <summary>
        /// Runs concurrent operations and aggregates their progress
        /// </summary>
        static async Task RunConcurrentOperationsWithAggregationAsync(IProgress<AggregatedProgress> progress)
        {
            var task1Progress = 0;
            var task2Progress = 0;
            var task3Progress = 0;

            var task1 = Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    await Task.Delay(300);
                    task1Progress = i;
                    progress?.Report(new AggregatedProgress
                    {
                        Task1Progress = task1Progress,
                        Task2Progress = task2Progress,
                        Task3Progress = task3Progress,
                        OverallPercentage = (task1Progress + task2Progress + task3Progress) / 3
                    });
                }
            });

            var task2 = Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 15)
                {
                    await Task.Delay(250);
                    task2Progress = i;
                    progress?.Report(new AggregatedProgress
                    {
                        Task1Progress = task1Progress,
                        Task2Progress = task2Progress,
                        Task3Progress = task3Progress,
                        OverallPercentage = (task1Progress + task2Progress + task3Progress) / 3
                    });
                }
            });

            var task3 = Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 20)
                {
                    await Task.Delay(200);
                    task3Progress = i;
                    progress?.Report(new AggregatedProgress
                    {
                        Task1Progress = task1Progress,
                        Task2Progress = task2Progress,
                        Task3Progress = task3Progress,
                        OverallPercentage = (task1Progress + task2Progress + task3Progress) / 3
                    });
                }
            });

            await Task.WhenAll(task1, task2, task3);
        }

        /// <summary>
        /// Runs parallel operations with individual progress tracking
        /// </summary>
        static async Task RunParallelOperationsAsync(IProgress<ParallelTaskProgress> progress)
        {
            var tasks = new[]
            {
                SimulateParallelTaskAsync(1, "Data Processing", progress),
                SimulateParallelTaskAsync(2, "File Compression", progress),
                SimulateParallelTaskAsync(3, "Network Upload", progress),
                SimulateParallelTaskAsync(4, "Database Sync", progress)
            };

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Simulates a single parallel task with progress reporting
        /// </summary>
        static async Task SimulateParallelTaskAsync(int taskId, string taskName, IProgress<ParallelTaskProgress> progress)
        {
            progress?.Report(new ParallelTaskProgress { TaskId = taskId, Status = $"{taskName} - Starting", Progress = 0 });

            for (int i = 0; i <= 100; i += 25)
            {
                await Task.Delay(400);
                progress?.Report(new ParallelTaskProgress { TaskId = taskId, Status = $"{taskName} - In Progress", Progress = i });
            }

            progress?.Report(new ParallelTaskProgress { TaskId = taskId, Status = $"{taskName} - Completed", Progress = 100 });
        }

        /// <summary>
        /// Simulates operations that might fail with error handling
        /// </summary>
        static async Task SimulateOperationsWithErrorsAsync(IProgress<ErrorHandlingProgress> progress)
        {
            var operations = new[]
            {
                ("File Download", 1000, false),
                ("Data Validation", 800, true),  // This one will fail
                ("Database Update", 1200, false),
                ("Cache Refresh", 600, false)
            };

            foreach (var (operation, delay, shouldFail) in operations)
            {
                try
                {
                    progress?.Report(new ErrorHandlingProgress
                    {
                        Operation = operation,
                        Message = "Starting...",
                        Percentage = 0,
                        HasError = false
                    });

                    for (int i = 0; i <= 100; i += 20)
                    {
                        await Task.Delay(delay / 5);
                        
                        if (shouldFail && i == 60)
                        {
                            throw new InvalidOperationException($"Simulated error in {operation}");
                        }

                        progress?.Report(new ErrorHandlingProgress
                        {
                            Operation = operation,
                            Message = $"Processing...",
                            Percentage = i,
                            HasError = false
                        });
                    }

                    progress?.Report(new ErrorHandlingProgress
                    {
                        Operation = operation,
                        Message = "Completed successfully",
                        Percentage = 100,
                        HasError = false
                    });
                }
                catch (Exception ex)
                {
                    progress?.Report(new ErrorHandlingProgress
                    {
                        Operation = operation,
                        ErrorMessage = ex.Message,
                        Percentage = 0,
                        HasError = true
                    });
                }
            }
        }

        /// <summary>
        /// Simulates recursive operations with progress reporting
        /// </summary>
        static async Task SimulateRecursiveOperationAsync(IProgress<RecursiveProgress> progress, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth)
                return;

            var operations = new[] { "Scanning files", "Processing data", "Updating metadata" };

            foreach (var operation in operations)
            {
                progress?.Report(new RecursiveProgress
                {
                    Depth = currentDepth,
                    Operation = operation,
                    Progress = 0
                });

                for (int i = 0; i <= 100; i += 33)
                {
                    await Task.Delay(200);
                    progress?.Report(new RecursiveProgress
                    {
                        Depth = currentDepth,
                        Operation = operation,
                        Progress = i
                    });
                }

                // Recursive call
                await SimulateRecursiveOperationAsync(progress, currentDepth + 1, maxDepth);
            }
        }
    }

    /// <summary>
    /// Represents the status of an operation for progress reporting
    /// </summary>
    public class OperationStatus
    {
        public string Operation { get; }
        public string Message { get; }
        public int Percentage { get; }
        public DateTime Timestamp { get; }

        public OperationStatus(string operation, string message, int percentage)
        {
            Operation = operation;
            Message = message;
            Percentage = percentage;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents detailed processing information for progress reporting
    /// </summary>
    public class ProcessingDetails
    {
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public TimeSpan ElapsedTime { get; set; }
    }

    /// <summary>
    /// Represents aggregated progress from multiple concurrent operations
    /// </summary>
    public class AggregatedProgress
    {
        public int Task1Progress { get; set; }
        public int Task2Progress { get; set; }
        public int Task3Progress { get; set; }
        public int OverallPercentage { get; set; }
    }

    /// <summary>
    /// Represents progress for parallel task operations
    /// </summary>
    public class ParallelTaskProgress
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
    }

    /// <summary>
    /// Represents progress with error handling capabilities
    /// </summary>
    public class ErrorHandlingProgress
    {
        public string Operation { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public bool HasError { get; set; }
    }

    /// <summary>
    /// Represents progress for recursive operations
    /// </summary>
    public class RecursiveProgress
    {
        public int Depth { get; set; }
        public string Operation { get; set; } = string.Empty;
        public int Progress { get; set; }
    }
}
