using Microsoft.Data.SqlClient;

namespace QuanLyBanHang.DAL;

internal static class SqlErrorHelper
{
    public static Exception ToFriendly(SqlException ex, string duplicateMessage, string referenceMessage)
    {
        return ex.Number switch
        {
            2601 or 2627 => new InvalidOperationException(duplicateMessage, ex),
            547 => new InvalidOperationException(referenceMessage, ex),
            _ => ex
        };
    }
}
