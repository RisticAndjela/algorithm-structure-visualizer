using AlgorithmVisualizer.Client;
using AlgorithmVisualizer.Client.State;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// In Blazor WebAssembly a scoped service lives for the browser application lifetime.
// The concrete state is used by UI controls, while Core code depends only on ISimulationRuntime.
builder.Services.AddScoped<SimulationState>();
builder.Services.AddScoped<ISimulationRuntime>(serviceProvider =>
    serviceProvider.GetRequiredService<SimulationState>());

await builder.Build().RunAsync();
