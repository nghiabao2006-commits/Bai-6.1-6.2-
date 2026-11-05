using QuanLyBanHang.DAL;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.BUS;

public class BanHangBUS
{
    private const decimal DiemMoiMoi100kNghinDong = 100000m;

    private readonly HoaDonDAL _hoaDonDAL = new();

    public decimal TinhTongTien(IEnumerable<CartItem> gioHang)
    {
        return gioHang.Sum(item => item.ThanhTien);
    }

    /// <summary>1 điểm cho mỗi 100.000 đồng (lấy phần nguyên).</summary>
    public int TinhDiemTichLuy(decimal tongTien)
    {
        return (int)(tongTien / DiemMoiMoi100kNghinDong);
    }

    public void ThemVaoGio(IList<CartItem> gioHang, SanPham sanPham, int soLuong)
    {
        if (sanPham == null)
        {
            throw new InvalidOperationException("Vui lòng chọn sản phẩm.");
        }

        if (soLuong <= 0)
        {
            throw new InvalidOperationException("Số lượng phải lớn hơn 0.");
        }

        CartItem existing = gioHang.FirstOrDefault(item => item.MaSP == sanPham.MaSP);
        int tongSoLuong = soLuong + (existing?.SoLuong ?? 0);

        if (tongSoLuong > sanPham.SoLuongTon)
        {
            throw new InvalidOperationException(
                $"Sản phẩm '{sanPham.TenSP}' chỉ còn {sanPham.SoLuongTon} trong kho.");
        }

        if (existing == null)
        {
            gioHang.Add(new CartItem
            {
                MaSP = sanPham.MaSP,
                TenSP = sanPham.TenSP,
                DonGia = sanPham.GiaBan,
                SoLuong = soLuong
            });
        }
        else
        {
            existing.SoLuong = tongSoLuong;
        }
    }

    /// <summary>
    /// Trả về mã hóa đơn vừa tạo. Toàn bộ thao tác nằm trong một SqlTransaction ở tầng DAL.
    /// </summary>
    public int ThanhToan(int maNV, int? maKH, IList<CartItem> gioHang)
    {
        if (maNV <= 0)
        {
            throw new InvalidOperationException("Vui lòng chọn nhân viên bán hàng.");
        }

        if (gioHang == null || gioHang.Count == 0)
        {
            throw new InvalidOperationException("Giỏ hàng đang trống, chưa có gì để thanh toán.");
        }

        foreach (CartItem item in gioHang)
        {
            if (item.SoLuong <= 0)
            {
                throw new InvalidOperationException($"Số lượng của '{item.TenSP}' phải lớn hơn 0.");
            }
        }

        decimal tongTien = TinhTongTien(gioHang);
        int diemCong = TinhDiemTichLuy(tongTien);

        var chiTiet = gioHang
            .Select(item => new ChiTietHoaDon
            {
                MaSP = item.MaSP,
                SoLuong = item.SoLuong,
                DonGia = item.DonGia
            })
            .ToList();

        return _hoaDonDAL.ThanhToan(maNV, maKH, chiTiet, tongTien, diemCong);
    }
}
