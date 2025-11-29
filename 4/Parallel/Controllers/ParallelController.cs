using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParallelProcessingDemo.Models;
using System.Threading.Tasks;
using System.Linq;

namespace ParallelProcessingDemo.Controllers;

public class ParallelController : Controller
{
    private readonly ILogger<ParallelController> _logger;

    public ParallelController(ILogger<ParallelController> _logger)
    {
        this._logger = _logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Parallel.ForEach Examples
    public async Task<IActionResult> ForEachDemo()
    {
        var model = new ParallelDemoViewModel
        {
            Title = "Parallel.ForEach Demonstration",
            Description = "Processing multiple files concurrently using Parallel.ForEach"
        };

        var stopwatch = Stopwatch.StartNew();

        // Simulate file processing tasks
        var files = GenerateSampleFiles(100);
        var processedFiles = new ConcurrentBag<FileProcessingResult>();

        Parallel.ForEach(files, file =>
        {
            var result = ProcessFile(file);
            processedFiles.Add(result);
        });

        stopwatch.Stop();

        model.ParallelResults = processedFiles.ToList();
        model.ExecutionTime = stopwatch.ElapsedMilliseconds;
        model.ParallelMethod = "Parallel.ForEach";
        model.SequentialExecutionTime = processedFiles.Sum(p => p.ProcessingTime);

        return View("DemoResult", model);
    }

    // AsParallel().Aggregate Examples
    public async Task<IActionResult> AggregateDemo()
    {
        var model = new ParallelDemoViewModel
        {
            Title = "AsParallel().Aggregate Demonstration",
            Description = "Calculating statistics from large dataset using parallel aggregation"
        };

        var stopwatch = Stopwatch.StartNew();

        // Generate large dataset
        var numbers = GenerateLargeNumberSet(1000000);

        // Parallel aggregation
        var stats = numbers.AsParallel().Aggregate(
            () => new NumberStats(),
            (acc, num) => acc.Add(num),
            (left, right) => left.Combine(right),
            final => final
        );

        stopwatch.Stop();

        model.AggregateStats = stats;
        model.ExecutionTime = stopwatch.ElapsedMilliseconds;
        model.ParallelMethod = "AsParallel().Aggregate";

        return View("AggregateResult", model);
    }

    // Parallel.Invoke Examples
    public async Task<IActionResult> InvokeDemo()
    {
        var model = new ParallelDemoViewModel
        {
            Title = "Parallel.Invoke Demonstration",
            Description = "Running multiple independent operations concurrently"
        };

        var stopwatch = Stopwatch.StartNew();

        var results = new ConcurrentBag<string>();

        Parallel.Invoke(
            () => results.Add($"Task 1: {PerformDatabaseOperation()}"),
            () => results.Add($"Task 2: {PerformApiCall()}"),
            () => results.Add($"Task 3: {ProcessImages()}"),
            () => results.Add($"Task 4: {SendNotifications()}")
        );

        stopwatch.Stop();

        model.InvokeResults = results.ToList();
        model.ExecutionTime = stopwatch.ElapsedMilliseconds;
        model.ParallelMethod = "Parallel.Invoke";

        return View("InvokeResult", model);
    }

    // PLINQ Examples
    public async Task<IActionResult> PlinqDemo()
    {
        var model = new ParallelDemoViewModel
        {
            Title = "PLINQ (Parallel LINQ) Demonstration",
            Description = "Querying and processing data using parallel LINQ operations"
        };

        // Generate customer data
        var customers = GenerateCustomerData(5000000);

        // Sequential processing
        var sequentialStopwatch = Stopwatch.StartNew();
        
        var sequentialHighValueCustomers = customers
            .Where(c => c.TotalPurchases > 5000)
            .OrderByDescending(c => c.TotalPurchases)
            .Take(10)
            .ToList();

        var sequentialAverageByRegion = customers
            .GroupBy(c => c.Region)
            .Select(g => new RegionStat
            {
                Region = g.Key,
                AveragePurchase = g.Average(c => c.TotalPurchases),
                CustomerCount = g.Count()
            })
            .OrderByDescending(r => r.AveragePurchase)
            .ToList();

        sequentialStopwatch.Stop();

        // Parallel processing (PLINQ)
        var parallelStopwatch = Stopwatch.StartNew();

        var highValueCustomers = customers
            .AsParallel()
            .Where(c => c.TotalPurchases > 5000)
            .OrderByDescending(c => c.TotalPurchases)
            .Take(10)
            .ToList();

        var averageByRegion = customers
            .AsParallel()
            .GroupBy(c => c.Region)
            .Select(g => new RegionStat
            {
                Region = g.Key,
                AveragePurchase = g.Average(c => c.TotalPurchases),
                CustomerCount = g.Count()
            })
            .OrderByDescending(r => r.AveragePurchase)
            .ToList();

        parallelStopwatch.Stop();

        model.PlinqResults = highValueCustomers;
        model.RegionStats = averageByRegion;
        model.ExecutionTime = parallelStopwatch.ElapsedMilliseconds;
        model.SequentialExecutionTime = sequentialStopwatch.ElapsedMilliseconds;
        model.ParallelMethod = "PLINQ";

        return View("PlinqResult", model);
    }

    // Dynamic Parallelism Examples
    public async Task<IActionResult> DynamicParallelismDemo()
    {
        var model = new ParallelDemoViewModel
        {
            Title = "Dynamic Parallelism Demonstration",
            Description = "Creating tasks dynamically during execution - Parent-Child Tasks and Tree Processing"
        };

        var stopwatch = Stopwatch.StartNew();
        var executionLog = new ConcurrentBag<string>();
        var treeResults = new ConcurrentBag<TreeProcessingResult>();

        // Example 1: Parent-Child Tasks
        executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Starting Parent Task");
        
        var parentTask = Task.Run(() =>
        {
            executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Parent task started");
            
            // Create child tasks dynamically
            var childTasks = new List<Task>();
            
            for (int i = 1; i <= 3; i++)
            {
                int childId = i;
                var childTask = Task.Run(() =>
                {
                    executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Child task {childId} started");
                    Thread.Sleep(Random.Shared.Next(500, 1500)); // Simulate work
                    executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Child task {childId} completed");
                });
                childTasks.Add(childTask);
            }
            
            // Wait for all children to complete
            Task.WaitAll(childTasks.ToArray());
            executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Parent task completed - all children finished");
        });

        await parentTask;

        // Example 2: Tree Processing with Dynamic Parallelism
        executionLog.Add($"[{DateTime.Now:HH:mm:ss.fff}] Starting Tree Processing");
        var rootNode = GenerateTreeStructure(4, 3); // Depth 4, branching factor 3
        
        await ProcessTreeDynamicParallel(rootNode, treeResults, executionLog, 0);

        stopwatch.Stop();

        model.DynamicParallelismData = new DynamicParallelismViewModel
        {
            Title = "Dynamic Parallelism Results",
            Description = "Results from parent-child tasks and tree processing",
            TreeResults = treeResults.ToList(),
            TaskExecutionLog = executionLog.ToList(),
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            TotalNodesProcessed = treeResults.Count,
            MaxDepth = treeResults.Any() ? treeResults.Max(r => r.Depth) : 0
        };

        return View("DynamicParallelismResult", model);
    }

    // Performance Comparison
    public async Task<IActionResult> PerformanceComparison()
    {
        var model = new PerformanceComparisonViewModel
        {
            Title = "Parallel Processing Performance Comparison"
        };

        // Test data
        var numbers = GenerateLargeNumberSet(100000);

        // Sequential processing
        var sequentialStopwatch = Stopwatch.StartNew();
        var sequentialSum = numbers.Sum();
        var sequentialStats = CalculateStatsSequentially(numbers);
        sequentialStopwatch.Stop();

        // Parallel processing
        var parallelStopwatch = Stopwatch.StartNew();
        var parallelSum = numbers.AsParallel().Sum();
        var parallelStats = numbers.AsParallel().Aggregate(
            () => new NumberStats(),
            (acc, num) => acc.Add(num),
            (left, right) => left.Combine(right),
            final => final
        );
        parallelStopwatch.Stop();

        model.SequentialTime = sequentialStopwatch.ElapsedMilliseconds;
        model.ParallelTime = parallelStopwatch.ElapsedMilliseconds;
        model.SequentialSum = sequentialSum;
        model.ParallelSum = parallelSum;
        model.SequentialStats = sequentialStats;
        model.ParallelStats = parallelStats;
        model.Speedup = (double)sequentialStopwatch.ElapsedMilliseconds / parallelStopwatch.ElapsedMilliseconds;

        return View(model);
    }

    #region Helper Methods

    private List<FileProcessingInfo> GenerateSampleFiles(int count)
    {
        var files = new List<FileProcessingInfo>();
        for (int i = 0; i < count; i++)
        {
            files.Add(new FileProcessingInfo
            {
                Name = $"file_{i}.txt",
                Size = Random.Shared.Next(1000, 100000),
                Type = i % 3 == 0 ? "Text" : i % 3 == 1 ? "Image" : "Document"
            });
        }
        return files;
    }

    private FileProcessingResult ProcessFile(FileProcessingInfo file)
    {
        // Simulate processing time based on file size
        Thread.Sleep(file.Size / 100);

        return new FileProcessingResult
        {
            FileName = file.Name,
            ProcessedSize = file.Size,
            ProcessingTime = file.Size / 100,
            Success = true
        };
    }

    private List<int> GenerateLargeNumberSet(int count)
    {
        var numbers = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            numbers.Add(Random.Shared.Next(1, 10000));
        }
        return numbers;
    }

