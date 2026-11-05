using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.GUI;

public class FormPhongBan : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly PhongBanBUS _bus = new();

    private DataGridView _dgvPhongBan;
    private Label _lblMaPB;
    private TextBox _txtTenPB;
    private TextBox _txtMota;
    private bool _dangSua;

    public FormPhongBan()
    {
        InitializeUi();
        LoadData();
    }

    private void InitializeUi()
    {
        Text = "Quản lý phòng ban";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(850, 550);
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
            Text = "Danh sách phòng ban",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvPhongBan = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvPhongBan);

        _dgvPhongBan.Columns.Add(UiHelper.CreateTextColumn("Mã PB", nameof(PhongBan.MaPB), 12));
        _dgvPhongBan.Columns.Add(UiHelper.CreateTextColumn("Tên phòng ban", nameof(PhongBan.TenPB), 38));
        _dgvPhongBan.Columns.Add(UiHelper.CreateTextColumn("Mô tả", nameof(PhongBan.Mota), 50));

        _dgvPhongBan.SelectionChanged += (_, _) => FillEditorFromSelection();

        group.Controls.Add(_dgvPhongBan);
        return group;
    }

    private GroupBox BuildEditorGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin phòng ban",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        var lblId = new Label { Text = "Mã phòng ban:", Location = new Point(18, 40), AutoSize = true, Font = NormalFont };
        _lblMaPB = new Label
        {
            Text = "Tự động",
            Location = new Point(135, 40),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UiHelper.Primary
        };

        var lblTen = new Label { Text = "Tên phòng ban:", Location = new Point(250, 40), AutoSize = true, Font = NormalFont };
        _txtTenPB = new TextBox { Location = new Point(370, 36), Width = 330, Font = NormalFont };

        var lblMota = new Label { Text = "Mô tả:", Location = new Point(18, 86), AutoSize = true, Font = NormalFont };
        _txtMota = new TextBox { Location = new Point(110, 82), Width = 590, Font = NormalFont };

        var btnThem = UiHelper.CreateButton("Thêm", UiHelper.Success, 100, 36);
        btnThem.Location = new Point(110, 132);
        btnThem.Click += (_, _) => ThemPhongBan();

        var btnSua = UiHelper.CreateButton("Sửa", UiHelper.Accent, 100, 36);
        btnSua.Location = new Point(222, 132);
        btnSua.Click += (_, _) => SuaPhongBan();

        var btnXoa = UiHelper.CreateButton("Xóa", UiHelper.Danger, 100, 36);
        btnXoa.Location = new Point(334, 132);
        btnXoa.Click += (_, _) => XoaPhongBan();

        var btnLamMoi = UiHelper.CreateButton("Nhập mới", Color.FromArgb(108, 117, 125), 110, 36);
        btnLamMoi.Location = new Point(446, 132);
        btnLamMoi.Click += (_, _) => ClearEditor();

        group.Controls.Add(lblId);
        group.Controls.Add(_lblMaPB);
        group.Controls.Add(lblTen);
        group.Controls.Add(_txtTenPB);
        group.Controls.Add(lblMota);
        group.Controls.Add(_txtMota);
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
            _dgvPhongBan.DataSource = _bus.LayDanhSach();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được danh sách phòng ban");
        }
    }

    private void FillEditorFromSelection()
    {
        if (_dgvPhongBan.CurrentRow?.DataBoundItem is not PhongBan phongBan)
        {
            return;
        }

        _lblMaPB.Text = phongBan.MaPB.ToString();
        _txtTenPB.Text = phongBan.TenPB;
        _txtMota.Text = phongBan.Mota ?? string.Empty;
        _dangSua = true;
    }

    private void ClearEditor()
    {
        _lblMaPB.Text = "Tự động";
        _txtTenPB.Clear();
        _txtMota.Clear();
        _dangSua = false;
        _dgvPhongBan.ClearSelection();
        _txtTenPB.Focus();
    }

    private PhongBan DocDuLieuNhap() => new()
    {
        MaPB = _dangSua && int.TryParse(_lblMaPB.Text, out int maPB) ? maPB : 0,
        TenPB = _txtTenPB.Text.Trim(),
        Mota = _txtMota.Text.Trim()
    };

    private void ThemPhongBan()
    {
        try
        {
            _bus.Them(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Thêm phòng ban thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể thêm phòng ban");
        }
    }

    private void SuaPhongBan()
    {
        if (!_dangSua)
        {
            UiHelper.ShowError(this, "Vui lòng chọn phòng ban cần sửa trong danh sách.");
            return;
        }

        try
        {
            _bus.Sua(DocDuLieuNhap());
            UiHelper.ShowInfo(this, "Cập nhật phòng ban thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể cập nhật phòng ban");
        }
    }

    private void XoaPhongBan()
    {
        if (_dgvPhongBan.CurrentRow?.DataBoundItem is not PhongBan phongBan)
        {
            UiHelper.ShowError(this, "Vui lòng chọn phòng ban cần xóa.");
            return;
        }

        DialogResult confirm = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa phòng ban '{phongBan.TenPB}'?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bus.Xoa(phongBan.MaPB);
            UiHelper.ShowInfo(this, "Xóa phòng ban thành công.");
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể xóa phòng ban");
        }
    }
}
