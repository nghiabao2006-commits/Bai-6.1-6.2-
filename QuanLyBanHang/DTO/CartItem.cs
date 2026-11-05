using System.ComponentModel;

namespace QuanLyBanHang.DTO;

public class CartItem : INotifyPropertyChanged
{
    private int _soLuong;

    public string MaSP { get; set; }
    public string TenSP { get; set; }
    public decimal DonGia { get; set; }

    public int SoLuong
    {
        get => _soLuong;
        set
        {
            if (_soLuong == value)
            {
                return;
            }

            _soLuong = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoLuong)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThanhTien)));
        }
    }

    public decimal ThanhTien => DonGia * SoLuong;

    public event PropertyChangedEventHandler PropertyChanged;
}
