using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ObservableUI;
using ObservableUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HTTP client for Web API communication
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:5001/")
});

// Register services
builder.Services.AddScoped<SimpleStreamingService>();
builder.Services.AddScoped<TransformationService>();
builder.Services.AddScoped<ErrorHandlingService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<CombiningService>();
// Note: Other services would need to be simplified as well for Blazor WebAssembly compatibility

await builder.Build().RunAsync();
