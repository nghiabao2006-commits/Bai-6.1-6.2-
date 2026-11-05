/* ============================================================================
   ScriptDB_SalesDB.sql
   Cơ sở dữ liệu: SalesDB
   Hệ thống: Quản lý bán hàng & siêu thị mini (Bài tập 6.1)
   Script có thể chạy nhiều lần: chỉ tạo bảng và nạp dữ liệu mẫu khi chưa có.
   ============================================================================ */

IF DB_ID(N'SalesDB') IS NULL
BEGIN
    CREATE DATABASE SalesDB;
END
GO

USE SalesDB;
GO

/* Filtered index yêu cầu các SET option dưới đây (sqlcmd không bật sẵn như SSMS) */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

SET NOCOUNT ON;
GO

/* ---------------------------- Bảng Nhân viên ---------------------------- */
IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien
    (
        MaNV        INT IDENTITY(1,1) NOT NULL,
        TenNV       NVARCHAR(100)     NOT NULL,
        SoDienThoai VARCHAR(15)       NULL,
        ChucVu      NVARCHAR(50)      NULL,
        CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV)
    );
END
GO

/* Số điện thoại là duy nhất nhưng được phép bỏ trống (nhiều NULL) */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_NhanVien_SoDienThoai' AND object_id = OBJECT_ID(N'dbo.NhanVien'))
BEGIN
    CREATE UNIQUE INDEX UX_NhanVien_SoDienThoai
        ON dbo.NhanVien (SoDienThoai) WHERE SoDienThoai IS NOT NULL;
END
GO

/* ---------------------------- Bảng Khách hàng --------------------------- */
IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        MaKH        INT IDENTITY(1,1) NOT NULL,
        TenKH       NVARCHAR(100)     NOT NULL,
        DienThoai   VARCHAR(15)       NULL,
        DiemTichLuy INT               NOT NULL
            CONSTRAINT DF_KhachHang_DiemTichLuy DEFAULT (0),
        CONSTRAINT PK_KhachHang PRIMARY KEY (MaKH)
    );
END
GO

/* Điện thoại khách hàng là duy nhất nhưng được phép bỏ trống (nhiều NULL) */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_KhachHang_DienThoai' AND object_id = OBJECT_ID(N'dbo.KhachHang'))
BEGIN
    CREATE UNIQUE INDEX UX_KhachHang_DienThoai
        ON dbo.KhachHang (DienThoai) WHERE DienThoai IS NOT NULL;
END
GO

/* ---------------------------- Bảng Sản phẩm ----------------------------- */
IF OBJECT_ID(N'dbo.SanPham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SanPham
    (
        MaSP       VARCHAR(20)    NOT NULL,
        TenSP      NVARCHAR(150)  NOT NULL,
        GiaBan     DECIMAL(18,2)  NOT NULL,
        SoLuongTon INT            NOT NULL,
        CONSTRAINT PK_SanPham PRIMARY KEY (MaSP),
        CONSTRAINT CK_SanPham_GiaBan CHECK (GiaBan > 0),
        CONSTRAINT CK_SanPham_SoLuongTon CHECK (SoLuongTon >= 0)
    );
END
GO

/* ---------------------------- Bảng Hóa đơn ------------------------------ */
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon
    (
        MaHD    INT IDENTITY(1,1) NOT NULL,
        NgayLap DATETIME          NOT NULL
            CONSTRAINT DF_HoaDon_NgayLap DEFAULT (GETDATE()),
        MaNV    INT               NOT NULL,
        MaKH    INT               NULL,
        TongTien DECIMAL(18,2)    NOT NULL,
        CONSTRAINT PK_HoaDon PRIMARY KEY (MaHD),
        CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien (MaNV),
        CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang (MaKH)
    );
END
GO

/* ------------------------- Bảng Chi tiết hóa đơn ------------------------- */
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon
    (
        MaHD   INT           NOT NULL,
        MaSP   VARCHAR(20)   NOT NULL,
        SoLuong INT          NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaSP),
        CONSTRAINT FK_ChiTietHoaDon_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon (MaHD),
        CONSTRAINT FK_ChiTietHoaDon_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham (MaSP),
        CONSTRAINT CK_ChiTietHoaDon_SoLuong CHECK (SoLuong > 0)
    );
