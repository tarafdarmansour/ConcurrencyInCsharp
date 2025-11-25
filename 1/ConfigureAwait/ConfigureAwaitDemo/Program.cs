var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Weather forecast endpoint (unchanged)
app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// ConfigureAwait demonstration endpoints
app.MapGet("/configureawait/with", async () =>
{
    var threadId = Environment.CurrentManagedThreadId;
    await Task.Delay(100).ConfigureAwait(false); // Properly configured
    var continuationThreadId = Environment.CurrentManagedThreadId;

    return new
    {
        InitialThreadId = threadId,
        ContinuationThreadId = continuationThreadId,
        UsedConfigureAwait = true,
        Message = "This continuation can run on any thread pool thread"
    };
})
.WithName("WithConfigureAwait")
.WithOpenApi();

app.MapGet("/configureawait/without", async () =>
{
    var threadId = Environment.CurrentManagedThreadId;
    await Task.Delay(100); // Without ConfigureAwait - captures context
    var continuationThreadId = Environment.CurrentManagedThreadId;

    return new
    {
        InitialThreadId = threadId,
        ContinuationThreadId = continuationThreadId,
        UsedConfigureAwait = false,
        Message = "This continuation tries to resume on the original ASP.NET context thread"
    };
})
.WithName("WithoutConfigureAwait")
.WithOpenApi();

app.MapGet("/configureawait/simulation/good", async () =>
{
    var startTime = DateTime.UtcNow;
    var tasks = new List<Task<string>>();

    // Simulate multiple I/O operations with proper ConfigureAwait usage
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(SimulateAsyncIOWithConfigureAwait(i));
    }

    var results = await Task.WhenAll(tasks);
    var endTime = DateTime.UtcNow;

    return new
    {
        Duration = (endTime - startTime).TotalMilliseconds,
        Results = results,
        Pattern = "Proper ConfigureAwait usage - threads can be reused efficiently"
    };
})
.WithName("GoodConfigureAwaitSimulation")
.WithOpenApi();

app.MapGet("/configureawait/simulation/bad", async () =>
{
    var startTime = DateTime.UtcNow;
    var tasks = new List<Task<string>>();

    // Simulate multiple I/O operations without ConfigureAwait
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(SimulateAsyncIOWithoutConfigureAwait(i));
    }

    var results = await Task.WhenAll(tasks);
    var endTime = DateTime.UtcNow;

    return new
    {
        Duration = (endTime - startTime).TotalMilliseconds,
        Results = results,
        Pattern = "Without ConfigureAwait - may cause thread pool starvation under load"
    };
})
.WithName("BadConfigureAwaitSimulation")
.WithOpenApi();

app.MapGet("/configureawait/loadtest/{count:int}", async (int count) =>
{
    if (count < 1 || count > 1000)
        return Results.BadRequest("Count must be between 1 and 1000");

    var startTime = DateTime.UtcNow;
    var tasks = new List<Task<string>>();

    // Simulate load with proper ConfigureAwait usage
    for (int i = 0; i < count; i++)
    {
        tasks.Add(SimulateAsyncIOWithConfigureAwait(i));
    }

    var results = await Task.WhenAll(tasks);
    var endTime = DateTime.UtcNow;

    ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
    ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

    return Results.Ok(new
    {
        RequestCount = count,
        Duration = (endTime - startTime).TotalMilliseconds,
        AvgResponseTime = (endTime - startTime).TotalMilliseconds / count,
        ThreadPoolInfo = new
        {
            AvailableWorkerThreads = availableWorkerThreads,
            AvailableCompletionPortThreads = availableCompletionPortThreads,
            MaxWorkerThreads = maxWorkerThreads,
            MaxCompletionPortThreads = maxCompletionPortThreads
        }
    });
})
.WithName("LoadTestWithConfigureAwait")
.WithOpenApi();

