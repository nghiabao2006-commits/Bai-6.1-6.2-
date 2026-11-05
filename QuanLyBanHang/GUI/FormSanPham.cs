using QuanLyBanHang.BUS;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.GUI;

public class FormSanPham : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly SanPhamBUS _bus = new();

    private TextBox _txtTimKiem;
    private DataGridView _dgvSanPham;
    private TextBox _txtMaSP;
    private TextBox _txtTenSP;
    private NumericUpDown _numGiaBan;
    private NumericUpDown _numSoLuongTon;
    private bool _dangSua;

    public FormSanPham()
    {
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Quản lý sản phẩm";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(1050, 640);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 175F));

        root.Controls.Add(BuildSearchGroup(), 0, 0);
        root.Controls.Add(BuildGrid(), 0, 1);
        root.Controls.Add(BuildEditorGroup(), 0, 2);

        Controls.Add(root);
    }

    private GroupBox BuildSearchGroup()
    {
        var group = new GroupBox
        {
            Text = "Tìm kiếm sản phẩm (theo mã hoặc tên)",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var label = new Label { Text = "Từ khóa:", Location = new Point(18, 34), AutoSize = true, Font = NormalFont };
        _txtTimKiem = new TextBox { Location = new Point(100, 30), Width = 360, Font = NormalFont };
        _txtTimKiem.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadData();
            }
        };

        var btnTim = UiHelper.CreateButton("Tìm kiếm", UiHelper.Accent, 110, 32);
        btnTim.Location = new Point(475, 28);
        btnTim.Click += (_, _) => LoadData();

        var btnLamMoi = UiHelper.CreateButton("Làm mới", Color.FromArgb(108, 117, 125), 110, 32);
        btnLamMoi.Location = new Point(595, 28);
        btnLamMoi.Click += (_, _) =>
        {
            _txtTimKiem.Clear();
            LoadData();
            ClearEditor();
        };

        group.Controls.Add(label);
        group.Controls.Add(_txtTimKiem);
        group.Controls.Add(btnTim);
        group.Controls.Add(btnLamMoi);

        return group;
    }

    private GroupBox BuildGrid()
    {
        var group = new GroupBox
        {
            Text = "Danh sách sản phẩm",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvSanPham = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvSanPham);

        _dgvSanPham.Columns.Add(UiHelper.CreateTextColumn("Mã sản phẩm", nameof(SanPham.MaSP), 20));
        _dgvSanPham.Columns.Add(UiHelper.CreateTextColumn("Tên sản phẩm", nameof(SanPham.TenSP), 48));
        _dgvSanPham.Columns.Add(UiHelper.CreateTextColumn("Giá bán (đ)", nameof(SanPham.GiaBan), 18, "N0", true));
        _dgvSanPham.Columns.Add(UiHelper.CreateTextColumn("Tồn kho", nameof(SanPham.SoLuongTon), 14, "N0", true));

        _dgvSanPham.SelectionChanged += (_, _) => FillEditorFromSelection();

        group.Controls.Add(_dgvSanPham);
        return group;
    }

    private GroupBox BuildEditorGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin sản phẩm",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        var lblMaSP = new Label { Text = "Mã sản phẩm:", Location = new Point(18, 36), AutoSize = true, Font = NormalFont };
        _txtMaSP = new TextBox { Location = new Point(125, 32), Width = 170, Font = NormalFont };

        var lblTenSP = new Label { Text = "Tên sản phẩm:", Location = new Point(315, 36), AutoSize = true, Font = NormalFont };
        _txtTenSP = new TextBox { Location = new Point(425, 32), Width = 330, Font = NormalFont };

        var lblGiaBan = new Label { Text = "Giá bán (đ):", Location = new Point(775, 36), AutoSize = true, Font = NormalFont };
        _numGiaBan = new NumericUpDown
        {
            Location = new Point(875, 32),
            Width = 140,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 1000000000,
            Increment = 1000,
            ThousandsSeparator = true,
            Font = NormalFont
        };

        var lblTon = new Label { Text = "Số lượng tồn:", Location = new Point(18, 86), AutoSize = true, Font = NormalFont };
        _numSoLuongTon = new NumericUpDown
        {
            Location = new Point(125, 82),
            Width = 170,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 1000000,
            ThousandsSeparator = true,
            Font = NormalFont
        };

        var btnThem = UiHelper.CreateButton("Thêm", UiHelper.Success, 100, 38);
        btnThem.Location = new Point(330, 80);
        btnThem.Click += (_, _) => ThemSanPham();

        var btnSua = UiHelper.CreateButton("Sửa", UiHelper.Accent, 100, 38);
        btnSua.Location = new Point(442, 80);
        btnSua.Click += (_, _) => SuaSanPham();

        var btnXoa = UiHelper.CreateButton("Xóa", UiHelper.Danger, 100, 38);
        btnXoa.Location = new Point(554, 80);
        btnXoa.Click += (_, _) => XoaSanPham();

        var btnLamMoi = UiHelper.CreateButton("Nhập mới", Color.FromArgb(108, 117, 125), 120, 38);
        btnLamMoi.Location = new Point(666, 80);
        btnLamMoi.Click += (_, _) => ClearEditor();

        group.Controls.Add(lblMaSP);
        group.Controls.Add(_txtMaSP);
        group.Controls.Add(lblTenSP);
        group.Controls.Add(_txtTenSP);
        group.Controls.Add(lblGiaBan);
        group.Controls.Add(_numGiaBan);
        group.Controls.Add(lblTon);
        group.Controls.Add(_numSoLuongTon);
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
            _dgvSanPham.DataSource = _bus.TimKiem(_txtTimKiem.Text);
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được danh sách sản phẩm");
        }
    }

    private void FillEditorFromSelection()
    {
        if (_dgvSanPham.CurrentRow?.DataBoundItem is not SanPham sanPham)
        {
            return;
        }

        _txtMaSP.Text = sanPham.MaSP;
        _txtTenSP.Text = sanPham.TenSP;
        _numGiaBan.Value = sanPham.GiaBan;
        _numSoLuongTon.Value = sanPham.SoLuongTon;
        _txtMaSP.ReadOnly = true;
        _dangSua = true;
    }

    private void ClearEditor()
    {
        _txtMaSP.ReadOnly = false;
        _txtMaSP.Clear();
        _txtTenSP.Clear();
        _numGiaBan.Value = 0;
        _numSoLuongTon.Value = 0;
        _dangSua = false;
        _dgvSanPham.ClearSelection();
        _txtMaSP.Focus();
    }

    private SanPham DocDuLieuNhap() => new()
    {
        MaSP = _txtMaSP.Text.Trim(),
        TenSP = _txtTenSP.Text.Trim(),
        GiaBan = _numGiaBan.Value,
        SoLuongTon = (int)_numSoLuongTon.Value
    };

    private void ThemSanPham()
    {
        try
        {
            _bus.Them(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Thêm sản phẩm thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể thêm sản phẩm");
        }
    }

    private void SuaSanPham()
    {
        if (!_dangSua)
        {
            UiHelper.ShowError(this, "Vui lòng chọn sản phẩm cần sửa trong danh sách.");
            return;
        }

        try
        {
            _bus.Sua(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Cập nhật sản phẩm thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể cập nhật sản phẩm");
        }
    }

    private void XoaSanPham()
    {
        if (_dgvSanPham.CurrentRow?.DataBoundItem is not SanPham sanPham)
        {
            UiHelper.ShowError(this, "Vui lòng chọn sản phẩm cần xóa.");
            return;
        }

        DialogResult confirm = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa sản phẩm '{sanPham.TenSP}'?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bus.Xoa(sanPham.MaSP);
            UiHelper.ShowInfo(this, "Xóa sản phẩm thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể xóa sản phẩm");
        }
    }
}
