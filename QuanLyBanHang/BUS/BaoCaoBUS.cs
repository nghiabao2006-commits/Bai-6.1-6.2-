using QuanLyBanHang.DAL;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.BUS;

public class BaoCaoBUS
{
    private readonly BaoCaoDAL _dal = new();

    public decimal LayDoanhThuHomNay() => _dal.GetDoanhThuHomNay();

    public decimal LayDoanhThuThang(int thang, int nam) => _dal.GetDoanhThuThang(thang, nam);

    public List<TopSanPham> LayTop5SanPhamBanChay(int thang, int nam) =>
        _dal.GetTop5SanPhamBanChay(thang, nam);
}
