use master
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'DienDanThaoLuan')
BEGIN
    DROP DATABASE DienDanThaoLuan;
END
GO


CREATE DATABASE DienDanThaoLuan

use DienDanThaoLuan

CREATE TABLE QuanTriVien
(
	MaQTV VARCHAR(15) PRIMARY KEY,
	HoTen NVARCHAR(80),
	AnhDaiDien VARCHAR(50),
	Email VARCHAR(30),
	SDT VARCHAR(11),
	NgaySinh DATE,
	TenDangNhap VARCHAR(15),
	MatKhau VARCHAR(10)
)

CREATE TABLE ThanhVien
(
	MaTV VARCHAR(15) PRIMARY KEY,
	HoTen NVARCHAR(80),
	AnhDaiDien VARCHAR(50),
	Email VARCHAR(30),
	GioiTinh NVARCHAR(3),
	SDT VARCHAR(11),
	NgaySinh DATE,
	NgayThamGia DATE,
	TenDangNhap VARCHAR(15),
	MatKhau VARCHAR(10)
)

CREATE TABLE LoaiCD
(
	MaLoai VARCHAR(15) PRIMARY KEY,
	TenLoai NVARCHAR(50)
)

CREATE TABLE ChuDe
(
	MaCD VARCHAR(15) PRIMARY KEY,
	TenCD NVARCHAR(50),
	MaLoai VARCHAR(15) FOREIGN KEY (MaLoai) REFERENCES LoaiCD(MaLoai)
)

CREATE TABLE BaiViet
(
	MaBV VARCHAR(15) PRIMARY KEY,
	TieuDeBV NVARCHAR(30),
	NoiDung xml,
	NgayDang DATETIME,
	TrangThai NVARCHAR(20),
	MaCD VARCHAR(15) FOREIGN KEY (MaCD) REFERENCES ChuDe(MaCD),
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV)
)

CREATE TABLE BinhLuan
(
	MaBL VARCHAR(15) PRIMARY KEY,
	IDCha VARCHAR(15),
	NoiDung xml,
	NgayGui DATETIME,
	TrangThai NVARCHAR(20),
	MaBV VARCHAR(15) FOREIGN KEY (MaBV) REFERENCES BaiViet(MaBV),
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV)
)

CREATE TABLE GopY
(
	ID int PRIMARY KEY IDENTITY(1,1),
	NoiDung xml,
	NgayGui DATETIME,
	TrangThai BIT,
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV)
)

CREATE TABLE ThongBao
(
	MaTB VARCHAR(15) PRIMARY KEY,
	NoiDung xml,
	NgayTB DATETIME,
	LoaiTB NVARCHAR(30),
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV),
	MaDoiTuong VARCHAR(15),
	LoaiDoiTuong VARCHAR(50),
	TrangThai BIT
)

-- Dữ liệu cho bảng QuanTriVien
INSERT INTO QuanTriVien (MaQTV, HoTen, AnhDaiDien, Email, SDT, NgaySinh, TenDangNhap, MatKhau) VALUES
('QTV001', N'Nguyễn Văn A', N'avatar.jpg', 'nva@gmail.com', '0912345678', '1980-05-15', 'nguyenvana', 'ad123'),
('QTV002', N'Trần Thị B', N'avatar.jpg', 'ttb@gmail.com', '0987654321', '1985-11-25', 'tranthib', 'ad456');

-- Dữ liệu cho bảng ThanhVien
INSERT INTO ThanhVien (MaTV, HoTen, AnhDaiDien, Email, GioiTinh, SDT, NgaySinh, NgayThamGia, TenDangNhap, MatKhau) VALUES
('TV001', N'Lê Văn C', N'avatar.jpg', 'lvc@gmail.com', 'Nam', '0911222333', '1999-03-21', '2023-01-01', 'levanc', '123'),
('TV002', N'Phạm Thị D', N'avatar.jpg', 'ptd@gmail.com', 'Nữ', '0922333444', '2000-08-10', '2023-02-15', 'phamthid', '456');

-- Dữ liệu cho bảng LoaiCD
INSERT INTO LoaiCD (MaLoai, TenLoai) VALUES
('L001', N'Ngôn ngữ lập trình'),
('L002', N'Bảo mật và an ninh mạng'),
('L003', N'Trí tuệ nhân tạo và Học máy'),
('L004', N'Cơ sở dữ liệu và Hệ quản trị CSDL'),
('L005', N'Phát triển phần mềm và Quản lý dự án'),
('L006', N'Hệ thống nhúng và IoT');

-- Dữ liệu cho bảng ChuDe
INSERT INTO ChuDe (MaCD, TenCD, MaLoai) VALUES
-- Chủ đề thuộc Loại Ngôn ngữ lập trình
('CD001', N'Lập trình Python', 'L001'),
('CD002', N'Lập trình Java', 'L001'),
('CD003', N'Lập trình C++', 'L001'),
('CD004', N'Lập trình JavaScript', 'L001'),

