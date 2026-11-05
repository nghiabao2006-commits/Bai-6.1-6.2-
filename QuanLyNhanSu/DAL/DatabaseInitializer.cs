using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL;

/// <summary>
/// Tự tạo CSDL HRMDB và nạp dữ liệu mẫu từ script nhúng trong ứng dụng.
/// </summary>
public static class DatabaseInitializer
{
    private const string ScriptFileName = "ScriptDB_HRMDB.sql";
    private const string CoreTable = "dbo.NhanVien";

    public static void EnsureDatabase()
    {
        if (!DatabaseExists())
        {
            RunScript();
            return;
        }

        if (!CoreTableExists())
        {
            RunScript();
        }
    }

    private static bool DatabaseExists()
    {
        using var connection = new SqlConnection(DbConnectionFactory.MasterConnectionString);
        connection.Open();

        using var command = new SqlCommand("SELECT DB_ID(@TenCSDL)", connection);
        command.Parameters.Add("@TenCSDL", SqlDbType.NVarChar, 128).Value = DbConnectionFactory.DatabaseName;

        object result = command.ExecuteScalar();
        return result != null && result != DBNull.Value;
    }

    private static bool CoreTableExists()
    {
        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        connection.Open();

        using var command = new SqlCommand("SELECT OBJECT_ID(@TenBang, N'U')", connection);
        command.Parameters.Add("@TenBang", SqlDbType.NVarChar, 128).Value = CoreTable;

        object result = command.ExecuteScalar();
        return result != null && result != DBNull.Value;
    }

    private static void RunScript()
    {
        string script = LoadScript();

        using var connection = new SqlConnection(DbConnectionFactory.MasterConnectionString);
        connection.Open();

        foreach (string batch in SplitBatches(script))
        {
            using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
            command.ExecuteNonQuery();
        }
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        return Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(batch => !string.IsNullOrWhiteSpace(batch));
    }

    private static string LoadScript()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(ScriptFileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InvalidOperationException(
                $"Không tìm thấy script {ScriptFileName} được nhúng trong ứng dụng.");
        }

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        return reader.ReadToEnd();
    }
}
