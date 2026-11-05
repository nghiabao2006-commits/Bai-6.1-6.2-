namespace QuanLyBanHang.DTO;

public class NhanVien
{
    public int MaNV { get; set; }
    public string TenNV { get; set; }
    public string SoDienThoai { get; set; }
    public string ChucVu { get; set; }

    public override string ToString() => TenNV;
}
