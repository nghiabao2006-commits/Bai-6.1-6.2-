using QuanLyBanHang.DAL;

namespace QuanLyBanHang.GUI;

public class MainForm : Form
{
    public MainForm()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        Text = "Quản lý bán hàng - Siêu thị mini";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 620);
        MinimumSize = new Size(900, 560);
        BackColor = Color.White;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = UiHelper.Primary
        };

        var title = new Label
        {
            Text = "HỆ THỐNG QUẢN LÝ BÁN HÀNG",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(32, 24)
        };

        header.Controls.Add(title);
        
        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = UiHelper.PageBackground,
            AutoScroll = true
        };

        AddMenuButton(menu, "Bán hàng", (_, _) => Open(new FormBanHang()));
        AddMenuButton(menu, "Quản lý sản phẩm", (_, _) => Open(new FormSanPham()));
        AddMenuButton(menu, "Quản lý khách hàng", (_, _) => Open(new FormKhachHang()));
        AddMenuButton(menu, "Quản lý nhân viên", (_, _) => Open(new FormNhanVien()));
        AddMenuButton(menu, "Báo cáo - Thống kê", (_, _) => Open(new FormBaoCao()));

        var exitButton = UiHelper.CreateButton("Thoát", UiHelper.Danger, 210, 90);
        exitButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        exitButton.Margin = new Padding(10);
        exitButton.Click += (_, _) => Close();
        menu.Controls.Add(exitButton);

        var status = new StatusStrip { SizingGrip = false };
        status.Items.Add(new ToolStripStatusLabel("Cơ sở dữ liệu: SalesDB") { Spring = true, TextAlign = ContentAlignment.MiddleLeft });
        status.Items.Add(new ToolStripStatusLabel("Máy chủ: " + DbConnectionFactory.ServerName));

        Controls.Add(menu);
        Controls.Add(header);
        Controls.Add(status);
    }

    private static void AddMenuButton(FlowLayoutPanel menu, string text, EventHandler handler)
    {
        var button = UiHelper.CreateButton(text, UiHelper.Accent, 210, 90);
        button.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Margin = new Padding(10);
        button.Click += handler;
        menu.Controls.Add(button);
    }

    private void Open(Form form)
    {
        using (form)
        {
            form.ShowDialog(this);
        }
    }
}
