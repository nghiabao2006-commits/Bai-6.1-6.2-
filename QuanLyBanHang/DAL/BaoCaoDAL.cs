using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.DAL;

public class BaoCaoDAL
{
    public decimal GetDoanhThuHomNay()
    {
        const string sql = @"
            SELECT ISNULL(SUM(TongTien), 0)
            FROM dbo.HoaDon
            WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE);";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    public decimal GetDoanhThuThang(int thang, int nam)
    {
        DateTime tuNgay = new(nam, thang, 1);
        DateTime denNgay = tuNgay.AddMonths(1);

        const string sql = @"
            SELECT ISNULL(SUM(TongTien), 0)
            FROM dbo.HoaDon
            WHERE NgayLap >= @TuNgay AND NgayLap < @DenNgay;";

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@TuNgay", SqlDbType.DateTime).Value = tuNgay;
        command.Parameters.Add("@DenNgay", SqlDbType.DateTime).Value = denNgay;

        connection.Open();
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    /// <summary>Top 5 sản phẩm bán chạy nhất trong tháng, đọc bằng SqlDataReader.</summary>
    public List<TopSanPham> GetTop5SanPhamBanChay(int thang, int nam)
    {
        DateTime tuNgay = new(nam, thang, 1);
        DateTime denNgay = tuNgay.AddMonths(1);

        const string sql = @"
            SELECT TOP 5
                ct.MaSP,
                sp.TenSP,
                SUM(ct.SoLuong) AS SoLuongBan,
                SUM(ct.SoLuong * ct.DonGia) AS DoanhThu
            FROM dbo.ChiTietHoaDon ct
            INNER JOIN dbo.HoaDon hd ON hd.MaHD = ct.MaHD
            INNER JOIN dbo.SanPham sp ON sp.MaSP = ct.MaSP
            WHERE hd.NgayLap >= @TuNgay AND hd.NgayLap < @DenNgay
            GROUP BY ct.MaSP, sp.TenSP
            ORDER BY SoLuongBan DESC, DoanhThu DESC;";

        var result = new List<TopSanPham>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@TuNgay", SqlDbType.DateTime).Value = tuNgay;
        command.Parameters.Add("@DenNgay", SqlDbType.DateTime).Value = denNgay;

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new TopSanPham
            {
                MaSP = reader.GetString(0),
                TenSP = reader.GetString(1),
                SoLuongBan = reader.GetInt32(2),
                DoanhThu = reader.GetDecimal(3)
            });
        }

        return result;
    }
}
