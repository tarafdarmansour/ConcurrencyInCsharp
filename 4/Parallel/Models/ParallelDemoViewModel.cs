using System.Collections.Generic;

namespace ParallelProcessingDemo.Models;

public class ParallelDemoViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParallelMethod { get; set; } = string.Empty;
    public long ExecutionTime { get; set; }
    public long SequentialExecutionTime { get; set; }

    // For ForEach demo
    public List<FileProcessingResult>? ParallelResults { get; set; }

    // For Aggregate demo
    public NumberStats? AggregateStats { get; set; }

    // For Invoke demo
    public List<string>? InvokeResults { get; set; }

    // For PLINQ demo
    public List<Customer>? PlinqResults { get; set; }
    public List<RegionStat>? RegionStats { get; set; }

    // For Dynamic Parallelism demo
    public DynamicParallelismViewModel? DynamicParallelismData { get; set; }
}

public class FileProcessingInfo
{
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class FileProcessingResult
{
    public string FileName { get; set; } = string.Empty;
    public int ProcessedSize { get; set; }
    public int ProcessingTime { get; set; }
    public bool Success { get; set; }
}

public class NumberStats
{
    public int Count { get; private set; }
    public int Sum { get; private set; }
    public double Average => Count > 0 ? (double)Sum / Count : 0;
    public int Min { get; private set; } = int.MaxValue;
    public int Max { get; private set; } = int.MinValue;

    public NumberStats Add(int number)
    {
        Count++;
        Sum += number;
        Min = Math.Min(Min, number);
        Max = Math.Max(Max, number);
        return this;
    }

    public NumberStats Combine(NumberStats other)
    {
        var combined = new NumberStats
        {
            Count = this.Count + other.Count,
            Sum = this.Sum + other.Sum,
            Min = Math.Min(this.Min, other.Min),
            Max = Math.Max(this.Max, other.Max)
        };
        return combined;
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
}

public class RegionStat
{
    public string Region { get; set; } = string.Empty;
    public decimal AveragePurchase { get; set; }
    public int CustomerCount { get; set; }
}

public class PerformanceComparisonViewModel
{
    public string Title { get; set; } = string.Empty;
    public long SequentialTime { get; set; }
    public long ParallelTime { get; set; }
    public long SequentialSum { get; set; }
    public long ParallelSum { get; set; }
    public NumberStats? SequentialStats { get; set; }
    public NumberStats? ParallelStats { get; set; }
    public double Speedup { get; set; }
}

// Dynamic Parallelism Models
public class TreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public List<TreeNode> Children { get; set; } = new();
    public TreeNode? Parent { get; set; }
    public int Depth { get; set; }
}

public class TreeProcessingResult
{
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public int ProcessedValue { get; set; }
    public int Depth { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DynamicParallelismViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TreeProcessingResult>? TreeResults { get; set; }
    public List<string>? TaskExecutionLog { get; set; }
    public long ExecutionTime { get; set; }
    public int TotalNodesProcessed { get; set; }
    public int MaxDepth { get; set; }
}