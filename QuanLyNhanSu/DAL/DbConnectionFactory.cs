using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace QuanLyNhanSu.DAL;

/// <summary>
/// Tạo kết nối ADO.NET tới HRMDB. Tự dò instance SQL Server đang chạy trên máy
/// (mặc định, các instance có tên và LocalDB) để source tải về máy khác vẫn chạy được.
/// </summary>
public static class DbConnectionFactory
{
    public const string DatabaseName = "HRMDB";

    private static readonly object SyncRoot = new();
    private static readonly string ConfiguredConnectionString = ReadConfiguredConnectionString();

    private static string _server;
    private static string _connectionString;

    public static string ServerName
    {
        get
        {
            EnsureResolved();
            return _server;
        }
    }

    public static string ConnectionString
    {
        get
        {
            EnsureResolved();
            return _connectionString;
        }
    }

    public static string MasterConnectionString
    {
        get
        {
            EnsureResolved();
            return Build(_server, "master", 10);
        }
    }

    private static string ReadConfiguredConnectionString()
    {
        try
        {
            return ConfigurationManager.ConnectionStrings[DatabaseName]?.ConnectionString;
        }
        catch (ConfigurationErrorsException)
        {
            return null;
        }
    }

    private static void EnsureResolved()
    {
        if (_connectionString != null)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_connectionString != null)
            {
                return;
            }

            var errors = new List<string>();

            foreach (string server in GetCandidateServers())
            {
                try
                {
                    using var connection = new SqlConnection(Build(server, "master", 3));
                    connection.Open();

                    _server = server;
                    _connectionString = Build(server, DatabaseName, 15);
                    return;
                }
                catch (Exception ex)
                {
                    errors.Add($"  - {server}: {FirstLine(ex.Message)}");
                }
            }

            throw new InvalidOperationException(
                "Không thể kết nối đến SQL Server. Đã thử các máy chủ sau:\n" +
                string.Join(Environment.NewLine, errors) +
                "\n\nGợi ý: cài SQL Server Express/LocalDB hoặc bật dịch vụ SQL Server, " +
                "sau đó kiểm tra chuỗi kết nối trong App.config.");
        }
    }

    private static IEnumerable<string> GetCandidateServers()
    {
        var servers = new List<string>();

        try
        {
            if (!string.IsNullOrWhiteSpace(ConfiguredConnectionString))
            {
                AddServer(servers, new SqlConnectionStringBuilder(ConfiguredConnectionString).DataSource);
            }
        }
        catch
        {
            // Chuỗi cấu hình sai định dạng: bỏ qua và dùng các ứng viên mặc định.
        }

        AddServer(servers, ".");
        AddServer(servers, @".\SQLEXPRESS");

        foreach (string instance in GetLocalInstances())
        {
            AddServer(servers, instance);
        }

        AddServer(servers, @"(localdb)\MSSQLLocalDB");

        return servers;
    }

    private static IEnumerable<string> GetLocalInstances()
    {
        var instances = new List<string>();

        foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");

                if (key == null)
                {
                    continue;
                }

                foreach (string name in key.GetValueNames())
                {
                    if (string.Equals(name, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    instances.Add(@".\" + name);
                }
            }
            catch
            {
                // Không đọc được registry: bỏ qua.
            }
        }

        return instances;
    }

    private static void AddServer(List<string> servers, string server)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            return;
        }

        if (!servers.Any(x => string.Equals(x, server, StringComparison.OrdinalIgnoreCase)))
        {
            servers.Add(server);
        }
    }

    private static string Build(string server, string database, int timeoutSeconds)
    {
        SqlConnectionStringBuilder builder;

        if (string.IsNullOrWhiteSpace(ConfiguredConnectionString))
        {
            builder = new SqlConnectionStringBuilder { IntegratedSecurity = true };
        }
        else
        {
            builder = new SqlConnectionStringBuilder(ConfiguredConnectionString);
        }

        builder.DataSource = server;
        builder.InitialCatalog = database;
        builder.ConnectTimeout = timeoutSeconds;
        builder.TrustServerCertificate = true;

        return builder.ConnectionString;
    }

    private static string FirstLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Không rõ lỗi";
        }

        int index = message.IndexOfAny(new[] { '\r', '\n' });
        return index < 0 ? message : message[..index];
    }
}