END
GO

/* ============================ DỮ LIỆU MẪU ================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien)
BEGIN
    SET IDENTITY_INSERT dbo.NhanVien ON;

    INSERT INTO dbo.NhanVien (MaNV, TenNV, SoDienThoai, ChucVu) VALUES
        (1, N'Nguyễn Văn An',  '0901234567', N'Thu ngân'),
        (2, N'Trần Thị Bình',  '0912345678', N'Quản lý'),
        (3, N'Lê Hoàng Nam',   '0987654321', N'Nhân viên bán hàng');

    SET IDENTITY_INSERT dbo.NhanVien OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang)
BEGIN
    SET IDENTITY_INSERT dbo.KhachHang ON;

    INSERT INTO dbo.KhachHang (MaKH, TenKH, DienThoai, DiemTichLuy) VALUES
        (1, N'Nguyễn Thị Mai',  '0909111222', 15),
        (2, N'Phạm Quốc Hưng',  '0938222333', 0),
        (3, N'Lý Thanh Hà',     '0977333444', 120);

    SET IDENTITY_INSERT dbo.KhachHang OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SanPham)
BEGIN
    INSERT INTO dbo.SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES
        ('8934567890123', N'Sữa tươi Vinamilk 100% 1L',          32000, 80),
        ('8938505970015', N'Nước suối Aquafina 500ml',              6000, 200),
        ('SP001',         N'Gạo ST25 túi 5kg',                    145000, 40),
        ('SP002',         N'Dầu ăn Tường An 1L',                   52000, 60),
        ('SP003',         N'Mì Hảo Hảo thùng 30 gói',             120000, 35),
        ('SP004',         N'Cà phê Trung Nguyên G7 hộp 20 gói',    64000, 45),
        ('SP005',         N'Bột giặt Omo 3kg',                     98000, 25),
        ('8935049500011', N'Nước mắm Nam Ngư 500ml',                45000, 50);
END
GO

/* Hóa đơn mẫu để màn hình Báo cáo - Thống kê có dữ liệu ngay khi chạy */
IF NOT EXISTS (SELECT 1 FROM dbo.HoaDon)
BEGIN
    DECLARE @hd1 INT, @hd2 INT, @hd3 INT;

    /* Hóa đơn 1: hôm nay */
    INSERT INTO dbo.HoaDon (NgayLap, MaNV, MaKH, TongTien)
    VALUES (GETDATE(), 1, 1, 386000);
    SET @hd1 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
        (@hd1, 'SP001',         2, 145000),
        (@hd1, '8934567890123', 3, 32000);

    UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - 2 WHERE MaSP = 'SP001';
    UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - 3 WHERE MaSP = '8934567890123';
    UPDATE dbo.KhachHang SET DiemTichLuy = DiemTichLuy + 3 WHERE MaKH = 1;

    /* Hóa đơn 2: cách đây 2 ngày */
    INSERT INTO dbo.HoaDon (NgayLap, MaNV, MaKH, TongTien)
    VALUES (DATEADD(DAY, -2, GETDATE()), 1, 2, 112000);
    SET @hd2 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
        (@hd2, 'SP002',         1, 52000),
        (@hd2, '8938505970015', 10, 6000);

    UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - 1  WHERE MaSP = 'SP002';
    UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - 10 WHERE MaSP = '8938505970015';
    UPDATE dbo.KhachHang SET DiemTichLuy = DiemTichLuy + 1 WHERE MaKH = 2;

    /* Hóa đơn 3: hôm nay, khách lẻ */
    INSERT INTO dbo.HoaDon (NgayLap, MaNV, MaKH, TongTien)
    VALUES (GETDATE(), 2, NULL, 120000);
    SET @hd3 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
        (@hd3, 'SP003', 1, 120000);

    UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - 1 WHERE MaSP = 'SP003';
END
GO

PRINT N'Đã khởi tạo cơ sở dữ liệu SalesDB.';
GO
