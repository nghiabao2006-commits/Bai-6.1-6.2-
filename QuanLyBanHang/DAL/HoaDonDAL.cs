using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.DAL;

public class HoaDonDAL
{
    /// <summary>
    /// Tạo hóa đơn trong một SqlTransaction: thêm HoaDon, ChiTietHoaDon,
    /// trừ tồn kho (rollback nếu thiếu hàng) và cộng điểm tích lũy.
    /// </summary>
    public int ThanhToan(int maNV, int? maKH, IReadOnlyList<ChiTietHoaDon> chiTiet, decimal tongTien, int diemCong)
    {
        using var connection = new SqlConnection(DbConnectionFactory.ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            int maHD;

            using (var command = new SqlCommand(
                       @"INSERT INTO dbo.HoaDon (MaNV, MaKH, TongTien)
                         VALUES (@MaNV, @MaKH, @TongTien);
                         SELECT CAST(SCOPE_IDENTITY() AS INT);",
                       connection,
                       transaction))
            {
                command.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNV;
                command.Parameters.Add("@MaKH", SqlDbType.Int).Value = (object)maKH ?? DBNull.Value;

                SqlParameter total = command.Parameters.Add("@TongTien", SqlDbType.Decimal);
                total.Value = tongTien;
                total.Precision = 18;
                total.Scale = 2;

                maHD = (int)command.ExecuteScalar();
            }

            foreach (ChiTietHoaDon item in chiTiet)
            {
                string tenSP;
                int soLuongTon;

                using (var command = new SqlCommand(
                           @"SELECT TenSP, SoLuongTon
                             FROM dbo.SanPham WITH (UPDLOCK, ROWLOCK)
                             WHERE MaSP = @MaSP;",
                           connection,
                           transaction))
                {
                    command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.MaSP;

                    using var reader = command.ExecuteReader();

                    if (!reader.Read())
                    {
                        RollbackQuietly(transaction);
                        throw new InvalidOperationException($"Sản phẩm '{item.MaSP}' không tồn tại.");
                    }

                    tenSP = reader.GetString(0);
                    soLuongTon = reader.GetInt32(1);
                }

                if (soLuongTon < item.SoLuong)
                {
                    RollbackQuietly(transaction);
                    throw new InvalidOperationException(
                        $"Sản phẩm '{tenSP}' chỉ còn {soLuongTon}, không đủ để bán {item.SoLuong}.");
                }

                using (var command = new SqlCommand(
                           @"INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
                             VALUES (@MaHD, @MaSP, @SoLuong, @DonGia);

                             UPDATE dbo.SanPham
                             SET SoLuongTon = SoLuongTon - @SoLuong
                             WHERE MaSP = @MaSP AND SoLuongTon >= @SoLuong;",
                           connection,
                           transaction))
                {
                    command.Parameters.Add("@MaHD", SqlDbType.Int).Value = maHD;
                    command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.MaSP;
                    command.Parameters.Add("@SoLuong", SqlDbType.Int).Value = item.SoLuong;

                    SqlParameter price = command.Parameters.Add("@DonGia", SqlDbType.Decimal);
                    price.Value = item.DonGia;
                    price.Precision = 18;
                    price.Scale = 2;

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows < 2)
                    {
                        RollbackQuietly(transaction);
                        throw new InvalidOperationException(
                            $"Không thể cập nhật tồn kho cho sản phẩm '{tenSP}'.");
                    }
                }
            }

            if (maKH.HasValue && diemCong > 0)
            {
                using var command = new SqlCommand(
                    @"UPDATE dbo.KhachHang
                      SET DiemTichLuy = DiemTichLuy + @Diem
                      WHERE MaKH = @MaKH;",
                    connection,
                    transaction);

                command.Parameters.Add("@Diem", SqlDbType.Int).Value = diemCong;
                command.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH.Value;

                if (command.ExecuteNonQuery() == 0)
                {
                    RollbackQuietly(transaction);
                    throw new InvalidOperationException("Khách hàng không tồn tại.");
                }
            }

            transaction.Commit();
            return maHD;
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
