/* ============================================================================
   ScriptDB_HRMDB.sql
   Cơ sở dữ liệu: HRMDB
   Hệ thống: Quản lý nhân sự & tính lương (Bài tập 6.2)
   Script có thể chạy nhiều lần: chỉ tạo bảng và nạp dữ liệu mẫu khi chưa có.
   ============================================================================ */

IF DB_ID(N'HRMDB') IS NULL
BEGIN
    CREATE DATABASE HRMDB;
END
GO

USE HRMDB;
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

/* ---------------------------- Bảng Phòng ban ---------------------------- */
IF OBJECT_ID(N'dbo.PhongBan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhongBan
    (
        MaPB  INT IDENTITY(1,1) NOT NULL,
        TenPB NVARCHAR(100)     NOT NULL,
        Mota  NVARCHAR(255)     NULL,
        CONSTRAINT PK_PhongBan PRIMARY KEY (MaPB)
    );
END
GO

/* ---------------------------- Bảng Nhân viên ---------------------------- */
IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien
    (
        MaNV        INT IDENTITY(1,1) NOT NULL,
        HoTen       NVARCHAR(100)     NOT NULL,
        NgaySinh    DATETIME          NULL,
        CCCD        VARCHAR(20)       NULL,
        MaPB        INT               NULL,
        LuongCoBan  DECIMAL(18,2)     NOT NULL,
        HeSoLuong   FLOAT             NOT NULL
            CONSTRAINT DF_NhanVien_HeSoLuong DEFAULT (1.0),
        CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV),
        CONSTRAINT CK_NhanVien_LuongCoBan CHECK (LuongCoBan > 0),
        CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY (MaPB) REFERENCES dbo.PhongBan (MaPB)
    );
END
GO

/* CCCD là duy nhất nhưng được phép bỏ trống (nhiều NULL) */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_NhanVien_CCCD' AND object_id = OBJECT_ID(N'dbo.NhanVien'))
BEGIN
    CREATE UNIQUE INDEX UX_NhanVien_CCCD
        ON dbo.NhanVien (CCCD) WHERE CCCD IS NOT NULL;
END
GO

/* -------------------------- Bảng Chấm công tháng ------------------------- */
IF OBJECT_ID(N'dbo.BangChamCong', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BangChamCong
    (
        MaCong    INT IDENTITY(1,1) NOT NULL,
        MaNV      INT               NOT NULL,
        ThangNam  VARCHAR(7)        NOT NULL,
        SoNgayCong FLOAT            NOT NULL
            CONSTRAINT DF_BangChamCong_SoNgayCong DEFAULT (0),
        SoGioOT   FLOAT             NOT NULL
            CONSTRAINT DF_BangChamCong_SoGioOT DEFAULT (0),
        DaChot    BIT               NOT NULL
            CONSTRAINT DF_BangChamCong_DaChot DEFAULT (0),
        CONSTRAINT PK_BangChamCong PRIMARY KEY (MaCong),
        CONSTRAINT UQ_BangChamCong UNIQUE (MaNV, ThangNam),
        CONSTRAINT CK_BangChamCong_SoNgayCong CHECK (SoNgayCong >= 0 AND SoNgayCong <= 31),
        CONSTRAINT CK_BangChamCong_SoGioOT CHECK (SoGioOT >= 0),
        CONSTRAINT FK_BangChamCong_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien (MaNV)
    );
END
GO

/* ---------------------------- Bảng Lương -------------------------------- */
IF OBJECT_ID(N'dbo.BangLuong', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BangLuong
    (
        MaLuong  INT IDENTITY(1,1) NOT NULL,
        MaNV     INT               NOT NULL,
        ThangNam VARCHAR(7)        NOT NULL,
        ThucLinh DECIMAL(18,2)     NOT NULL,
        CONSTRAINT PK_BangLuong PRIMARY KEY (MaLuong),
        CONSTRAINT UQ_BangLuong UNIQUE (MaNV, ThangNam),
        CONSTRAINT FK_BangLuong_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien (MaNV)
    );
END
GO

/* ============================ DỮ LIỆU MẪU ================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.PhongBan)
BEGIN
    SET IDENTITY_INSERT dbo.PhongBan ON;

    INSERT INTO dbo.PhongBan (MaPB, TenPB, Mota) VALUES
        (1, N'Phòng Nhân sự',  N'Quản lý hồ sơ, tuyển dụng và chấm công'),
        (2, N'Phòng Kế toán',  N'Quản lý tài chính, lương thưởng'),
        (3, N'Phòng Kinh doanh', N'Kinh doanh và chăm sóc khách hàng');

    SET IDENTITY_INSERT dbo.PhongBan OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien)
BEGIN
    SET IDENTITY_INSERT dbo.NhanVien ON;

    INSERT INTO dbo.NhanVien (MaNV, HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
        (1, N'Nguyễn Thị Lan',  '1990-05-12', '001190000001', 1, 12000000, 1.5),
        (2, N'Trần Văn Minh',   '1988-11-23', '001188000002', 3, 15000000, 1.8),
        (3, N'Lê Thu Hà',       '1995-02-14', '001195000003', 2, 10000000, 1.3),
        (4, N'Phạm Quốc Bảo',   '1993-08-30', '001193000004', 3,  9000000, 1.0),
        (5, N'Hoàng Minh Châu', '1997-12-05', '001197000005', 1,  8500000, 1.2),
        (6, N'Đỗ Gia Huy',      '1991-03-21', '001191000006', 2, 11000000, 1.4);

    SET IDENTITY_INSERT dbo.NhanVien OFF;
END
GO

/* Chấm công mẫu cho tháng hiện tại để màn hình Chấm công có dữ liệu ngay */
DECLARE @ThangNam VARCHAR(7) =
    RIGHT(CONVERT(VARCHAR(10), GETDATE(), 103), 7);  /* MM/yyyy */

IF NOT EXISTS (SELECT 1 FROM dbo.BangChamCong WHERE ThangNam = @ThangNam)
BEGIN
    INSERT INTO dbo.BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT) VALUES
        (1, @ThangNam, 22, 4),
        (2, @ThangNam, 21, 10),
        (3, @ThangNam, 23, 2),
        (4, @ThangNam, 20, 0),
        (5, @ThangNam, 24, 6),
        (6, @ThangNam, 22, 8);
END
GO

PRINT N'Đã khởi tạo cơ sở dữ liệu HRMDB.';
GO