    private NumberStats CalculateStatsSequentially(List<int> numbers)
    {
        var stats = new NumberStats();
        foreach (var num in numbers)
        {
            stats.Add(num);
        }
        return stats;
    }

    private string PerformDatabaseOperation()
    {
        Thread.Sleep(500); // Simulate DB operation
        return "Database query completed";
    }

    private string PerformApiCall()
    {
        Thread.Sleep(300); // Simulate API call
        return "External API call completed";
    }

    private string ProcessImages()
    {
        Thread.Sleep(1000); // Simulate image processing
        return "Image processing completed";
    }

    private string SendNotifications()
    {
        Thread.Sleep(800); // Simulate notification sending
        return "Notifications sent";
    }

    private List<Customer> GenerateCustomerData(int count)
    {
        var regions = new[] { "North", "South", "East", "West", "Central" };
        var customers = new List<Customer>();

        for (int i = 0; i < count; i++)
        {
            customers.Add(new Customer
            {
                Id = i + 1,
                Name = $"Customer {i + 1}",
                Region = regions[Random.Shared.Next(regions.Length)],
                TotalPurchases = Random.Shared.Next(100, 20000)
            });
        }

        return customers;
    }

    // Dynamic Parallelism Helper Methods
    private TreeNode GenerateTreeStructure(int maxDepth, int branchingFactor, int currentDepth = 0, int nodeId = 1)
    {
        var node = new TreeNode
        {
            Id = nodeId,
            Name = $"Node-{nodeId}",
            Value = Random.Shared.Next(1, 100),
            Depth = currentDepth
        };

        if (currentDepth < maxDepth - 1)
        {
            for (int i = 0; i < branchingFactor; i++)
            {
                var child = GenerateTreeStructure(maxDepth, branchingFactor, currentDepth + 1, nodeId * 10 + i + 1);
                child.Parent = node;
                node.Children.Add(child);
            }
        }

        return node;
    }

