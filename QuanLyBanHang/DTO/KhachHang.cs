namespace QuanLyBanHang.DTO;

public class KhachHang
{
    public int MaKH { get; set; }
    public string TenKH { get; set; }
    public string DienThoai { get; set; }
    public int DiemTichLuy { get; set; }

    public override string ToString() => TenKH;
}
