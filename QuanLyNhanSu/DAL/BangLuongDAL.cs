using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.DAL;

public class BangLuongDAL
{
    /// <summary>
    /// Xem trước bảng lương theo công thức:
    /// ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + SoGioOT * 50.000
    /// </summary>
    public List<LuongNhanVien> GetPreview(string thangNam)
    {
        const string sql = @"
            SELECT nv.MaNV,
                   nv.HoTen,
                   ISNULL(pb.TenPB, N''),
                   cc.SoNgayCong,
                   cc.SoGioOT,
                   nv.LuongCoBan,
                   nv.HeSoLuong,
                   CAST((nv.LuongCoBan * nv.HeSoLuong / 22.0) * cc.SoNgayCong
                        + (cc.SoGioOT * 50000) AS DECIMAL(18,2)) AS ThucLinh
            FROM dbo.BangChamCong cc
            INNER JOIN dbo.NhanVien nv ON nv.MaNV = cc.MaNV
            LEFT JOIN dbo.PhongBan pb ON pb.MaPB = nv.MaPB
            WHERE cc.ThangNam = @ThangNam
            ORDER BY nv.HoTen;";

        var result = new List<LuongNhanVien>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new LuongNhanVien
            {
                MaNV = reader.GetInt32(0),
                HoTen = reader.GetString(1),
                TenPB = reader.GetString(2),
                SoNgayCong = reader.GetDouble(3),
                SoGioOT = reader.GetDouble(4),
                LuongCoBan = reader.GetDecimal(5),
                HeSoLuong = reader.GetDouble(6),
                ThucLinh = reader.GetDecimal(7)
            });
        }

        return result;
    }

    /// <summary>
    /// Chốt lương tháng trong một SqlTransaction: ghi BangLuong cho toàn bộ nhân viên
    /// có chấm công và khóa dữ liệu chấm công (DaChot = 1).
    /// </summary>
    public int ChotLuong(string thangNam)
    {
        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            using (var checkCommand = new SqlCommand(
                       "SELECT COUNT(*) FROM dbo.BangLuong WHERE ThangNam = @ThangNam",
                       connection,
                       transaction))
            {
                checkCommand.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;

                if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
                {
                    RollbackQuietly(transaction);
                    throw new InvalidOperationException($"Tháng {thangNam} đã được chốt lương.");
                }
            }

            int rows;

            using (var insertCommand = new SqlCommand(
                       @"INSERT INTO dbo.BangLuong (MaNV, ThangNam, ThucLinh)
                         SELECT cc.MaNV,
                                cc.ThangNam,
                                CAST((nv.LuongCoBan * nv.HeSoLuong / 22.0) * cc.SoNgayCong
                                     + (cc.SoGioOT * 50000) AS DECIMAL(18,2))
                         FROM dbo.BangChamCong cc
                         INNER JOIN dbo.NhanVien nv ON nv.MaNV = cc.MaNV
                         WHERE cc.ThangNam = @ThangNam;",
                       connection,
                       transaction))
            {
                insertCommand.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;
                rows = insertCommand.ExecuteNonQuery();
            }

            if (rows == 0)
            {
                RollbackQuietly(transaction);
                throw new InvalidOperationException(
                    $"Chưa có dữ liệu chấm công cho tháng {thangNam}, không thể chốt lương.");
            }

            using (var lockCommand = new SqlCommand(
                       "UPDATE dbo.BangChamCong SET DaChot = 1 WHERE ThangNam = @ThangNam",
                       connection,
                       transaction))
            {
                lockCommand.Parameters.Add("@ThangNam", SqlDbType.VarChar, 7).Value = thangNam;
                lockCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            return rows;
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    private static void RollbackQuietly(SqlTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
            // Transaction đã kết thúc hoặc kết nối đã đóng.
        }
    }
}
