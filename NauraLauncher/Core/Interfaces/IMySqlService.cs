using System.Collections.Generic;
using System.Threading.Tasks;

namespace NauraLauncher.Core.Interfaces;

public interface IMySqlService
{
    Task<bool> TestConnectionAsync(string connectionString);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null);
}
