using QuanLyBanHang.DAL;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.BUS;

public class SanPhamBUS
{
    private readonly SanPhamDAL _dal = new();

    public List<SanPham> LayDanhSach() => _dal.GetAll();

    public List<SanPham> TimKiem(string keyword) => _dal.Search(keyword);

    public SanPham LayTheoMa(string maSP) => _dal.GetById(maSP);

    public void Them(SanPham sanPham)
    {
        Validate(sanPham);
        _dal.Insert(sanPham);
    }

    public void Sua(SanPham sanPham)
    {
        Validate(sanPham);
        _dal.Update(sanPham);
    }

    public void Xoa(string maSP) => _dal.Delete(maSP);

    private static void Validate(SanPham sanPham)
    {
        sanPham.MaSP = (sanPham.MaSP ?? string.Empty).Trim();
        sanPham.TenSP = (sanPham.TenSP ?? string.Empty).Trim();

        if (sanPham.MaSP.Length == 0)
        {
            throw new InvalidOperationException("Mã sản phẩm không được để trống.");
        }

        if (sanPham.MaSP.Length > 20)
        {
            throw new InvalidOperationException("Mã sản phẩm không được vượt quá 20 ký tự.");
        }

        if (sanPham.TenSP.Length == 0)
        {
            throw new InvalidOperationException("Tên sản phẩm không được để trống.");
        }

        if (sanPham.TenSP.Length > 150)
        {
            throw new InvalidOperationException("Tên sản phẩm không được vượt quá 150 ký tự.");
        }

        if (sanPham.GiaBan <= 0)
        {
            throw new InvalidOperationException("Giá bán phải lớn hơn 0.");
        }

        if (sanPham.SoLuongTon < 0)
        {
            throw new InvalidOperationException("Số lượng tồn phải lớn hơn hoặc bằng 0.");
        }
    }
}
