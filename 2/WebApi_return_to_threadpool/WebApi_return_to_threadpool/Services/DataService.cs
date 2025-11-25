namespace WebApi_return_to_threadpool.Services;

public class DataService
{
    private readonly HttpClient _httpClient;

    public DataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Without ConfigureAwait(false)
    public async Task<string> GetDataAsync()
    {
        var result = await _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/todos/1");
        await Task.Delay(TimeSpan.FromSeconds(1));
        return result;
    }

    // With ConfigureAwait(false)
    public async Task<string> GetDataWithConfigureAwaitAsync()
    {
        var result = await _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/todos/2")
            .ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromSeconds(1));
        return result;
    }

    // With ConfigureAwait(true)
    public async Task<string> GetDataWithConfigureAwaitAsyncTrue()
    {
        var result = await _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/todos/2")
            .ConfigureAwait(true);
        await Task.Delay(TimeSpan.FromSeconds(1));
        return result;
    }
}