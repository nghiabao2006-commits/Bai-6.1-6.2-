using System.Data;
using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.GUI;

public class FormChamCong : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly ChamCongBUS _bus = new();
    private readonly NhanVienBUS _nhanVienBUS = new();

    private ComboBox _cboThang;
    private NumericUpDown _numNam;
    private Label _lblTrangThai;
    private GroupBox _grpGrid;
    private DataGridView _dgvChamCong;
    private Button _btnLuu;

    private DataTable _table;
    private string _thangNam;
    private bool _daChot;
    private Dictionary<int, string> _tenNhanVien = new();

    public FormChamCong()
    {
        InitializeUi();
        TaiDuLieu();
    }

    private void InitializeUi()
    {
        Text = "Chấm công tháng";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(1000, 640);
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
            Text = "Chọn tháng chấm công",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblThang = new Label { Text = "Tháng:", Location = new Point(18, 36), AutoSize = true, Font = NormalFont };
        _cboThang = new ComboBox
        {
            Location = new Point(85, 32),
            Width = 80,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = NormalFont
        };
        for (int month = 1; month <= 12; month++)
        {
            _cboThang.Items.Add(month);
        }
        _cboThang.SelectedIndex = DateTime.Today.Month - 1;

        var lblNam = new Label { Text = "Năm:", Location = new Point(190, 36), AutoSize = true, Font = NormalFont };
        _numNam = new NumericUpDown
        {
            Location = new Point(245, 32),
            Width = 90,
            Minimum = 2000,
            Maximum = 2100,
            Value = DateTime.Today.Year,
            Font = NormalFont
        };

        var btnTai = UiHelper.CreateButton("Tải dữ liệu", UiHelper.Accent, 120, 32);
        btnTai.Location = new Point(360, 30);
        btnTai.Click += (_, _) => TaiDuLieu();

        _lblTrangThai = new Label
        {
            Location = new Point(500, 36),
            AutoSize = true,
            Font = NormalFont,
            ForeColor = Color.DimGray
        };

        group.Controls.Add(lblThang);
        group.Controls.Add(_cboThang);
        group.Controls.Add(lblNam);
        group.Controls.Add(_numNam);
        group.Controls.Add(btnTai);
        group.Controls.Add(_lblTrangThai);

        return group;
    }

    private GroupBox BuildGrid()
    {
        _grpGrid = new GroupBox
        {
            Text = "Bảng chấm công",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        _dgvChamCong = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvChamCong);
        _dgvChamCong.ReadOnly = false;

        _dgvChamCong.Columns.Add(CreateColumn("colMaNV", "Mã NV", null, 10));
        _dgvChamCong.Columns.Add(CreateColumn("colHoTen", "Họ tên", null, 30));
        _dgvChamCong.Columns.Add(CreateColumn("colSoNgayCong", "Số ngày công", "SoNgayCong", 18, "0.#", true));
        _dgvChamCong.Columns.Add(CreateColumn("colSoGioOT", "Số giờ OT", "SoGioOT", 15, "0.#", true));
        _dgvChamCong.Columns.Add(CreateColumn("colTrangThai", "Trạng thái", null, 15));

        // Chỉ hai cột số liệu được sửa; các cột còn lại chỉ để hiển thị.
        _dgvChamCong.Columns["colMaNV"].DataPropertyName = "MaNV";
        _dgvChamCong.Columns["colMaNV"].ReadOnly = true;
        _dgvChamCong.Columns["colHoTen"].ReadOnly = true;
        _dgvChamCong.Columns["colTrangThai"].ReadOnly = true;

        _dgvChamCong.CellFormatting += GridCellFormatting;
        _dgvChamCong.DataError += (_, e) =>
        {
            e.ThrowException = false;
            UiHelper.ShowError(this, "Giá trị nhập vào không hợp lệ. Vui lòng nhập số.");
        };

        _grpGrid.Controls.Add(_dgvChamCong);
        return _grpGrid;
    }

    private static DataGridViewTextBoxColumn CreateColumn(
        string name,
        string header,
        string property,
        float fillWeight,
        string format = null,
        bool alignRight = false)
    {
        DataGridViewTextBoxColumn column = UiHelper.CreateTextColumn(header, property, fillWeight, format, alignRight);
        column.Name = name;
        return column;
    }

    private Panel BuildBottomPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var note = new Label
        {
            Text = "Nhập trực tiếp số ngày công và số giờ làm thêm (OT) trên lưới, sau đó bấm Lưu chấm công.\n" +
                   "Dữ liệu được đồng bộ bằng SqlDataAdapter + SqlCommandBuilder (mô hình ngắt kết nối).",
            Location = new Point(10, 22),
            Size = new Size(650, 60),
            Font = NormalFont,
            ForeColor = Color.DimGray
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 250,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 26, 10, 0)
        };

        _btnLuu = UiHelper.CreateButton("Lưu chấm công", UiHelper.Success, 170, 50);
        _btnLuu.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _btnLuu.Click += (_, _) => LuuChamCong();
        actions.Controls.Add(_btnLuu);

        panel.Controls.Add(actions);
        panel.Controls.Add(note);

        return panel;
    }

    private void TaiDuLieu()
    {
        try
        {
            int thang = (int)_cboThang.SelectedItem;
            int nam = (int)_numNam.Value;
            _thangNam = ChamCongBUS.FormatMonth(thang, nam);

            _tenNhanVien = _nhanVienBUS.LayDanhSach()
                .ToDictionary(nhanVien => nhanVien.MaNV, nhanVien => nhanVien.HoTen);

            _table = _bus.LayBangChamCong(_thangNam);
            _daChot = _bus.DaChotLuong(_thangNam);

            _dgvChamCong.DataSource = _table;
            _dgvChamCong.ReadOnly = _daChot;

            _grpGrid.Text = $"Bảng chấm công tháng {_thangNam}";

            if (_daChot)
            {
                _lblTrangThai.Text = "Tháng này đã chốt lương - dữ liệu chấm công bị khóa.";
                _lblTrangThai.ForeColor = UiHelper.Danger;
            }
            else if (_table.Rows.Count == 0)
            {
                _lblTrangThai.Text = "Chưa có nhân viên để chấm công.";
                _lblTrangThai.ForeColor = UiHelper.Danger;
            }
            else
            {
                _lblTrangThai.Text = "Có thể sửa số ngày công và giờ OT trực tiếp trên lưới.";
                _lblTrangThai.ForeColor = Color.DimGray;
            }

            _btnLuu.Enabled = !_daChot && _table.Rows.Count > 0;
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được bảng chấm công");
        }
    }

    private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        string columnName = _dgvChamCong.Columns[e.ColumnIndex].Name;

        if (columnName == "colHoTen")
        {
            object value = _dgvChamCong.Rows[e.RowIndex].Cells["colMaNV"].Value;

            if (value != null && value != DBNull.Value &&
                _tenNhanVien.TryGetValue(Convert.ToInt32(value), out string hoTen))
            {
                e.Value = hoTen;
            }
        }
        else if (columnName == "colTrangThai")
        {
            e.Value = _daChot ? "Đã khóa" : "Chưa chốt";
        }
    }

    private void LuuChamCong()
    {
        if (_table == null)
        {
            return;
        }

        if (_daChot)
        {
            UiHelper.ShowError(this, $"Tháng {_thangNam} đã chốt lương nên không thể sửa chấm công.");
            return;
        }

        try
        {
            _dgvChamCong.EndEdit();
            BindingManagerBase manager = BindingContext[_table];
            manager?.EndCurrentEdit();

            int affectedRows = _bus.LuuChamCong(_table, _thangNam);

            UiHelper.ShowInfo(
                this,
                affectedRows == 0
                    ? "Không có thay đổi nào cần lưu."
                    : $"Đã lưu {affectedRows} dòng chấm công cho tháng {_thangNam}.",
                "Lưu chấm công");

            TaiDuLieu();
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không thể lưu chấm công");
        }
    }
}
