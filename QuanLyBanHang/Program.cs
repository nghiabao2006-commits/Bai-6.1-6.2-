using QuanLyBanHang.DAL;
using QuanLyBanHang.GUI;

namespace QuanLyBanHang;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseInitializer.EnsureDatabase();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể khởi tạo cơ sở dữ liệu SalesDB.\n\n" +
                ex.Message +
                "\n\nHãy kiểm tra SQL Server (hoặc SQL Server LocalDB) đã được cài đặt và đang chạy.",
                "Lỗi khởi động",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
