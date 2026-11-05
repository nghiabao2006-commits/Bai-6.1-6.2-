using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.DAL;

public class KhachHangDAL
{
    public List<KhachHang> GetAll()
    {
        var result = new List<KhachHang>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(
            "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM dbo.KhachHang ORDER BY TenKH", connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public int Insert(KhachHang khachHang)
    {
        const string sql = @"
            INSERT INTO dbo.KhachHang (TenKH, DienThoai, DiemTichLuy)
            VALUES (@TenKH, @DienThoai, @DiemTichLuy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, khachHang);

            connection.Open();
            return (int)command.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Số điện thoại của khách hàng đã tồn tại.",
                "Không thể lưu khách hàng vì dữ liệu đang được sử dụng.");
        }
    }

    public void Update(KhachHang khachHang)
    {
        const string sql = @"
            UPDATE dbo.KhachHang
            SET TenKH = @TenKH, DienThoai = @DienThoai, DiemTichLuy = @DiemTichLuy
            WHERE MaKH = @MaKH;";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, khachHang);
            command.Parameters.Add("@MaKH", SqlDbType.Int).Value = khachHang.MaKH;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Số điện thoại của khách hàng đã tồn tại.",
                "Không thể cập nhật khách hàng vì dữ liệu đang được sử dụng.");
        }
    }

    public void Delete(int maKH)
    {
        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.KhachHang WHERE MaKH = @MaKH", connection);
            command.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Không thể xóa khách hàng do dữ liệu bị trùng.",
                "Không thể xóa khách hàng vì khách hàng đã có hóa đơn.");
        }
    }

    private static void AddParameters(SqlCommand command, KhachHang khachHang)
    {
        command.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = khachHang.TenKH;
        command.Parameters.Add("@DienThoai", SqlDbType.VarChar, 15).Value =
            string.IsNullOrWhiteSpace(khachHang.DienThoai) ? DBNull.Value : khachHang.DienThoai;
        command.Parameters.Add("@DiemTichLuy", SqlDbType.Int).Value = khachHang.DiemTichLuy;
    }

    private static KhachHang Map(SqlDataReader reader)
    {
        return new KhachHang
        {
            MaKH = reader.GetInt32(0),
            TenKH = reader.GetString(1),
            DienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
            DiemTichLuy = reader.GetInt32(3)
        };
    }
}
