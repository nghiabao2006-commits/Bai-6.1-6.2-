using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.DAL;

public class NhanVienDAL
{
    public List<NhanVien> GetAll()
    {
        var result = new List<NhanVien>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(
            "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM dbo.NhanVien ORDER BY TenNV", connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public int Insert(NhanVien nhanVien)
    {
        const string sql = @"
            INSERT INTO dbo.NhanVien (TenNV, SoDienThoai, ChucVu)
            VALUES (@TenNV, @SoDienThoai, @ChucVu);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, nhanVien);

            connection.Open();
            return (int)command.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Số điện thoại của nhân viên đã tồn tại.",
                "Không thể lưu nhân viên vì dữ liệu đang được sử dụng.");
        }
    }

    public void Update(NhanVien nhanVien)
    {
        const string sql = @"
            UPDATE dbo.NhanVien
            SET TenNV = @TenNV, SoDienThoai = @SoDienThoai, ChucVu = @ChucVu
            WHERE MaNV = @MaNV;";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, nhanVien);
            command.Parameters.Add("@MaNV", SqlDbType.Int).Value = nhanVien.MaNV;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Số điện thoại của nhân viên đã tồn tại.",
                "Không thể cập nhật nhân viên vì dữ liệu đang được sử dụng.");
        }
    }

    public void Delete(int maNV)
    {
        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.NhanVien WHERE MaNV = @MaNV", connection);
            command.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNV;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Không thể xóa nhân viên do dữ liệu bị trùng.",
                "Không thể xóa nhân viên vì nhân viên đã có hóa đơn bán hàng.");
        }
    }

    private static void AddParameters(SqlCommand command, NhanVien nhanVien)
    {
        command.Parameters.Add("@TenNV", SqlDbType.NVarChar, 100).Value = nhanVien.TenNV;
        command.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value =
            string.IsNullOrWhiteSpace(nhanVien.SoDienThoai) ? DBNull.Value : nhanVien.SoDienThoai;
        command.Parameters.Add("@ChucVu", SqlDbType.NVarChar, 50).Value =
            string.IsNullOrWhiteSpace(nhanVien.ChucVu) ? DBNull.Value : nhanVien.ChucVu;
    }

    private static NhanVien Map(SqlDataReader reader)
    {
        return new NhanVien
        {
            MaNV = reader.GetInt32(0),
            TenNV = reader.GetString(1),
            SoDienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
            ChucVu = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }
}
