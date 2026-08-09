using AlgorithmVisualizer.Client;
using AlgorithmVisualizer.Client.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// In Blazor WebAssembly a scoped service lives for the browser application lifetime.
// This store contains only UI/playback state. Algorithm state remains in Core.
builder.Services.AddScoped<SimulationState>();

await builder.Build().RunAsync();
