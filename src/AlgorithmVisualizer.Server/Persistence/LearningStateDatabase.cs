using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AlgorithmVisualizer.Server.Persistence;

public sealed class LearningStateDatabase
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public LearningStateDatabase(IHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration["LearningDatabase:Path"];
        _databasePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "learning-state.db")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public string DatabasePath => _databasePath;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode = WAL;
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS LearningState (
                UserId TEXT NOT NULL,
                StateKey TEXT NOT NULL,
                StateValue TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                PRIMARY KEY (UserId, StateKey)
            );

            CREATE INDEX IF NOT EXISTS IX_LearningState_UserId
                ON LearningState (UserId);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetAllAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT StateKey, StateValue
            FROM LearningState
            WHERE UserId = $userId
            ORDER BY StateKey;
            """;
        command.Parameters.AddWithValue("$userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }

    public async Task UpsertAsync(
        string userId,
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateKey(key);
        ValidateValue(value);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO LearningState (UserId, StateKey, StateValue, UpdatedAtUtc)
            VALUES ($userId, $stateKey, $stateValue, $updatedAtUtc)
            ON CONFLICT(UserId, StateKey) DO UPDATE SET
                StateValue = excluded.StateValue,
                UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        command.Parameters.AddWithValue("$userId", userId);
        command.Parameters.AddWithValue("$stateKey", key);
        command.Parameters.AddWithValue("$stateValue", value);
        command.Parameters.AddWithValue(
            "$updatedAtUtc",
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        string userId,
        string key,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateKey(key);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM LearningState
            WHERE UserId = $userId AND StateKey = $stateKey;
            """;
        command.Parameters.AddWithValue("$userId", userId);
        command.Parameters.AddWithValue("$stateKey", key);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM LearningState WHERE UserId = $userId;";
        command.Parameters.AddWithValue("$userId", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void ValidateUserId(string userId)
    {
        if (!Guid.TryParseExact(userId, "N", out _))
        {
            throw new ArgumentException("UserId must be a normalized GUID.", nameof(userId));
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
        {
            throw new ArgumentException("State key must contain 1 to 200 characters.", nameof(key));
        }
    }

    private static void ValidateValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > 65_536)
        {
            throw new ArgumentException("State value cannot exceed 64 KiB.", nameof(value));
        }
    }
}
