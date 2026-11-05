using QuanLyNhanSu.DAL;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.BUS;

public class PhongBanBUS
{
    private readonly PhongBanDAL _dal = new();

    public List<PhongBan> LayDanhSach() => _dal.GetAll();

    public int Them(PhongBan phongBan)
    {
        Validate(phongBan);
        return _dal.Insert(phongBan);
    }

    public void Sua(PhongBan phongBan)
    {
        Validate(phongBan);
        _dal.Update(phongBan);
    }

    public void Xoa(int maPB) => _dal.Delete(maPB);

    private static void Validate(PhongBan phongBan)
    {
        phongBan.TenPB = (phongBan.TenPB ?? string.Empty).Trim();
        phongBan.Mota = string.IsNullOrWhiteSpace(phongBan.Mota) ? null : phongBan.Mota.Trim();

        if (phongBan.TenPB.Length == 0)
        {
            throw new InvalidOperationException("Tên phòng ban không được để trống.");
        }

        if (phongBan.TenPB.Length > 100)
        {
            throw new InvalidOperationException("Tên phòng ban không được vượt quá 100 ký tự.");
        }

        if (phongBan.Mota != null && phongBan.Mota.Length > 255)
        {
            throw new InvalidOperationException("Mô tả không được vượt quá 255 ký tự.");
        }
    }
}
