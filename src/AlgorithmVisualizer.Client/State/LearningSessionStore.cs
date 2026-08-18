using System.Text.Json;
using System.Net.Http.Json;

namespace AlgorithmVisualizer.Client.State;

/// <summary>
/// Keeps learning preferences and practice progress in C# memory and mirrors them
/// to the ASP.NET Core persistence API backed by SQLite.
///
/// No browser-storage JavaScript is used. The in-memory dictionary makes reads
/// synchronous for Razor pages after startup; writes are serialized and persisted
/// through HttpClient in the background. If the backend is temporarily unavailable,
/// the current browser session remains usable and keeps its in-memory values.
/// </summary>
public sealed class LearningSessionStore
{
    private const string ApiRoute = "api/learning-state";

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    private Task _writeQueue = Task.CompletedTask;

    public LearningSessionStore(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool IsDurablePersistenceAvailable { get; private set; }
    public string? LastPersistenceError { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var persisted = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
                ApiRoute,
                cancellationToken);

            _values.Clear();
            if (persisted is not null)
            {
                foreach (var (key, value) in persisted)
                {
                    if (!string.IsNullOrWhiteSpace(key) && value is not null)
                    {
                        _values[key] = value;
                    }
                }
            }

            IsDurablePersistenceAvailable = true;
            LastPersistenceError = null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or NotSupportedException or JsonException)
        {
            IsDurablePersistenceAvailable = false;
            LastPersistenceError = exception.Message;
        }
    }

    public string? GetItem(string key)
    {
        ValidateKey(key);
        return _values.TryGetValue(key, out var value) ? value : null;
    }

    public void SetItem(string key, string value)
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(value);

        _values[key] = value;
        _writeQueue = PersistAfterAsync(
            _writeQueue,
            () => PersistItemAsync(key, value));
    }

    public void RemoveItem(string key)
    {
        ValidateKey(key);
        _values.Remove(key);
        _writeQueue = PersistAfterAsync(
            _writeQueue,
            () => DeleteItemAsync(key));
    }

    public void Clear()
    {
        _values.Clear();
        _writeQueue = PersistAfterAsync(
            _writeQueue,
            ClearPersistedStateAsync);
    }

    /// <summary>
    /// Allows tests or explicit application flows to wait until all queued SQL-backed
    /// persistence requests have completed.
    /// </summary>
    public Task FlushAsync() => _writeQueue;

    private async Task PersistItemAsync(string key, string value)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            ApiRoute,
            new LearningStateWriteRequest(key, value));
        response.EnsureSuccessStatusCode();
    }

    private async Task DeleteItemAsync(string key)
    {
        using var response = await _httpClient.DeleteAsync(
            $"{ApiRoute}/{Uri.EscapeDataString(key)}");
        response.EnsureSuccessStatusCode();
    }

    private async Task ClearPersistedStateAsync()
    {
        using var response = await _httpClient.DeleteAsync(ApiRoute);
        response.EnsureSuccessStatusCode();
    }

    private async Task PersistAfterAsync(Task previousWrite, Func<Task> persistenceAction)
    {
        try
        {
            await previousWrite;
        }
        catch
        {
            // A previous failed write must not permanently block later persistence.
        }

        try
        {
            await persistenceAction();
            IsDurablePersistenceAvailable = true;
            LastPersistenceError = null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            IsDurablePersistenceAvailable = false;
            LastPersistenceError = exception.Message;
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A learning-state key is required.", nameof(key));
        }

        if (key.Length > 200)
        {
            throw new ArgumentException("A learning-state key cannot exceed 200 characters.", nameof(key));
        }
    }

    private sealed record LearningStateWriteRequest(string Key, string Value);
}
