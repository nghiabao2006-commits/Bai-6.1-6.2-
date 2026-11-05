using System.Data;
using QuanLyNhanSu.DAL;

namespace QuanLyNhanSu.BUS;

public class ChamCongBUS
{
    private readonly ChamCongDAL _dal = new();

    public static string FormatMonth(int thang, int nam) => $"{thang:00}/{nam}";

    public DataTable LayBangChamCong(string thangNam) => _dal.GetTable(thangNam);

    public bool DaChotLuong(string thangNam) => _dal.IsMonthLocked(thangNam);

    public int LuuChamCong(DataTable table, string thangNam)
    {
        if (DaChotLuong(thangNam))
        {
            throw new InvalidOperationException(
                $"Tháng {thangNam} đã chốt lương nên dữ liệu chấm công bị khóa, không thể sửa.");
        }

        foreach (DataRow row in table.Rows)
        {
            if (row.RowState == DataRowState.Deleted)
            {
                continue;
            }

            double soNgayCong = row.IsNull("SoNgayCong") ? 0 : Convert.ToDouble(row["SoNgayCong"]);
            double soGioOT = row.IsNull("SoGioOT") ? 0 : Convert.ToDouble(row["SoGioOT"]);
            int maNV = Convert.ToInt32(row["MaNV"]);

            if (soNgayCong < 0 || soNgayCong > 31)
            {
                throw new InvalidOperationException(
                    $"Nhân viên mã {maNV}: số ngày công phải từ 0 đến 31.");
            }

            if (soGioOT < 0)
            {
                throw new InvalidOperationException(
                    $"Nhân viên mã {maNV}: số giờ làm thêm (OT) không được âm.");
            }
        }

        return _dal.Save(table, thangNam);
    }
}
