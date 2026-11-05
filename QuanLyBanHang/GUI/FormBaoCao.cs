using QuanLyBanHang.BUS;
using QuanLyBanHang.DTO;

namespace QuanLyBanHang.GUI;

public class FormBaoCao : Form
{
    private static readonly Font NormalFont = new("Segoe UI", 9.5F);

    private readonly BaoCaoBUS _bus = new();

    private DateTimePicker _dtpThang;
    private Label _lblDoanhThuHomNay;
    private Label _lblTieuDeThang;
    private Label _lblDoanhThuThang;
    private DataGridView _dgvTop5;

    public FormBaoCao()
    {
        InitializeUi();
        LoadReport();
    }

    private void InitializeUi()
    {
        Text = "Báo cáo - Thống kê";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1000, 650);
        MinimumSize = new Size(950, 600);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 135F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(BuildFilterGroup(), 0, 0);
        root.Controls.Add(BuildSummaryPanel(), 0, 1);
        root.Controls.Add(BuildTop5Group(), 0, 2);

        Controls.Add(root);
    }

    private GroupBox BuildFilterGroup()
    {
        var group = new GroupBox
        {
            Text = "Chọn thời gian báo cáo",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var label = new Label { Text = "Tháng / Năm:", Location = new Point(18, 36), AutoSize = true, Font = NormalFont };
        _dtpThang = new DateTimePicker
        {
            Location = new Point(135, 32),
            Width = 120,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MM/yyyy",
            ShowUpDown = true,
            Font = NormalFont,
            Value = DateTime.Today
        };

        var btnXem = UiHelper.CreateButton("Xem báo cáo", UiHelper.Accent, 130, 32);
        btnXem.Location = new Point(275, 30);
        btnXem.Click += (_, _) => LoadReport();

        group.Controls.Add(label);
        group.Controls.Add(_dtpThang);
        group.Controls.Add(btnXem);

        return group;
    }

    private TableLayoutPanel BuildSummaryPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 8)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        var groupHomNay = new GroupBox
        {
            Text = "Doanh thu hôm nay",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0)
        };
        _lblDoanhThuHomNay = new Label
        {
            Text = "0 đ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = UiHelper.Success
        };
        groupHomNay.Controls.Add(_lblDoanhThuHomNay);

        var groupThang = new GroupBox
        {
            Text = "Doanh thu theo tháng",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(6, 0, 0, 0)
        };
        _lblTieuDeThang = new Label
        {
            Text = "Tháng",
            Location = new Point(18, 30),
            AutoSize = true,
            Font = NormalFont,
            ForeColor = Color.DimGray
        };
        _lblDoanhThuThang = new Label
        {
            Text = "0 đ",
            Location = new Point(18, 55),
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = UiHelper.Danger
        };
        groupThang.Controls.Add(_lblTieuDeThang);
        groupThang.Controls.Add(_lblDoanhThuThang);

        panel.Controls.Add(groupHomNay, 0, 0);
        panel.Controls.Add(groupThang, 1, 0);

        return panel;
    }

    private GroupBox BuildTop5Group()
    {
        var group = new GroupBox
        {
            Text = "Top 5 sản phẩm bán chạy trong tháng",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0)
        };

        _dgvTop5 = new DataGridView { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(_dgvTop5);

        _dgvTop5.Columns.Add(UiHelper.CreateTextColumn("Mã sản phẩm", nameof(TopSanPham.MaSP), 18));
        _dgvTop5.Columns.Add(UiHelper.CreateTextColumn("Tên sản phẩm", nameof(TopSanPham.TenSP), 50));
        _dgvTop5.Columns.Add(UiHelper.CreateTextColumn("Số lượng bán", nameof(TopSanPham.SoLuongBan), 15, "N0", true));
        _dgvTop5.Columns.Add(UiHelper.CreateTextColumn("Doanh thu (đ)", nameof(TopSanPham.DoanhThu), 22, "N0", true));

        group.Controls.Add(_dgvTop5);
        return group;
    }

    private void LoadReport()
    {
        int thang = _dtpThang.Value.Month;
        int nam = _dtpThang.Value.Year;

        try
        {
            _lblDoanhThuHomNay.Text = UiHelper.Money(_bus.LayDoanhThuHomNay());
            _lblTieuDeThang.Text = $"Tháng {thang:00}/{nam}";
            _lblDoanhThuThang.Text = UiHelper.Money(_bus.LayDoanhThuThang(thang, nam));
            _dgvTop5.DataSource = _bus.LayTop5SanPhamBanChay(thang, nam);
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex.Message, "Không tải được báo cáo");
        }
    }
}
