using QuanLyNhanSu.DAL;

namespace QuanLyNhanSu.GUI;

public class MainForm : Form
{
    public MainForm()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        Text = "Quản lý nhân sự - Tính lương";
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
            Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            AutoSize = true,
            UseMnemonic = false,
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

        AddMenuButton(menu, "Quản lý nhân viên", (_, _) => Open(new FormNhanVien()));
        AddMenuButton(menu, "Quản lý phòng ban", (_, _) => Open(new FormPhongBan()));
        AddMenuButton(menu, "Chấm công tháng", (_, _) => Open(new FormChamCong()));
        AddMenuButton(menu, "Tính lương - Chốt lương", (_, _) => Open(new FormLuong()));

        var exitButton = UiHelper.CreateButton("Thoát", UiHelper.Danger, 210, 90);
        exitButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        exitButton.Margin = new Padding(10);
        exitButton.Click += (_, _) => Close();
        menu.Controls.Add(exitButton);

        var status = new StatusStrip { SizingGrip = false };
        status.Items.Add(new ToolStripStatusLabel("Cơ sở dữ liệu: HRMDB") { Spring = true, TextAlign = ContentAlignment.MiddleLeft });
        status.Items.Add(new ToolStripStatusLabel("Máy chủ: " + DbConnectionFactory.ServerName));

        Controls.Add(menu);
        Controls.Add(header);
        Controls.Add(status);
    }

    private static void AddMenuButton(FlowLayoutPanel menu, string text, EventHandler handler)
    {
        var button = UiHelper.CreateButton(text, UiHelper.Accent, 230, 90);
        button.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
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
