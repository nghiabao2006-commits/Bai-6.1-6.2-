using QuanLyBanHang.DAL;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.BUS;

public class NhanVienBUS
{
    private readonly NhanVienDAL _dal = new();

    public List<NhanVien> LayDanhSach() => _dal.GetAll();

    public int Them(NhanVien nhanVien)
    {
        Validate(nhanVien);
        return _dal.Insert(nhanVien);
    }

    public void Sua(NhanVien nhanVien)
    {
        Validate(nhanVien);
        _dal.Update(nhanVien);
    }

    public void Xoa(int maNV) => _dal.Delete(maNV);

    private static void Validate(NhanVien nhanVien)
    {
        nhanVien.TenNV = (nhanVien.TenNV ?? string.Empty).Trim();
        nhanVien.SoDienThoai = string.IsNullOrWhiteSpace(nhanVien.SoDienThoai) ? null : nhanVien.SoDienThoai.Trim();
        nhanVien.ChucVu = string.IsNullOrWhiteSpace(nhanVien.ChucVu) ? null : nhanVien.ChucVu.Trim();

        if (nhanVien.TenNV.Length == 0)
        {
            throw new InvalidOperationException("Tên nhân viên không được để trống.");
        }

        if (nhanVien.TenNV.Length > 100)
        {
            throw new InvalidOperationException("Tên nhân viên không được vượt quá 100 ký tự.");
        }

        if (nhanVien.SoDienThoai != null &&
            (!nhanVien.SoDienThoai.All(char.IsDigit) || nhanVien.SoDienThoai.Length > 15))
        {
            throw new InvalidOperationException("Số điện thoại chỉ gồm chữ số và tối đa 15 ký tự.");
        }
    }
}
