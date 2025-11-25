using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParallelProcessingDemo.Models;
using System.Threading.Tasks;

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

        var stopwatch = Stopwatch.StartNew();

        // Generate customer data
        var customers = GenerateCustomerData(50000);

        // PLINQ query with parallel processing
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

        stopwatch.Stop();

        model.PlinqResults = highValueCustomers;
        model.RegionStats = averageByRegion;
        model.ExecutionTime = stopwatch.ElapsedMilliseconds;
        model.ParallelMethod = "PLINQ";

        return View("PlinqResult", model);
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

    #endregion
}
