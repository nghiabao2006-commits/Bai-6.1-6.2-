using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.DAL;

public class SanPhamDAL
{
    public List<SanPham> GetAll()
    {
        return Search(string.Empty);
    }

    /// <summary>
    /// Tìm theo mã vạch (mã sản phẩm) hoặc tên, dùng SqlParameter với ký tự đại diện %.
    /// </summary>
    public List<SanPham> Search(string keyword)
    {
        var result = new List<SanPham>();

        const string sql = @"
            SELECT MaSP, TenSP, GiaBan, SoLuongTon
            FROM dbo.SanPham
            WHERE MaSP LIKE @Keyword OR TenSP LIKE @Keyword
            ORDER BY TenSP;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200).Value = "%" + (keyword ?? string.Empty).Trim() + "%";

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public SanPham GetById(string maSP)
    {
        const string sql = @"
            SELECT MaSP, TenSP, GiaBan, SoLuongTon
            FROM dbo.SanPham
            WHERE MaSP = @MaSP;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = maSP;

        connection.Open();
        using var reader = command.ExecuteReader();

        return reader.Read() ? Map(reader) : null;
    }

    public void Insert(SanPham sanPham)
    {
        const string sql = @"
            INSERT INTO dbo.SanPham (MaSP, TenSP, GiaBan, SoLuongTon)
            VALUES (@MaSP, @TenSP, @GiaBan, @SoLuongTon);";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = sanPham.MaSP;
            AddParameters(command, sanPham);

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Mã sản phẩm đã tồn tại.",
                "Không thể lưu sản phẩm vì dữ liệu đang được sử dụng.");
        }
    }

    public void Update(SanPham sanPham)
    {
        const string sql = @"
            UPDATE dbo.SanPham
            SET TenSP = @TenSP, GiaBan = @GiaBan, SoLuongTon = @SoLuongTon
            WHERE MaSP = @MaSP;";

        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = sanPham.MaSP;
            AddParameters(command, sanPham);

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Mã sản phẩm đã tồn tại.",
                "Không thể cập nhật sản phẩm vì dữ liệu đang được sử dụng.");
        }
    }

    public void Delete(string maSP)
    {
        try
        {
            using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.SanPham WHERE MaSP = @MaSP", connection);
            command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = maSP;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorHelper.ToFriendly(
                ex,
                "Không thể xóa sản phẩm do dữ liệu bị trùng.",
                "Không thể xóa sản phẩm vì sản phẩm đã có trong hóa đơn.");
        }
    }

    private static void AddParameters(SqlCommand command, SanPham sanPham)
    {
        command.Parameters.Add("@TenSP", SqlDbType.NVarChar, 150).Value = sanPham.TenSP;

        SqlParameter price = command.Parameters.Add("@GiaBan", SqlDbType.Decimal);
        price.Value = sanPham.GiaBan;
        price.Precision = 18;
        price.Scale = 2;

        command.Parameters.Add("@SoLuongTon", SqlDbType.Int).Value = sanPham.SoLuongTon;
    }

    private static SanPham Map(SqlDataReader reader)
    {
        return new SanPham
        {
            MaSP = reader.GetString(0),
            TenSP = reader.GetString(1),
            GiaBan = reader.GetDecimal(2),
            SoLuongTon = reader.GetInt32(3)
        };
    }
}
