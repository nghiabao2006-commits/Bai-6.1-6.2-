using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.GUI;

public class FormNhanVien : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly NhanVienBUS _bus = new();
    private readonly PhongBanBUS _phongBanBUS = new();

    private TextBox _txtTimKiem;
    private ComboBox _cboLocPhongBan;
    private DataGridView _dgvNhanVien;
    private Label _lblMaNV;
    private TextBox _txtHoTen;
    private DateTimePicker _dtpNgaySinh;
    private TextBox _txtCCCD;
    private ComboBox _cboPhongBan;
    private NumericUpDown _numLuongCoBan;
    private NumericUpDown _numHeSoLuong;
    private bool _dangSua;

    public FormNhanVien()
    {
        InitializeUi();
        LoadDepartments();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Quản lý nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1150, 720);
        MinimumSize = new Size(1050, 660);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 205F));

        root.Controls.Add(BuildSearchGroup(), 0, 0);
        root.Controls.Add(BuildGrid(), 0, 1);
        root.Controls.Add(BuildEditorGroup(), 0, 2);

        Controls.Add(root);
    }

    private GroupBox BuildSearchGroup()
    {
        var group = new GroupBox
        {
            Text = "Tìm kiếm nhân viên (theo tên, CCCD hoặc phòng ban)",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblKeyword = new Label { Text = "Từ khóa:", Location = new Point(18, 34), AutoSize = true, Font = NormalFont };
        _txtTimKiem = new TextBox { Location = new Point(100, 30), Width = 240, Font = NormalFont };
        _txtTimKiem.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadData();
            }
        };

        var lblPhongBan = new Label { Text = "Phòng ban:", Location = new Point(365, 34), AutoSize = true, Font = NormalFont };
        _cboLocPhongBan = new ComboBox
        {
            Location = new Point(455, 30),
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };

        var btnTim = UiHelper.CreateButton("Tìm kiếm", UiHelper.Accent, 110, 32);
        btnTim.Location = new Point(695, 28);
        btnTim.Click += (_, _) => LoadData();

        var btnLamMoi = UiHelper.CreateButton("Làm mới", Color.FromArgb(108, 117, 125), 110, 32);
        btnLamMoi.Location = new Point(815, 28);
        btnLamMoi.Click += (_, _) =>
        {
            _txtTimKiem.Clear();
            _cboLocPhongBan.SelectedIndex = 0;
            LoadData();
            ClearEditor();
        };

        group.Controls.Add(lblKeyword);
        group.Controls.Add(_txtTimKiem);
        group.Controls.Add(lblPhongBan);
        group.Controls.Add(_cboLocPhongBan);
        group.Controls.Add(btnTim);
        group.Controls.Add(btnLamMoi);

        return group;
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

        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Mã NV", nameof(NhanVien.MaNV), 8));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Họ tên", nameof(NhanVien.HoTen), 24));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Ngày sinh", nameof(NhanVien.NgaySinh), 14, "dd/MM/yyyy"));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("CCCD", nameof(NhanVien.CCCD), 15));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Phòng ban", nameof(NhanVien.TenPB), 18));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Lương cơ bản (đ)", nameof(NhanVien.LuongCoBan), 17, "N0", true));
        _dgvNhanVien.Columns.Add(UiHelper.CreateTextColumn("Hệ số", nameof(NhanVien.HeSoLuong), 8, "0.##", true));

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

        _lblMaNV = new Label
        {
            Text = "Mã nhân viên: Tự động",
            Location = new Point(880, 36),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UiHelper.Primary
        };

        var lblHoTen = new Label { Text = "Họ tên:", Location = new Point(18, 38), AutoSize = true, Font = NormalFont };
        _txtHoTen = new TextBox { Location = new Point(100, 34), Width = 250, Font = NormalFont };

        var lblNgaySinh = new Label { Text = "Ngày sinh:", Location = new Point(370, 38), AutoSize = true, Font = NormalFont };
        _dtpNgaySinh = new DateTimePicker
        {
            Location = new Point(455, 34),
            Width = 150,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            ShowCheckBox = true,
            Checked = false,
            Font = NormalFont
        };

        var lblCCCD = new Label { Text = "Số CCCD:", Location = new Point(625, 38), AutoSize = true, Font = NormalFont };
        _txtCCCD = new TextBox { Location = new Point(700, 34), Width = 160, Font = NormalFont };

        var lblPhongBan = new Label { Text = "Phòng ban:", Location = new Point(18, 84), AutoSize = true, Font = NormalFont };
        _cboPhongBan = new ComboBox
        {
            Location = new Point(100, 80),
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };

        var lblLuong = new Label { Text = "Lương cơ bản (đ):", Location = new Point(340, 84), AutoSize = true, Font = NormalFont };
        _numLuongCoBan = new NumericUpDown
        {
            Location = new Point(455, 80),
            Width = 150,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 1000000000,
            Increment = 500000,
            ThousandsSeparator = true,
            Font = NormalFont
        };

        var lblHeSo = new Label { Text = "Hệ số lương:", Location = new Point(625, 84), AutoSize = true, Font = NormalFont };
        _numHeSoLuong = new NumericUpDown
        {
            Location = new Point(720, 80),
            Width = 100,
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 100,
            Increment = 0.1m,
            Value = 1.0m,
            Font = NormalFont
        };

        var btnThem = UiHelper.CreateButton("Thêm", UiHelper.Success, 100, 36);
        btnThem.Location = new Point(100, 133);
        btnThem.Click += (_, _) => ThemNhanVien();

        var btnSua = UiHelper.CreateButton("Sửa", UiHelper.Accent, 100, 36);
        btnSua.Location = new Point(212, 133);
        btnSua.Click += (_, _) => SuaNhanVien();

        var btnXoa = UiHelper.CreateButton("Xóa", UiHelper.Danger, 100, 36);
        btnXoa.Location = new Point(324, 133);
        btnXoa.Click += (_, _) => XoaNhanVien();

        var btnLamMoi = UiHelper.CreateButton("Nhập mới", Color.FromArgb(108, 117, 125), 110, 36);
        btnLamMoi.Location = new Point(436, 133);
        btnLamMoi.Click += (_, _) => ClearEditor();

        group.Controls.Add(_lblMaNV);
        group.Controls.Add(lblHoTen);
        group.Controls.Add(_txtHoTen);
        group.Controls.Add(lblNgaySinh);
        group.Controls.Add(_dtpNgaySinh);
        group.Controls.Add(lblCCCD);
        group.Controls.Add(_txtCCCD);
        group.Controls.Add(lblPhongBan);
        group.Controls.Add(_cboPhongBan);
        group.Controls.Add(lblLuong);
        group.Controls.Add(_numLuongCoBan);
        group.Controls.Add(lblHeSo);
        group.Controls.Add(_numHeSoLuong);
        group.Controls.Add(btnThem);
        group.Controls.Add(btnSua);
        group.Controls.Add(btnXoa);
        group.Controls.Add(btnLamMoi);

        return group;
    }

    private void LoadDepartments()
    {
        try
        {
            var departments = _phongBanBUS.LayDanhSach();
            departments.Insert(0, new PhongBan { MaPB = 0, TenPB = "Chưa phân phòng", Mota = null });

            _cboPhongBan.DataSource = departments;
            _cboPhongBan.DisplayMember = nameof(PhongBan.TenPB);
            _cboPhongBan.ValueMember = nameof(PhongBan.MaPB);

            var filter = new List<PhongBan> { new() { MaPB = 0, TenPB = "Tất cả phòng ban" } };
            filter.AddRange(departments.Where(department => department.MaPB > 0));

            _cboLocPhongBan.DataSource = filter;
            _cboLocPhongBan.DisplayMember = nameof(PhongBan.TenPB);
            _cboLocPhongBan.ValueMember = nameof(PhongBan.MaPB);
            _cboLocPhongBan.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được danh sách phòng ban");
        }
    }

    private void LoadData()
    {
        try
        {
            int? maPB = _cboLocPhongBan.SelectedItem is PhongBan phongBan && phongBan.MaPB > 0
                ? phongBan.MaPB
                : null;

            _dgvNhanVien.DataSource = _bus.TimKiem(_txtTimKiem.Text, maPB);
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

        _lblMaNV.Text = "Mã nhân viên: " + nhanVien.MaNV;
        _txtHoTen.Text = nhanVien.HoTen;
        _txtCCCD.Text = nhanVien.CCCD ?? string.Empty;
        _numLuongCoBan.Value = nhanVien.LuongCoBan;
        _numHeSoLuong.Value = (decimal)nhanVien.HeSoLuong;

        if (nhanVien.NgaySinh.HasValue)
        {
            _dtpNgaySinh.Value = nhanVien.NgaySinh.Value;
            _dtpNgaySinh.Checked = true;
        }
        else
        {
            _dtpNgaySinh.Checked = false;
        }

        _cboPhongBan.SelectedValue = nhanVien.MaPB ?? 0;
        _dangSua = true;
    }

    private void ClearEditor()
    {
        _lblMaNV.Text = "Mã nhân viên: Tự động";
        _txtHoTen.Clear();
        _txtCCCD.Clear();
        _dtpNgaySinh.Checked = false;
        _cboPhongBan.SelectedIndex = 0;
        _numLuongCoBan.Value = 0;
        _numHeSoLuong.Value = 1.0m;
        _dangSua = false;
        _dgvNhanVien.ClearSelection();
        _txtHoTen.Focus();
    }

    private NhanVien DocDuLieuNhap() => new()
    {
        MaNV = _dangSua && int.TryParse(_lblMaNV.Text.Replace("Mã nhân viên:", string.Empty).Trim(), out int maNV)
            ? maNV
            : 0,
        HoTen = _txtHoTen.Text.Trim(),
        NgaySinh = _dtpNgaySinh.Checked ? _dtpNgaySinh.Value.Date : null,
        CCCD = _txtCCCD.Text.Trim(),
        MaPB = _cboPhongBan.SelectedItem is PhongBan phongBan && phongBan.MaPB > 0 ? phongBan.MaPB : null,
        LuongCoBan = _numLuongCoBan.Value,
        HeSoLuong = (double)_numHeSoLuong.Value
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
            $"Bạn có chắc muốn xóa nhân viên '{nhanVien.HoTen}'?",
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
