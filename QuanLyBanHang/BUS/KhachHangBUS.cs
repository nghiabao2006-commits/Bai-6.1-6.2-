using QuanLyBanHang.DAL;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.BUS;

public class KhachHangBUS
{
    private readonly KhachHangDAL _dal = new();

    public List<KhachHang> LayDanhSach() => _dal.GetAll();

    public int Them(KhachHang khachHang)
    {
        Validate(khachHang);
        return _dal.Insert(khachHang);
    }

    public void Sua(KhachHang khachHang)
    {
        Validate(khachHang);
        _dal.Update(khachHang);
    }

    public void Xoa(int maKH) => _dal.Delete(maKH);

    private static void Validate(KhachHang khachHang)
    {
        khachHang.TenKH = (khachHang.TenKH ?? string.Empty).Trim();
        khachHang.DienThoai = string.IsNullOrWhiteSpace(khachHang.DienThoai) ? null : khachHang.DienThoai.Trim();

        if (khachHang.TenKH.Length == 0)
        {
            throw new InvalidOperationException("Tên khách hàng không được để trống.");
        }

        if (khachHang.TenKH.Length > 100)
        {
            throw new InvalidOperationException("Tên khách hàng không được vượt quá 100 ký tự.");
        }

        if (khachHang.DienThoai != null &&
            (!khachHang.DienThoai.All(char.IsDigit) || khachHang.DienThoai.Length > 15))
        {
            throw new InvalidOperationException("Điện thoại chỉ gồm chữ số và tối đa 15 ký tự.");
        }

        if (khachHang.DiemTichLuy < 0)
        {
            throw new InvalidOperationException("Điểm tích lũy không được âm.");
        }
    }
}
