using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TaskWhenAllExceptionExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Console.WriteLine("=== Task.WhenAll Exception Behavior Example ===\n");

            // // Example 1: Some tasks succeed, some throw exceptions
            // await DemonstrateTaskWhenAllWithExceptions();

            // Console.WriteLine("\n" + new string('=', 60) + "\n");

            // // Example 2: Handling exceptions properly
            await DemonstrateProperExceptionHandling();

            // Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 3: Using Task.WhenAll with individual exception handling
            // await DemonstrateIndividualTaskHandling();
        }

        static async Task DemonstrateTaskWhenAllWithExceptions()
        {
            Console.WriteLine("Example 1: Task.WhenAll with mixed results (some exceptions)");
            Console.WriteLine("------------------------------------------------------------");

            // Create tasks with different behaviors
            var tasks = new List<Task<string>>
            {
                Task.Run(async () => 
                {
                    await Task.Delay(1000);
                    return "Task 1: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(500);
                    throw new InvalidOperationException("Task 2: This task failed!");
                    return "This will never be reached";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(1500);
                    return "Task 3: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(200);
                    throw new ArgumentException("Task 4: Another failure!");
                    return "This will never be reached";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(800);
                    return "Task 5: Success";
                })
            };

            try
            {
                Console.WriteLine("Starting all tasks...");
                var results = await Task.WhenAll(tasks);
                
                // This line will never be reached because Task.WhenAll throws
                Console.WriteLine("All tasks completed successfully!");
                foreach (var result in results)
                {
                    Console.WriteLine($"Result: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task.WhenAll threw an exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                
                // Note: Task.WhenAll throws the FIRST exception it encounters
                // Other exceptions are lost unless we handle them differently
            }
        }

        static async Task DemonstrateProperExceptionHandling()
        {
            Console.WriteLine("Example 2: Proper exception handling with Task.WhenAll");
            Console.WriteLine("-----------------------------------------------------");

            var tasks = new List<Task<string>>
            {
                Task.Run(async () => 
                {
                    await Task.Delay(1000);
                    return "Task A: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(500);
                    throw new InvalidOperationException("Task B: Failed!");
                    return "This will never be reached";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(1500);
                    return "Task C: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(200);
                    throw new ArgumentException("Task D: Failed!");
                    return "This will never be reached";
                })
            };

            try
            {
                Console.WriteLine("Starting all tasks...");
                var results = await Task.WhenAll(tasks);
                Console.WriteLine("All tasks completed successfully!");
            }
            catch (AggregateException aggEx)
            {
                Console.WriteLine($"Caught AggregateException with {aggEx.InnerExceptions.Count} inner exceptions:");
                foreach (var innerEx in aggEx.InnerExceptions)
                {
                    Console.WriteLine($"  - {innerEx.GetType().Name}: {innerEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught single exception: {ex.GetType().Name}: {ex.Message}");
            }

            // Check individual task status
            Console.WriteLine("\nIndividual task statuses:");
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                Console.WriteLine($"Task {i + 1}: Status = {task.Status}");
                if (task.IsFaulted)
                {
                    Console.WriteLine($"  Exception: {task.Exception?.GetBaseException().Message}");
                }
                else if (task.IsCompletedSuccessfully)
                {
                    Console.WriteLine($"  Result: {task.Result}");
                }
            }
        }

        static async Task DemonstrateIndividualTaskHandling()
        {
            Console.WriteLine("Example 3: Individual task handling to preserve all exceptions");
            Console.WriteLine("------------------------------------------------------------");

            var tasks = new List<Task<string>>
            {
                Task.Run(async () => 
                {
                    await Task.Delay(1000);
                    return "Task X: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(500);
                    throw new InvalidOperationException("Task Y: Failed!");
                    return "This will never be reached";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(1500);
                    return "Task Z: Success";
                }),
                Task.Run(async () => 
                {
                    await Task.Delay(200);
                    throw new ArgumentException("Task W: Failed!");
                    return "This will never be reached";
                })
            };

            Console.WriteLine("Starting all tasks...");
            
            // Wait for all tasks to complete, but handle each individually
            await Task.WhenAll(tasks);

            Console.WriteLine("\nAll tasks have completed. Checking individual results:");
            
            var successfulResults = new List<string>();
            var exceptions = new List<Exception>();

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (task.IsCompletedSuccessfully)
                {
                    successfulResults.Add(task.Result);
                    Console.WriteLine($"✓ Task {i + 1}: {task.Result}");
                }
                else if (task.IsFaulted)
                {
                    var exception = task.Exception?.GetBaseException();
                    if (exception != null)
                    {
                        exceptions.Add(exception);
                        Console.WriteLine($"✗ Task {i + 1}: Failed with {exception.GetType().Name}: {exception.Message}");
                    }
                }
            }

            Console.WriteLine($"\nSummary:");
            Console.WriteLine($"  Successful tasks: {successfulResults.Count}");
            Console.WriteLine($"  Failed tasks: {exceptions.Count}");
            
            if (exceptions.Any())
            {
                Console.WriteLine("\nAll exceptions that occurred:");
                foreach (var ex in exceptions)
                {
                    Console.WriteLine($"  - {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
