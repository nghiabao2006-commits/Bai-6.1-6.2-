using QuanLyBanHang.BUS;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.GUI;

public class FormKhachHang : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly KhachHangBUS _bus = new();

    private DataGridView _dgvKhachHang;
    private Label _lblMaKH;
    private TextBox _txtTenKH;
    private TextBox _txtDienThoai;
    private NumericUpDown _numDiem;
    private bool _dangSua;

    public FormKhachHang()
    {
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Quản lý khách hàng";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1000, 650);
        MinimumSize = new Size(950, 600);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));

        root.Controls.Add(BuildGrid(), 0, 0);
        root.Controls.Add(BuildEditorGroup(), 0, 1);

        Controls.Add(root);
    }

    private GroupBox BuildGrid()
    {
        var group = new GroupBox
        {
            Text = "Danh sách khách hàng",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvKhachHang = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvKhachHang);

        _dgvKhachHang.Columns.Add(UiHelper.CreateTextColumn("Mã KH", nameof(KhachHang.MaKH), 12));
        _dgvKhachHang.Columns.Add(UiHelper.CreateTextColumn("Tên khách hàng", nameof(KhachHang.TenKH), 42));
        _dgvKhachHang.Columns.Add(UiHelper.CreateTextColumn("Điện thoại", nameof(KhachHang.DienThoai), 24));
        _dgvKhachHang.Columns.Add(UiHelper.CreateTextColumn("Điểm tích lũy", nameof(KhachHang.DiemTichLuy), 18, "N0", true));

        _dgvKhachHang.SelectionChanged += (_, _) => FillEditorFromSelection();

        group.Controls.Add(_dgvKhachHang);
        return group;
    }

    private GroupBox BuildEditorGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin khách hàng",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        var lblId = new Label { Text = "Mã khách hàng:", Location = new Point(18, 38), AutoSize = true, Font = NormalFont };
        _lblMaKH = new Label
        {
            Text = "Tự động",
            Location = new Point(140, 38),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UiHelper.Primary
        };

        var lblTen = new Label { Text = "Tên khách hàng:", Location = new Point(18, 82), AutoSize = true, Font = NormalFont };
        _txtTenKH = new TextBox { Location = new Point(140, 78), Width = 280, Font = NormalFont };

        var lblDienThoai = new Label { Text = "Điện thoại:", Location = new Point(445, 82), AutoSize = true, Font = NormalFont };
        _txtDienThoai = new TextBox { Location = new Point(530, 78), Width = 170, Font = NormalFont };

        var lblDiem = new Label { Text = "Điểm tích lũy:", Location = new Point(720, 82), AutoSize = true, Font = NormalFont };
        _numDiem = new NumericUpDown
        {
            Location = new Point(820, 78),
            Width = 100,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 1000000,
            ThousandsSeparator = true,
            Font = NormalFont
        };

        var btnThem = UiHelper.CreateButton("Thêm", UiHelper.Success, 100, 36);
        btnThem.Location = new Point(140, 130);
        btnThem.Click += (_, _) => ThemKhachHang();

        var btnSua = UiHelper.CreateButton("Sửa", UiHelper.Accent, 100, 36);
        btnSua.Location = new Point(252, 130);
        btnSua.Click += (_, _) => SuaKhachHang();

        var btnXoa = UiHelper.CreateButton("Xóa", UiHelper.Danger, 100, 36);
        btnXoa.Location = new Point(364, 130);
        btnXoa.Click += (_, _) => XoaKhachHang();

        var btnLamMoi = UiHelper.CreateButton("Nhập mới", Color.FromArgb(108, 117, 125), 110, 36);
        btnLamMoi.Location = new Point(476, 130);
        btnLamMoi.Click += (_, _) => ClearEditor();

        group.Controls.Add(lblId);
        group.Controls.Add(_lblMaKH);
        group.Controls.Add(lblTen);
        group.Controls.Add(_txtTenKH);
        group.Controls.Add(lblDienThoai);
        group.Controls.Add(_txtDienThoai);
        group.Controls.Add(lblDiem);
        group.Controls.Add(_numDiem);
        group.Controls.Add(btnThem);
        group.Controls.Add(btnSua);
        group.Controls.Add(btnXoa);
        group.Controls.Add(btnLamMoi);

        return group;
    }

    private void LoadData()
    {
        try
        {
            _dgvKhachHang.DataSource = _bus.LayDanhSach();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được danh sách khách hàng");
        }
    }

    private void FillEditorFromSelection()
    {
        if (_dgvKhachHang.CurrentRow?.DataBoundItem is not KhachHang khachHang)
        {
            return;
        }

        _lblMaKH.Text = khachHang.MaKH.ToString();
        _txtTenKH.Text = khachHang.TenKH;
        _txtDienThoai.Text = khachHang.DienThoai ?? string.Empty;
        _numDiem.Value = khachHang.DiemTichLuy;
        _dangSua = true;
    }

    private void ClearEditor()
    {
        _lblMaKH.Text = "Tự động";
        _txtTenKH.Clear();
        _txtDienThoai.Clear();
        _numDiem.Value = 0;
        _dangSua = false;
        _dgvKhachHang.ClearSelection();
        _txtTenKH.Focus();
    }

    private KhachHang DocDuLieuNhap() => new()
    {
        MaKH = _dangSua && int.TryParse(_lblMaKH.Text, out int maKH) ? maKH : 0,
        TenKH = _txtTenKH.Text.Trim(),
        DienThoai = _txtDienThoai.Text.Trim(),
        DiemTichLuy = (int)_numDiem.Value
    };

    private void ThemKhachHang()
    {
        try
        {
            _bus.Them(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Thêm khách hàng thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể thêm khách hàng");
        }
    }

    private void SuaKhachHang()
    {
        if (!_dangSua)
        {
            UiHelper.ShowError(this, "Vui lòng chọn khách hàng cần sửa trong danh sách.");
            return;
        }

        try
        {
            _bus.Sua(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Cập nhật khách hàng thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể cập nhật khách hàng");
        }
    }

    private void XoaKhachHang()
    {
        if (_dgvKhachHang.CurrentRow?.DataBoundItem is not KhachHang khachHang)
        {
            UiHelper.ShowError(this, "Vui lòng chọn khách hàng cần xóa.");
            return;
        }

        DialogResult confirm = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa khách hàng '{khachHang.TenKH}'?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bus.Xoa(khachHang.MaKH);
            UiHelper.ShowInfo(this, "Xóa khách hàng thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể xóa khách hàng");
        }
    }
}
