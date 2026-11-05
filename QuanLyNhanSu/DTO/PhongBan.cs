namespace QuanLyNhanSu.DTO;

public class PhongBan
{
    public int MaPB { get; set; }
    public string TenPB { get; set; }
    public string Mota { get; set; }

    public override string ToString() => TenPB;
}
