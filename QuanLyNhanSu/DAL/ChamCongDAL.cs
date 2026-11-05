using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL;

/// <summary>
/// Chấm công theo mô hình ngắt kết nối: SqlDataAdapter nạp DataTable,
/// SqlCommandBuilder + adapter.Update() đồng bộ thay đổi về SQL Server.
/// </summary>
public class ChamCongDAL
{
    public DataTable GetTable(string thangNam)
    {
        EnsureRowsForMonth(thangNam);

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var adapter = new SqlDataAdapter(
            @"SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot
              FROM dbo.BangChamCong
              WHERE ThangNam = @ThangNam
              ORDER BY MaNV;",
            connection);

        adapter.SelectCommand.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;
        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    /// <summary>Bảo đảm mọi nhân viên đều có dòng chấm công cho tháng đang chọn.</summary>
    public void EnsureRowsForMonth(string thangNam)
    {
        const string sql = @"
            INSERT INTO dbo.BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT)
            SELECT nv.MaNV, @ThangNam, 0, 0
            FROM dbo.NhanVien nv
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.BangChamCong cc
                WHERE cc.MaNV = nv.MaNV AND cc.ThangNam = @ThangNam
            );";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool IsMonthLocked(string thangNam)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM dbo.BangChamCong
            WHERE ThangNam = @ThangNam AND DaChot = 1;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int Save(DataTable table, string thangNam)
    {
        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var adapter = new SqlDataAdapter(
            @"SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot
              FROM dbo.BangChamCong
              WHERE ThangNam = @ThangNam;",
            connection);

        adapter.SelectCommand.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;
        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

        using var builder = new SqlCommandBuilder(adapter);
        return adapter.Update(table);
    }
}