-- Chủ đề thuộc Loại Bảo mật và an ninh mạng
('CD005', N'Bảo mật hệ thống mạng', 'L002'),
('CD006', N'An ninh mạng trong doanh nghiệp', 'L002'),
('CD007', N'Tấn công mạng và cách phòng chống', 'L002'),
('CD008', N'Phòng thủ mạng với Firewall', 'L002'),

-- Chủ đề thuộc Loại Trí tuệ nhân tạo và Học máy
('CD009', N'Machine Learning cơ bản', 'L003'),
('CD010', N'Deep Learning với TensorFlow', 'L003'),
('CD011', N'Xử lý ngôn ngữ tự nhiên (NLP)', 'L003'),
('CD012', N'Thị giác máy tính (Computer Vision)', 'L003'),

-- Chủ đề thuộc Loại Cơ sở dữ liệu và Hệ quản trị CSDL
('CD013', N'Quản trị cơ sở dữ liệu SQL', 'L004'),
('CD014', N'Cơ sở dữ liệu NoSQL', 'L004'),
('CD015', N'Tối ưu hóa truy vấn SQL', 'L004'),
('CD016', N'Thiết kế cơ sở dữ liệu', 'L004'),

-- Chủ đề thuộc Loại Phát triển phần mềm và Quản lý dự án
('CD017', N'Phát triển phần mềm Agile', 'L005'),
('CD018', N'Quản lý dự án Scrum', 'L005'),
('CD019', N'Phần mềm quản lý dự án Jira', 'L005'),
('CD020', N'Kiểm thử phần mềm', 'L005'),

-- Chủ đề thuộc Loại Hệ thống nhúng và IoT
('CD021', N'Cảm biến trong IoT', 'L006'),
('CD022', N'Hệ điều hành thời gian thực (RTOS)', 'L006'),
('CD023', N'Giao thức truyền thông trong IoT', 'L006'),
('CD024', N'Phát triển ứng dụng IoT với Arduino', 'L006');

-- Dữ liệu cho bảng BaiViet
INSERT INTO BaiViet (MaBV, TieuDeBV, NoiDung, NgayDang, TrangThai, MaCD, MaTV) VALUES
('BV001', N'Học lập trình Python cơ bản',N'<NoiDung>Bài viết về Python dành cho người mới bắt đầu</NoiDung>', '2023-09-01', N'Đã duyệt', 'CD001', 'TV001'),
('BV002', N'Các phương pháp bảo mật mạng', N'<NoiDung>Những cách bảo vệ hệ thống mạng khỏi tấn công mạng</NoiDung>', N'2023-09-10', 'Đã duyệt', 'CD005', 'TV002'),
('BV003', N'Giới thiệu về Machine Learning', N'<NoiDung>Bài viết về Machine Learning cơ bản</NoiDung>', '2023-09-15', N'Đã duyệt', 'CD009', 'TV001'),
('BV004', N'Quản trị SQL Server', N'<NoiDung>Cách quản trị cơ sở dữ liệu bằng SQL Server</NoiDung>', '2023-09-18', N'Đã duyệt', 'CD013', 'TV002');

-- Dữ liệu cho bảng BinhLuan
INSERT INTO BinhLuan (MaBL, IDCha, NoiDung, NgayGui, TrangThai, MaBV, MaTV) VALUES
('BL001',null, N'<NoiDung>Bài viết rất hữu ích</NoiDung>', '2023-09-02', N'Hiển thị', 'BV001', 'TV002'),
('BL002',null, N'<NoiDung>Tôi đã học được nhiều điều mới</NoiDung>', '2024-10-02', N'Hiển thị', 'BV002', 'TV001');

-- Dữ liệu cho bảng GopY
INSERT INTO GopY (NoiDung, NgayGui, TrangThai, MaTV) VALUES
(N'<NoiDung>Giao diện trang web cần cải thiện</NoiDung>', '2023-09-05', 1, 'TV001'),
(N'<NoiDung>Tốc độ tải web cần được cải thiện</NoiDung>', '2023-09-12', 1, 'TV002');

-- Dữ liệu cho bảng ThongBao
INSERT INTO ThongBao (MaTB, NoiDung, NgayTB, MaQTV) VALUES
('TB001', N'<NoiDung>Chào mừng ngày 8/3</NoiDung>', '2023-08-03', 'QTV001'),
('TB002', N'<NoiDung>Chúc mừng năm mới 2024</NoiDung>', '2024-01-01', 'QTV002');
INSERT INTO ThongBao (MaTB, NoiDung, NgayTB, LoaiTB, MaTV, MaDoiTuong, LoaiDoiTuong, TrangThai)
VALUES 
('TB001', N'<NoiDung>Bài viết của bạn đã được duyệt</NoiDung>', '2024-10-01 10:00:00', N'Duyệt bài viết', 'TV001', 'BV001', N'BaiViet', 0),
('TB002', N'<NoiDung>Có bình luận mới trên bài viết của bạn</NoiDung>', '2024-10-02 12:00:00', N'Bình luận', 'TV002', 'BL002', N'BinhLuan', 0),
('TB003', N'<NoiDung>Chúc mừng năm mới 2024</NoiDung>', '2024-10-02 13:00:00', N'Thông báo hệ thống', NULL, NULL, NULL, 1).