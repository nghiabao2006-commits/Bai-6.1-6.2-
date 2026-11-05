using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.DAL;

public class PhongBanDAL
{
    public List<PhongBan> GetAll()
    {
        var result = new List<PhongBan>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(
            "SELECT MaPB, TenPB, Mota FROM dbo.PhongBan ORDER BY TenPB", connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new PhongBan
            {
                MaPB = reader.GetInt32(0),
                TenPB = reader.GetString(1),
                Mota = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }

        return result;
    }

    public int Insert(PhongBan phongBan)
    {
        const string sql = @"
            INSERT INTO dbo.PhongBan (TenPB, Mota)
            VALUES (@TenPB, @Mota);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, phongBan);

            connection.Open();
            return (int)command.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Phòng ban đã tồn tại.",
                "Không thể lưu phòng ban vì dữ liệu đang được sử dụng.");
        }
    }

    public void Update(PhongBan phongBan)
    {
        const string sql = @"
            UPDATE dbo.PhongBan
            SET TenPB = @TenPB, Mota = @Mota
            WHERE MaPB = @MaPB;";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            AddParameters(command, phongBan);
            command.Parameters.Add("@MaPB", SqlDbType.Int).Value = phongBan.MaPB;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Phòng ban đã tồn tại.",
                "Không thể cập nhật phòng ban vì dữ liệu đang được sử dụng.");
        }
    }

    public void Delete(int maPB)
    {
        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.PhongBan WHERE MaPB = @MaPB", connection);
            command.Parameters.Add("@MaPB", SqlDbType.Int).Value = maPB;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Không thể xóa phòng ban do dữ liệu bị trùng.",
                "Không thể xóa phòng ban vì vẫn còn nhân viên thuộc phòng ban này.");
        }
    }

    private static void AddParameters(SqlCommand command, PhongBan phongBan)
    {
        command.Parameters.Add("@TenPB", SqlDbType.NVarChar, 100).Value = phongBan.TenPB;
        command.Parameters.Add("@Mota", SqlDbType.NVarChar, 255).Value =
            string.IsNullOrWhiteSpace(phongBan.Mota) ? DBNull.Value : phongBan.Mota;
    }
}