app.MapGet("/configureawait/performance/comparison/{iterations:int}", async (int iterations) =>
{
    if (iterations < 1 || iterations > 100)
        return Results.BadRequest("Iterations must be between 1 and 100");

    // Test with ConfigureAwait(false)
    var withConfigureAwaitResults = new List<long>();
    for (int i = 0; i < iterations; i++)
    {
        var start = DateTime.UtcNow;
        await PerformanceTestWithConfigureAwait();
        var end = DateTime.UtcNow;
        withConfigureAwaitResults.Add((end - start).Ticks);
    }

    // Test without ConfigureAwait
    var withoutConfigureAwaitResults = new List<long>();
    for (int i = 0; i < iterations; i++)
    {
        var start = DateTime.UtcNow;
        await PerformanceTestWithoutConfigureAwait();
        var end = DateTime.UtcNow;
        withoutConfigureAwaitResults.Add((end - start).Ticks);
    }

    var avgWith = TimeSpan.FromTicks((long)withConfigureAwaitResults.Average()).TotalMilliseconds;
    var avgWithout = TimeSpan.FromTicks((long)withoutConfigureAwaitResults.Average()).TotalMilliseconds;

    return Results.Ok(new
    {
        Iterations = iterations,
        WithConfigureAwait = new
        {
            AverageMs = avgWith,
            MinMs = TimeSpan.FromTicks(withConfigureAwaitResults.Min()).TotalMilliseconds,
            MaxMs = TimeSpan.FromTicks(withConfigureAwaitResults.Max()).TotalMilliseconds
        },
        WithoutConfigureAwait = new
        {
            AverageMs = avgWithout,
            MinMs = TimeSpan.FromTicks(withoutConfigureAwaitResults.Min()).TotalMilliseconds,
            MaxMs = TimeSpan.FromTicks(withoutConfigureAwaitResults.Max()).TotalMilliseconds
        },
        PerformanceDifference = new
        {
            Ms = avgWithout - avgWith,
            Percentage = avgWith > 0 ? ((avgWithout - avgWith) / avgWith) * 100 : 0
        }
    });
})
.WithName("PerformanceComparison")
.WithOpenApi();

app.Run();

// Helper methods to simulate async I/O operations
async Task<string> SimulateAsyncIOWithConfigureAwait(int id)
{
    // Simulate database call or external API call
    await Task.Delay(Random.Shared.Next(10, 50)).ConfigureAwait(false);
    // Simulate some CPU work
    await Task.Run(() => Thread.Sleep(Random.Shared.Next(5, 15))).ConfigureAwait(false);
    return $"Operation {id} completed on thread {Environment.CurrentManagedThreadId}";
}

async Task<string> SimulateAsyncIOWithoutConfigureAwait(int id)
{
    // Simulate database call or external API call (without ConfigureAwait)
    await Task.Delay(Random.Shared.Next(10, 50));
    // Simulate some CPU work
    await Task.Run(() => Thread.Sleep(Random.Shared.Next(5, 15)));
    return $"Operation {id} completed on thread {Environment.CurrentManagedThreadId}";
}

// Performance test methods for comparison
async Task PerformanceTestWithConfigureAwait()
{
    // Simulate a typical async operation chain with ConfigureAwait
    var data1 = await SimulateExternalCall().ConfigureAwait(false);
    var data2 = await SimulateDatabaseCall().ConfigureAwait(false);
    var result = await ProcessDataAsync(data1 + data2).ConfigureAwait(false);
}

async Task PerformanceTestWithoutConfigureAwait()
{
    // Simulate the same operations without ConfigureAwait
    var data1 = await SimulateExternalCall();
    var data2 = await SimulateDatabaseCall();
    var result = await ProcessDataAsync(data1 + data2);
}

// Additional simulation methods
async Task<string> SimulateExternalCall()
{
    await Task.Delay(Random.Shared.Next(5, 15));
    return "external_data";
}

async Task<string> SimulateDatabaseCall()
{
    await Task.Delay(Random.Shared.Next(8, 25));
    return "db_data";
}

async Task<string> ProcessDataAsync(string data)
{
    await Task.Run(() => Thread.Sleep(Random.Shared.Next(3, 10)));
    return data.ToUpper();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
