using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.DAL;

public class NhanVienDAL
{
    public List<NhanVien> GetAll() => Search(string.Empty, null);

    /// <summary>
    /// Tìm theo tên, CCCD hoặc tên phòng ban; lọc thêm theo phòng ban.
    /// Toàn bộ tham số dùng SqlParameter, không nối chuỗi SQL.
    /// </summary>
    public List<NhanVien> Search(string keyword, int? maPB)
    {
        const string sql = @"
            SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD,
                   nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong
            FROM dbo.NhanVien nv
            LEFT JOIN dbo.PhongBan pb ON pb.MaPB = nv.MaPB
            WHERE (nv.HoTen LIKE @Keyword OR nv.CCCD LIKE @Keyword OR pb.TenPB LIKE @Keyword)
              AND (@MaPB IS NULL OR nv.MaPB = @MaPB)
            ORDER BY nv.HoTen;";

        var result = new List<NhanVien>();

        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200).Value =
            "%" + (keyword ?? string.Empty).Trim() + "%";
        command.Parameters.Add("@MaPB", SqlDbType.Int).Value = (object)maPB ?? DBNull.Value;

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
            INSERT INTO dbo.NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong)
            VALUES (@HoTen, @NgaySinh, @CCCD, @MaPB, @LuongCoBan, @HeSoLuong);
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
                "Số CCCD của nhân viên đã tồn tại.",
                "Không thể lưu nhân viên vì dữ liệu đang được sử dụng.");
        }
    }

    public void Update(NhanVien nhanVien)
    {
        const string sql = @"
            UPDATE dbo.NhanVien
            SET HoTen = @HoTen,
                NgaySinh = @NgaySinh,
                CCCD = @CCCD,
                MaPB = @MaPB,
                LuongCoBan = @LuongCoBan,
                HeSoLuong = @HeSoLuong
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
                "Số CCCD của nhân viên đã tồn tại.",
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
                "Không thể xóa nhân viên vì nhân viên đã có dữ liệu chấm công hoặc bảng lương.");
        }
    }

    private static void AddParameters(SqlCommand command, NhanVien nhanVien)
    {
        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = nhanVien.HoTen;
        command.Parameters.Add("@NgaySinh", SqlDbType.DateTime).Value =
            (object)nhanVien.NgaySinh ?? DBNull.Value;
        command.Parameters.Add("@CCCD", SqlDbType.VarChar, 20).Value =
            string.IsNullOrWhiteSpace(nhanVien.CCCD) ? DBNull.Value : nhanVien.CCCD;
        command.Parameters.Add("@MaPB", SqlDbType.Int).Value =
            nhanVien.MaPB.HasValue && nhanVien.MaPB.Value > 0 ? nhanVien.MaPB.Value : DBNull.Value;

        SqlParameter salary = command.Parameters.Add("@LuongCoBan", SqlDbType.Decimal);
        salary.Value = nhanVien.LuongCoBan;
        salary.Precision = 18;
        salary.Scale = 2;

        command.Parameters.Add("@HeSoLuong", SqlDbType.Float).Value = nhanVien.HeSoLuong;
    }

    private static NhanVien Map(SqlDataReader reader)
    {
        return new NhanVien
        {
            MaNV = reader.GetInt32(0),
            HoTen = reader.GetString(1),
            NgaySinh = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
            CCCD = reader.IsDBNull(3) ? null : reader.GetString(3),
            MaPB = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            TenPB = reader.IsDBNull(5) ? null : reader.GetString(5),
            LuongCoBan = reader.GetDecimal(6),
            HeSoLuong = reader.GetDouble(7)
        };
    }
}
