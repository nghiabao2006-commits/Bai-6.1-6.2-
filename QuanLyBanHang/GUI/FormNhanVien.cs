using QuanLyBanHang.BUS;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.GUI;

public class FormNhanVien : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly NhanVienBUS _bus = new();

    private DataGridView _dgvNhanVien;
    private Label _lblMaNV;
    private TextBox _txtTenNV;
    private TextBox _txtSoDienThoai;
    private ComboBox _cboChucVu;
    private bool _dangSua;

    public FormNhanVien()
    {
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Quản lý nhân viên";
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
            Text = "Danh sách nhân viên",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvNhanVien = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvNhanVien);

        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Mã NV", nameof(NhanVien.MaNV), 12));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Tên nhân viên", nameof(NhanVien.TenNV), 38));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Số điện thoại", nameof(NhanVien.SoDienThoai), 25));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Chức vụ", nameof(NhanVien.ChucVu), 25));

        _dgvNhanVien.SelectionChanged += (_, _) => FillEditorFromSelection();

        group.Controls.Add(_dgvNhanVien);
        return group;
    }

    private GroupBox BuildEditorGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin nhân viên",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        var lblId = new Label { Text = "Mã nhân viên:", Location = new Point(18, 38), AutoSize = true, Font = NormalFont };
        _lblMaNV = new Label
        {
            Text = "Tự động",
            Location = new Point(135, 38),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UiHelper.Primary
        };

        var lblTen = new Label { Text = "Tên nhân viên:", Location = new Point(18, 82), AutoSize = true, Font = NormalFont };
        _txtTenNV = new TextBox { Location = new Point(135, 78), Width = 280, Font = NormalFont };

        var lblDienThoai = new Label { Text = "Số điện thoại:", Location = new Point(440, 82), AutoSize = true, Font = NormalFont };
        _txtSoDienThoai = new TextBox { Location = new Point(545, 78), Width = 170, Font = NormalFont };

        var lblChucVu = new Label { Text = "Chức vụ:", Location = new Point(735, 82), AutoSize = true, Font = NormalFont };
        _cboChucVu = new ComboBox
        {
            Location = new Point(815, 78),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDown,
            Font = NormalFont
        };
        _cboChucVu.Items.AddRange(new object[] { "Thu ngân", "Quản lý", "Nhân viên bán hàng" });

        var btnThem = UiHelper.CreateButton("Thêm", UiHelper.Success, 100, 36);
        btnThem.Location = new Point(135, 130);
        btnThem.Click += (_, _) => ThemNhanVien();

        var btnSua = UiHelper.CreateButton("Sửa", UiHelper.Accent, 100, 36);
        btnSua.Location = new Point(247, 130);
        btnSua.Click += (_, _) => SuaNhanVien();

        var btnXoa = UiHelper.CreateButton("Xóa", UiHelper.Danger, 100, 36);
        btnXoa.Location = new Point(359, 130);
        btnXoa.Click += (_, _) => XoaNhanVien();

        var btnLamMoi = UiHelper.CreateButton("Nhập mới", Color.FromArgb(108, 117, 125), 110, 36);
        btnLamMoi.Location = new Point(471, 130);
        btnLamMoi.Click += (_, _) => ClearEditor();

        group.Controls.Add(lblId);
        group.Controls.Add(_lblMaNV);
        group.Controls.Add(lblTen);
        group.Controls.Add(_txtTenNV);
        group.Controls.Add(lblDienThoai);
        group.Controls.Add(_txtSoDienThoai);
        group.Controls.Add(lblChucVu);
        group.Controls.Add(_cboChucVu);
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
            _dgvNhanVien.DataSource = _bus.LayDanhSach();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được danh sách nhân viên");
        }
    }

    private void FillEditorFromSelection()
    {
        if (_dgvNhanVien.CurrentRow?.DataBoundItem is not NhanVien nhanVien)
        {
            return;
        }

        _lblMaNV.Text = nhanVien.MaNV.ToString();
        _txtTenNV.Text = nhanVien.TenNV;
        _txtSoDienThoai.Text = nhanVien.SoDienThoai ?? string.Empty;
        _cboChucVu.Text = nhanVien.ChucVu ?? string.Empty;
        _dangSua = true;
    }

    private void ClearEditor()
    {
        _lblMaNV.Text = "Tự động";
        _txtTenNV.Clear();
        _txtSoDienThoai.Clear();
        _cboChucVu.Text = string.Empty;
        _dangSua = false;
        _dgvNhanVien.ClearSelection();
        _txtTenNV.Focus();
    }

    private NhanVien DocDuLieuNhap() => new()
    {
        MaNV = _dangSua && int.TryParse(_lblMaNV.Text, out int maNV) ? maNV : 0,
        TenNV = _txtTenNV.Text.Trim(),
        SoDienThoai = _txtSoDienThoai.Text.Trim(),
        ChucVu = _cboChucVu.Text.Trim()
    };

    private void ThemNhanVien()
    {
        try
        {
            _bus.Them(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Thêm nhân viên thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể thêm nhân viên");
        }
    }

    private void SuaNhanVien()
    {
        if (!_dangSua)
        {
            UiHelper.ShowError(this, "Vui lòng chọn nhân viên cần sửa trong danh sách.");
            return;
        }

        try
        {
            _bus.Sua(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Cập nhật nhân viên thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể cập nhật nhân viên");
        }
    }

    private void XoaNhanVien()
    {
        if (_dgvNhanVien.CurrentRow?.DataBoundItem is not NhanVien nhanVien)
        {
            UiHelper.ShowError(this, "Vui lòng chọn nhân viên cần xóa.");
            return;
        }

        DialogResult confirm = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa nhân viên '{nhanVien.TenNV}'?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bus.Xoa(nhanVien.MaNV);
            UiHelper.ShowInfo(this, "Xóa nhân viên thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể xóa nhân viên");
        }
    }
}