    private async Task ProcessTreeDynamicParallel(
        TreeNode node, 
        ConcurrentBag<TreeProcessingResult> results, 
        ConcurrentBag<string> log, 
        int depth)
    {
        var nodeStopwatch = Stopwatch.StartNew();
        
        log.Add($"[{DateTime.Now:HH:mm:ss.fff}] Processing node {node.Name} at depth {depth}");

        // Simulate processing the current node
        await Task.Run(() =>
        {
            Thread.Sleep(Random.Shared.Next(100, 500)); // Simulate work
            var processedValue = node.Value * 2; // Some processing

            results.Add(new TreeProcessingResult
            {
                NodeId = node.Id,
                NodeName = node.Name,
                ProcessedValue = processedValue,
                Depth = depth,
                ProcessingTimeMs = nodeStopwatch.ElapsedMilliseconds,
                Status = "Completed"
            });
        });

        nodeStopwatch.Stop();

        // Process children dynamically in parallel
        if (node.Children.Any())
        {
            var childTasks = node.Children.Select(child => 
                ProcessTreeDynamicParallel(child, results, log, depth + 1)
            ).ToArray();

            await Task.WhenAll(childTasks);
        }

        log.Add($"[{DateTime.Now:HH:mm:ss.fff}] Completed node {node.Name} and all its children");
    }

    #endregion
}
