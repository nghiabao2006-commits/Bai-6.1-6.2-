using QuanLyNhanSu.DAL;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.BUS;

public class BangLuongBUS
{
    private readonly BangLuongDAL _dal = new();
    private readonly ChamCongDAL _chamCongDAL = new();

    public List<LuongNhanVien> XemTruoc(string thangNam) => _dal.GetPreview(thangNam);

    public bool DaChotLuong(string thangNam) => _chamCongDAL.IsMonthLocked(thangNam);

    public int ChotLuong(string thangNam) => _dal.ChotLuong(thangNam);
}
