using MySqlConnector;

namespace NauraLauncher.Server.Infrastructure;

/// <summary>
/// MySQL connection factory - compatible with MySQL 5.6
/// Uses MySqlConnector library which supports MySQL 5.6
/// Clean architecture: Infrastructure layer
/// </summary>
public class MySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        // Ensure charset is utf8 for MySQL 5.6 compatibility
        if (!connectionString.Contains("CharSet", StringComparison.OrdinalIgnoreCase))
        {
            _connectionString = connectionString.TrimEnd(';') + ";CharSet=utf8;";
        }
        else
        {
            _connectionString = connectionString;
        }
    }

    public async Task<MySqlConnection> CreateConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public MySqlConnection CreateConnection()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Validates connection string for MySQL 5.6 compatibility
    /// </summary>
    public static bool ValidateConnectionString(string connectionString, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            error = "Connection string is empty";
            return false;
        }

        // Check for incompatible options that might be MySQL 8.0+ only
        var lower = connectionString.ToLowerInvariant();

        // These are generally OK, but we warn about potential issues
        if (lower.Contains("caching_sha2_password"))
        {
            error = "caching_sha2_password is MySQL 8.0+ default, MySQL 5.6 uses mysql_native_password. Use mysql_native_password or update server.";
            return false;
        }

        // Check for SSL options that might fail on old MySQL
        // Not an error, just a note

        return true;
    }
}

/// <summary>
/// Repository base with common MySQL 5.6 safe operations
/// </summary>
public abstract class MySqlRepositoryBase
{
    protected readonly MySqlConnectionFactory _connectionFactory;

    protected MySqlRepositoryBase(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected async Task<T> ExecuteWithConnectionAsync<T>(Func<MySqlConnection, Task<T>> action)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await action(connection);
    }

    protected async Task ExecuteWithConnectionAsync(Func<MySqlConnection, Task> action)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await action(connection);
    }

    // MySQL 5.6 safe query helpers - avoid MySQL 8.0 syntax
    protected string BuildPaginationClause(int page, int pageSize)
    {
        // MySQL 5.6 supports LIMIT offset, count - safe
        var offset = page * pageSize;
        return $"LIMIT {offset}, {pageSize}";
    }

    protected string EscapeLikePattern(string input)
    {
        // Escape special characters for LIKE queries
        return input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
