using System.ComponentModel;
using QuanLyBanHang.BUS;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.GUI;

public class FormBanHang : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly NhanVienBUS _nhanVienBUS = new();
    private readonly KhachHangBUS _khachHangBUS = new();
    private readonly SanPhamBUS _sanPhamBUS = new();
    private readonly BanHangBUS _banHangBUS = new();
    private readonly BindingList<CartItem> _gioHang = new();

    private ComboBox _cboNhanVien;
    private ComboBox _cboKhachHang;
    private Label _lblThongTinKhach;
    private TextBox _txtMaSP;
    private Label _lblTenSP;
    private NumericUpDown _numSoLuong;
    private DataGridView _dgvGioHang;
    private Label _lblTongTien;
    private Label _lblDiem;
    private bool _dangTaiDuLieu;

    public FormBanHang()
    {
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Bán hàng - Lập hóa đơn";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(1000, 640);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));

        root.Controls.Add(BuildInvoiceInfoGroup(), 0, 0);
        root.Controls.Add(BuildProductGroup(), 0, 1);
        root.Controls.Add(BuildCartGroup(), 0, 2);
        root.Controls.Add(BuildBottomPanel(), 0, 3);

        Controls.Add(root);
    }

    private GroupBox BuildInvoiceInfoGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin hóa đơn",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblNhanVien = new Label { Text = "Nhân viên:", Location = new Point(18, 36), AutoSize = true, Font = NormalFont };
        _cboNhanVien = new ComboBox
        {
            Location = new Point(110, 32),
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };

        var lblKhachHang = new Label { Text = "Khách hàng:", Location = new Point(390, 36), AutoSize = true, Font = NormalFont };
        _cboKhachHang = new ComboBox
        {
            Location = new Point(495, 32),
            Width = 280,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };
        _cboKhachHang.SelectedIndexChanged += CustomerChanged;

        _lblThongTinKhach = new Label
        {
            Location = new Point(795, 36),
            AutoSize = true,
            Font = NormalFont,
            ForeColor = UiHelper.Primary
        };

        group.Controls.Add(lblNhanVien);
        group.Controls.Add(_cboNhanVien);
        group.Controls.Add(lblKhachHang);
        group.Controls.Add(_cboKhachHang);
        group.Controls.Add(_lblThongTinKhach);

        return group;
    }

    private GroupBox BuildProductGroup()
    {
        var group = new GroupBox
        {
            Text = "Thêm sản phẩm vào giỏ",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblMaSP = new Label { Text = "Mã vạch / Mã SP:", Location = new Point(18, 30), AutoSize = true, Font = NormalFont };
        _txtMaSP = new TextBox { Location = new Point(150, 26), Width = 250, Font = new Font("Segoe UI", 10F) };
        _txtMaSP.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ThemSanPham();
            }
        };

        var btnTim = UiHelper.CreateButton("Tìm", UiHelper.Accent, 80, 30);
        btnTim.Location = new Point(412, 24);
        btnTim.Click += (_, _) => TimSanPham();

        _lblTenSP = new Label
        {
            Text = "Tên sản phẩm: ...",
            Location = new Point(18, 68),
            Width = 660,
            AutoEllipsis = true,
            Font = NormalFont,
            ForeColor = Color.DimGray
        };

        var lblSoLuong = new Label { Text = "Số lượng:", Location = new Point(700, 68), AutoSize = true, Font = NormalFont };
        _numSoLuong = new NumericUpDown
        {
            Location = new Point(780, 64),
            Width = 80,
            Minimum = 1,
            Maximum = 10000,
            Value = 1,
            Font = NormalFont
        };

        var btnThem = UiHelper.CreateButton("Thêm vào giỏ", UiHelper.Success, 150, 34);
        btnThem.Location = new Point(880, 60);
        btnThem.Click += (_, _) => ThemSanPham();

        group.Controls.Add(lblMaSP);
        group.Controls.Add(_txtMaSP);
        group.Controls.Add(btnTim);
        group.Controls.Add(_lblTenSP);
        group.Controls.Add(lblSoLuong);
        group.Controls.Add(_numSoLuong);
        group.Controls.Add(btnThem);

        return group;
    }

    private GroupBox BuildCartGroup()
    {
        var group = new GroupBox
        {
            Text = "Giỏ hàng",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        _dgvGioHang = new DataGridView { Dock = DockStyle.Fill, Margin = new Padding(0), ReadOnly = true };
        UiHelper.StyleGrid(_dgvGioHang);

        _dgvGioHang.Columns.Add(UiHelper.CreateTextColumn("Mã SP", nameof(CartItem.MaSP), 16));
        _dgvGioHang.Columns.Add(UiHelper.CreateTextColumn("Tên sản phẩm", nameof(CartItem.TenSP), 44));
        _dgvGioHang.Columns.Add(UiHelper.CreateTextColumn("Đơn giá (đ)", nameof(CartItem.DonGia), 16, "N0", true));
        _dgvGioHang.Columns.Add(UiHelper.CreateTextColumn("Số lượng", nameof(CartItem.SoLuong), 10, null, true));
        _dgvGioHang.Columns.Add(UiHelper.CreateTextColumn("Thành tiền (đ)", nameof(CartItem.ThanhTien), 20, "N0", true));

        _dgvGioHang.DataSource = _gioHang;

        group.Controls.Add(_dgvGioHang);
        return group;
    }

    private Panel BuildBottomPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        _lblTongTien = new Label
        {
            Text = "Tổng tiền: 0 đ",
            Location = new Point(10, 18),
            AutoSize = true,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = UiHelper.Danger
        };

        _lblDiem = new Label
        {
            Text = "Điểm tích lũy được: 0 điểm",
            Location = new Point(12, 62),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = Color.DimGray
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 530,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 20, 10, 0)
        };

        var btnThanhToan = UiHelper.CreateButton("Thanh toán", UiHelper.Success, 170, 52);
        btnThanhToan.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
        btnThanhToan.Click += (_, _) => ThanhToan();

        var btnXoaGio = UiHelper.CreateButton("Xóa giỏ hàng", UiHelper.Danger, 140, 44);
        btnXoaGio.Click += (_, _) =>
        {
            if (_gioHang.Count == 0)
            {
                return;
            }

            if (MessageBox.Show(this, "Xóa toàn bộ giỏ hàng?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _gioHang.Clear();
                UpdateTotals();
            }
        };

        var btnXoaDong = UiHelper.CreateButton("Xóa dòng", Color.FromArgb(108, 117, 125), 130, 44);
        btnXoaDong.Click += (_, _) => XoaDongDangChon();

        actions.Controls.Add(btnThanhToan);
        actions.Controls.Add(btnXoaGio);
        actions.Controls.Add(btnXoaDong);

        panel.Controls.Add(actions);
        panel.Controls.Add(_lblDiem);
        panel.Controls.Add(_lblTongTien);

        return panel;
    }

    private void LoadData()
    {
        try
        {
            var nhanViens = _nhanVienBUS.LayDanhSach();
            _cboNhanVien.DataSource = nhanViens;
            _cboNhanVien.DisplayMember = nameof(NhanVien.TenNV);
            _cboNhanVien.ValueMember = nameof(NhanVien.MaNV);
            _cboNhanVien.SelectedIndex = nhanViens.Count > 0 ? 0 : -1;

            LoadCustomers();
            UpdateTotals();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được dữ liệu");
        }
    }

    private void LoadCustomers()
    {
        _dangTaiDuLieu = true;

        try
        {
            var khachHangs = _khachHangBUS.LayDanhSach();
            khachHangs.Insert(0, new KhachHang
            {
                MaKH = 0,
                TenKH = "Khách lẻ",
                DienThoai = null,
                DiemTichLuy = 0
            });

            _cboKhachHang.DataSource = null;
            _cboKhachHang.DataSource = khachHangs;
            _cboKhachHang.DisplayMember = nameof(KhachHang.TenKH);
            _cboKhachHang.ValueMember = nameof(KhachHang.MaKH);
            _cboKhachHang.SelectedIndex = 0;
        }
        finally
        {
            _dangTaiDuLieu = false;
        }

        CapNhatThongTinKhachHang();
    }

    private void CustomerChanged(object sender, EventArgs e)
    {
        if (!_dangTaiDuLieu)
        {
            CapNhatThongTinKhachHang();
        }
    }

    private void CapNhatThongTinKhachHang()
    {
        if (_cboKhachHang.SelectedItem is KhachHang khachHang)
        {
            _lblThongTinKhach.Text = khachHang.MaKH == 0
                ? "Khách lẻ (không tích điểm)"
                : $"Điểm hiện có: {khachHang.DiemTichLuy}";
        }
    }

    private void TimSanPham()
    {
        string code = _txtMaSP.Text.Trim();

        if (code.Length == 0)
        {
            UiHelper.ShowError(this, "Vui lòng nhập hoặc quét mã sản phẩm.");
            _txtMaSP.Focus();
            return;
        }

        try
        {
            SanPham sanPham = _sanPhamBUS.LayTheoMa(code);

            if (sanPham == null)
            {
                UiHelper.ShowError(this, $"Không tìm thấy sản phẩm có mã '{code}'.");
                _txtMaSP.SelectAll();
                _txtMaSP.Focus();
                return;
            }

            ShowProductInfo(sanPham);
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message);
        }
    }

    private void ThemSanPham()
    {
        string code = _txtMaSP.Text.Trim();

        if (code.Length == 0)
        {
            UiHelper.ShowError(this, "Vui lòng nhập hoặc quét mã sản phẩm.");
            _txtMaSP.Focus();
            return;
        }

        try
        {
            SanPham sanPham = _sanPhamBUS.LayTheoMa(code);

            if (sanPham == null)
            {
                UiHelper.ShowError(this, $"Không tìm thấy sản phẩm có mã '{code}'.");
                _txtMaSP.SelectAll();
                _txtMaSP.Focus();
                return;
            }

            _banHangBUS.ThemVaoGio(_gioHang, sanPham, (int)_numSoLuong.Value);
            ShowProductInfo(sanPham);

            _txtMaSP.Clear();
            _numSoLuong.Value = 1;
            _txtMaSP.Focus();
            UpdateTotals();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể thêm sản phẩm");
        }
    }

    private void ShowProductInfo(SanPham sanPham)
    {
        _lblTenSP.Text =
            $"Tên sản phẩm: {sanPham.TenSP}   |   Giá: {UiHelper.Money(sanPham.GiaBan)}   |   Tồn: {sanPham.SoLuongTon}";
    }

    private void XoaDongDangChon()
    {
        if (_dgvGioHang.CurrentRow?.DataBoundItem is CartItem item)
        {
            _gioHang.Remove(item);
            UpdateTotals();
        }
    }

    private void UpdateTotals()
    {
        decimal tongTien = _banHangBUS.TinhTongTien(_gioHang);
        int diem = _banHangBUS.TinhDiemTichLuy(tongTien);

        _lblTongTien.Text = "Tổng tiền: " + UiHelper.Money(tongTien);
        _lblDiem.Text = $"Điểm tích lũy được: {diem} điểm";
    }

    private void ThanhToan()
    {
        if (_gioHang.Count == 0)
        {
            UiHelper.ShowError(this, "Giỏ hàng đang trống, chưa có gì để thanh toán.");
            return;
        }

        if (_cboNhanVien.SelectedItem is not NhanVien nhanVien)
        {
            UiHelper.ShowError(this, "Vui lòng chọn nhân viên bán hàng.");
            return;
        }

        KhachHang khachHang = _cboKhachHang.SelectedItem as KhachHang;
        int? maKH = khachHang != null && khachHang.MaKH > 0 ? khachHang.MaKH : null;
        string tenKhach = maKH.HasValue ? khachHang.TenKH : "Khách lẻ";

        decimal tongTien = _banHangBUS.TinhTongTien(_gioHang);
        int diem = _banHangBUS.TinhDiemTichLuy(tongTien);

        DialogResult confirm = MessageBox.Show(
            this,
            $"Xác nhận thanh toán?\n\n" +
            $"Khách hàng: {tenKhach}\n" +
            $"Tổng tiền: {UiHelper.Money(tongTien)}\n" +
            $"Điểm tích lũy cộng thêm: {diem}",
            "Xác nhận thanh toán",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            int maHD = _banHangBUS.ThanhToan(nhanVien.MaNV, maKH, _gioHang);

            UiHelper.ShowInfo(
                this,
                $"Thanh toán thành công!\n\n" +
                $"Mã hóa đơn: {maHD}\n" +
                $"Tổng tiền: {UiHelper.Money(tongTien)}\n" +
                $"Điểm tích lũy cộng thêm: {diem}",
                "Thanh toán thành công");

            _gioHang.Clear();
            UpdateTotals();
            LoadCustomers();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Thanh toán thất bại");
        }
    }
}
