using AlgorithmVisualizer.Client;
using AlgorithmVisualizer.Client.State;
using AlgorithmVisualizer.Core.DataStructures.Linear.Queue;
using AlgorithmVisualizer.Core.DataStructures.Linear.Stack;
using AlgorithmVisualizer.Core.DataStructures.Trees.Bst;
using AlgorithmVisualizer.Core.DataStructures.Trees.Avl;
using AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;
using AlgorithmVisualizer.Core.DataStructures.Heap;
using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.Optimization.GradientDescent;
using AlgorithmVisualizer.Core.MachineLearning.Supervised.LinearRegression;
using AlgorithmVisualizer.Core.MachineLearning.Supervised.LogisticRegression;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;
using AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;
using AlgorithmVisualizer.Core.Algorithms.Search.Linear;
using AlgorithmVisualizer.Core.Algorithms.Search.Binary;
using AlgorithmVisualizer.Core.Algorithms.GraphTraversal;
using AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra;
using AlgorithmVisualizer.Core.Algorithms.GraphOrdering.Topological;
using AlgorithmVisualizer.Core.Algorithms.GraphSpanningTree.Mst;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The hosted ASP.NET Core project serves the WASM client and the persistence API
// from the same origin, so HttpClient needs no CORS setup or browser-storage JS.
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// In Blazor WebAssembly a scoped service lives for the browser application lifetime.
// The concrete state is used by UI controls, while Core code depends only on ISimulationRuntime.
builder.Services.AddScoped<SimulationState>();
builder.Services.AddScoped<LearningSessionStore>();
builder.Services.AddScoped<PracticeProgressStore>();
builder.Services.AddScoped<ISimulationRuntime>(serviceProvider =>
    serviceProvider.GetRequiredService<SimulationState>());

// Live simulation services own data-structure state, not rendering concerns.
builder.Services.AddScoped<StackSimulation>();
builder.Services.AddScoped<QueueSimulation>();
builder.Services.AddScoped<BstSimulation>();
builder.Services.AddScoped<AvlSimulation>();
builder.Services.AddScoped<RedBlackSimulation>();
builder.Services.AddScoped<HeapSimulation>();
builder.Services.AddScoped<DaryHeapSimulation>();
builder.Services.AddScoped<GraphSimulation>();
builder.Services.AddScoped<VectorSimulation>();
builder.Services.AddScoped<GradientDescentSimulation>();
builder.Services.AddScoped<LinearRegressionSimulation>();
builder.Services.AddScoped<LogisticRegressionSimulation>();
builder.Services.AddScoped<BubbleSortSimulation>();
builder.Services.AddScoped<SelectionSortSimulation>();
builder.Services.AddScoped<InsertionSortSimulation>();
builder.Services.AddScoped<MergeSortSimulation>();
builder.Services.AddScoped<QuickSortSimulation>();
builder.Services.AddScoped<HeapSortSimulation>();
builder.Services.AddScoped<LinearSearchSimulation>();
builder.Services.AddScoped<BinarySearchSimulation>();
builder.Services.AddScoped<BinarySearchInputSorter>();
builder.Services.AddScoped<BreadthFirstSearchSimulation>();
builder.Services.AddScoped<DepthFirstSearchSimulation>();
builder.Services.AddScoped<DijkstraSimulation>();
builder.Services.AddScoped<TopologicalSortSimulation>();
builder.Services.AddScoped<MstSimulation>();

var host = builder.Build();

// Load persisted progress/preferences before Razor pages render so existing synchronous
// GetItem calls see the SQL-backed state immediately.
await host.Services.GetRequiredService<LearningSessionStore>().InitializeAsync();
await host.RunAsync();
