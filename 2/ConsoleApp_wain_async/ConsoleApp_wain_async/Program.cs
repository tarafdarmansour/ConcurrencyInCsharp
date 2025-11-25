// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Deadlock();


async Task WaitAsync()
{
    Console.WriteLine($"This await will capture the current context ... {Environment.CurrentManagedThreadId}");
    await Task.Delay(TimeSpan.FromSeconds(4));
    Console.WriteLine($"and will attempt to resume the method here in that context.{Environment.CurrentManagedThreadId}");
}
void Deadlock()
{
    Console.WriteLine("Start the delay.");
    Task task = WaitAsync();
    Console.WriteLine("Synchronously block, waiting for the async method to complete.");
    task.Wait();
}