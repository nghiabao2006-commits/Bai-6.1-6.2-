using QuanLyNhanSu.DAL;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.BUS;

public class NhanVienBUS
{
    private readonly NhanVienDAL _dal = new();

    public List<NhanVien> LayDanhSach() => _dal.GetAll();

    public List<NhanVien> TimKiem(string keyword, int? maPB) => _dal.Search(keyword, maPB);

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
        nhanVien.HoTen = (nhanVien.HoTen ?? string.Empty).Trim();
        nhanVien.CCCD = string.IsNullOrWhiteSpace(nhanVien.CCCD) ? null : nhanVien.CCCD.Trim();

        if (nhanVien.HoTen.Length == 0)
        {
            throw new InvalidOperationException("Họ tên nhân viên không được để trống.");
        }

        if (nhanVien.HoTen.Length > 100)
        {
            throw new InvalidOperationException("Họ tên không được vượt quá 100 ký tự.");
        }

        if (nhanVien.CCCD != null &&
            (!nhanVien.CCCD.All(char.IsDigit) || nhanVien.CCCD.Length > 20))
        {
            throw new InvalidOperationException("Số CCCD chỉ gồm chữ số và tối đa 20 ký tự.");
        }

        if (nhanVien.NgaySinh.HasValue && nhanVien.NgaySinh.Value.Date > DateTime.Today)
        {
            throw new InvalidOperationException("Ngày sinh không được lớn hơn ngày hiện tại.");
        }

        if (nhanVien.LuongCoBan <= 0)
        {
            throw new InvalidOperationException("Lương cơ bản phải lớn hơn 0.");
        }

        if (nhanVien.HeSoLuong <= 0)
        {
            throw new InvalidOperationException("Hệ số lương phải lớn hơn 0.");
        }
    }
}
