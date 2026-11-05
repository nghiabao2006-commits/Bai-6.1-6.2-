using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.GUI;

public class FormLuong : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly BangLuongBUS _bus = new();

    private ComboBox _cboThang;
    private NumericUpDown _numNam;
    private Label _lblTrangThai;
    private Label _lblTongQuyLuong;
    private GroupBox _grpGrid;
    private DataGridView _dgvLuong;
    private Button _btnChot;

    private string _thangNam;
    private decimal _tongQuyLuong;
    private int _soNhanVien;

    public FormLuong()
    {
        InitializeUi();
        TaiDuLieu();
    }

    private void InitializeUi()
    {
        Text = "Tính lương - Chốt lương";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1150, 700);
        MinimumSize = new Size(1050, 640);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));

        root.Controls.Add(BuildFilterGroup(), 0, 0);
        root.Controls.Add(BuildGrid(), 0, 1);
        root.Controls.Add(BuildBottomPanel(), 0, 2);

        Controls.Add(root);
    }

    private GroupBox BuildFilterGroup()
    {
        var group = new GroupBox
        {
            Text = "Chọn tháng tính lương",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblThang = new Label { Text = "Tháng:", Location = new Point(18, 32), AutoSize = true, Font = NormalFont };
        _cboThang = new ComboBox
        {
            Location = new Point(85, 28),
            Width = 80,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };
        for (int month = 1; month <= 12; month++)
        {
            _cboThang.Items.Add(month);
        }
        _cboThang.SelectedIndex = DateTime.Today.Month - 1;

        var lblNam = new Label { Text = "Năm:", Location = new Point(190, 32), AutoSize = true, Font = NormalFont };
        _numNam = new NumericUpDown
        {
            Location = new Point(245, 28),
            Width = 90,
            Minimum = 2000,
            Maximum = 2100,
            Value = DateTime.Today.Year,
            Font = NormalFont
        };

        var btnXem = UiHelper.CreateButton("Xem trước", UiHelper.Accent, 110, 32);
        btnXem.Location = new Point(360, 26);
        btnXem.Click += (_, _) => TaiDuLieu();

        var formula = new Label
        {
            Text = "Công thức: Thực lĩnh = (Lương cơ bản × Hệ số lương / 22) × Số ngày công + Số giờ OT × 50.000",
            Location = new Point(18, 68),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.DimGray
        };

        group.Controls.Add(lblThang);
        group.Controls.Add(_cboThang);
        group.Controls.Add(lblNam);
        group.Controls.Add(_numNam);
        group.Controls.Add(btnXem);
        group.Controls.Add(formula);

        return group;
    }

    private GroupBox BuildGrid()
    {
        _grpGrid = new GroupBox
        {
            Text = "Bảng lương",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvLuong = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvLuong);

        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Mã NV", nameof(LuongNhanVien.MaNV), 8));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Họ tên", nameof(LuongNhanVien.HoTen), 22));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Phòng ban", nameof(LuongNhanVien.TenPB), 16));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Ngày công", nameof(LuongNhanVien.SoNgayCong), 10, "0.#", true));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Giờ OT", nameof(LuongNhanVien.SoGioOT), 8, "0.#", true));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Lương cơ bản (đ)", nameof(LuongNhanVien.LuongCoBan), 15, "N0", true));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Hệ số", nameof(LuongNhanVien.HeSoLuong), 7, "0.##", true));
        _dgvLuong.Columns.Add(UiHelper.CreateTextColumn("Thực lĩnh (đ)", nameof(LuongNhanVien.ThucLinh), 16, "N0", true));

        _grpGrid.Controls.Add(_dgvLuong);
        return _grpGrid;
    }

    private Panel BuildBottomPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        _lblTongQuyLuong = new Label
        {
            Text = "Tổng quỹ lương: 0 đ",
            Location = new Point(10, 22),
            AutoSize = true,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = UiHelper.Primary
        };

        _lblTrangThai = new Label
        {
            Text = string.Empty,
            Location = new Point(12, 62),
            AutoSize = true,
            Font = NormalFont,
            ForeColor = Color.DimGray
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 280,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 26, 10, 0)
        };

        _btnChot = UiHelper.CreateButton("Chốt lương tháng", UiHelper.Danger, 180, 50);
        _btnChot.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _btnChot.Click += (_, _) => ChotLuong();
        actions.Controls.Add(_btnChot);

        panel.Controls.Add(actions);
        panel.Controls.Add(_lblTrangThai);
        panel.Controls.Add(_lblTongQuyLuong);

        return panel;
    }

    private void TaiDuLieu()
    {
        try
        {
            int thang = (int)_cboThang.SelectedItem;
            int nam = (int)_numNam.Value;
            _thangNam = ChamCongBUS.FormatMonth(thang, nam);

            List<LuongNhanVien> danhSach = _bus.XemTruoc(_thangNam);
            _dgvLuong.DataSource = danhSach;

            _soNhanVien = danhSach.Count;
            _tongQuyLuong = danhSach.Sum(item => item.ThucLinh);

            _grpGrid.Text = $"Bảng lương tháng {_thangNam}";
            _lblTongQuyLuong.Text = "Tổng quỹ lương: " + UiHelper.Money(_tongQuyLuong);

            bool daChot = _bus.DaChotLuong(_thangNam);
            _btnChot.Enabled = !daChot && _soNhanVien > 0;

            if (daChot)
            {
                _btnChot.Text = "Đã chốt lương";
                _lblTrangThai.Text = $"Tháng {_thangNam} đã chốt lương. Dữ liệu chấm công của tháng đã bị khóa.";
                _lblTrangThai.ForeColor = UiHelper.Danger;
            }
            else if (_soNhanVien == 0)
            {
                _btnChot.Text = "Chốt lương tháng";
                _lblTrangThai.Text = "Chưa có dữ liệu chấm công cho tháng này.";
                _lblTrangThai.ForeColor = UiHelper.Danger;
            }
            else
            {
                _btnChot.Text = "Chốt lương tháng";
                _lblTrangThai.Text = $"Có {_soNhanVien} nhân viên trong bảng lương. Bấm Chốt lương để ghi nhận và khóa chấm công.";
                _lblTrangThai.ForeColor = Color.DimGray;
            }
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được bảng lương");
        }
    }

    private void ChotLuong()
    {
        DialogResult confirm = MessageBox.Show(
            this,
            $"Chốt lương tháng {_thangNam}?\n\n" +
            $"Số nhân viên: {_soNhanVien}\n" +
            $"Tổng quỹ lương: {UiHelper.Money(_tongQuyLuong)}\n\n" +
            "Sau khi chốt, dữ liệu chấm công của tháng sẽ bị khóa.",
            "Xác nhận chốt lương",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            int rows = _bus.ChotLuong(_thangNam);

            UiHelper.ShowInfo(
                this,
                $"Đã chốt lương tháng {_thangNam} cho {rows} nhân viên.\n" +
                $"Tổng quỹ lương: {UiHelper.Money(_tongQuyLuong)}",
                "Chốt lương thành công");

            TaiDuLieu();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể chốt lương");
        }
    }
}
