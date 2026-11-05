using System.Globalization;

namespace QuanLyBanHang.GUI;

internal static class UiHelper
{
    public static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    public static readonly Color Primary = Color.FromArgb(31, 78, 121);
    public static readonly Color Accent = Color.FromArgb(0, 122, 204);
    public static readonly Color Danger = Color.FromArgb(192, 57, 43);
    public static readonly Color Success = Color.FromArgb(39, 174, 96);
    public static readonly Color PageBackground = Color.FromArgb(244, 246, 249);

    public static string Money(decimal value) => value.ToString("N0", Vietnamese) + " đ";

    public static Button CreateButton(string text, Color backColor, int width = 130, int height = 36)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(6)
        };
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(212, 230, 241);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.RowTemplate.Height = 30;
    }

    public static DataGridViewTextBoxColumn CreateTextColumn(
        string header,
        string property,
        float fillWeight = 100,
        string format = null,
        bool alignRight = false)
    {
        var column = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = fillWeight
        };

        if (format != null)
        {
            column.DefaultCellStyle.Format = format;
            column.DefaultCellStyle.FormatProvider = Vietnamese;
        }

        if (alignRight)
        {
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        return column;
    }

    public static void ShowError(IWin32Window owner, string message, string title = "Lỗi")
    {
        MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowInfo(IWin32Window owner, string message, string title = "Thông báo")
    {
        MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
