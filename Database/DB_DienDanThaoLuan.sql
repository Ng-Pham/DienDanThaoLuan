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
	AnhBia VARCHAR (50),
	Email VARCHAR(30),
	GioiTinh NVARCHAR(3),
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
	AnhBia VARCHAR (50),
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
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV),
	MaQTV VARCHAR(15) FOREIGN KEY (MaQTV) REFERENCES QuanTriVien(MaQTV)
)

CREATE TABLE BinhLuan
(
	MaBL VARCHAR(15) PRIMARY KEY,
	IDCha VARCHAR(15),
	NoiDung xml,
	NgayGui DATETIME,
	TrangThai NVARCHAR(20),
	MaBV VARCHAR(15) FOREIGN KEY (MaBV) REFERENCES BaiViet(MaBV),
	MaTV VARCHAR(15) FOREIGN KEY (MaTV) REFERENCES ThanhVien(MaTV),
	MaQTV VARCHAR(15) FOREIGN KEY (MaQTV) REFERENCES QuanTriVien(MaQTV)
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
	MaQTV VARCHAR(15) FOREIGN KEY (MaQTV) REFERENCES QuanTriVien(MaQTV),
	MaDoiTuong VARCHAR(15),
	LoaiDoiTuong VARCHAR(50),
	TrangThai BIT
)

-- Dữ liệu cho bảng QuanTriVien
INSERT INTO QuanTriVien (MaQTV, HoTen, AnhDaiDien, AnhBia, Email, GioiTinh, SDT, NgaySinh, TenDangNhap, MatKhau) VALUES
('QTV001', N'Nguyễn Văn A', N'avatar.jpg', N'default-bg.jpg','nva@gmail.com', N'Nam' ,'0912345678', '1980-05-15', 'nguyenvana', 'ad123'),
('QTV002', N'Trần Thị B', N'avatar.jpg', N'default-bg.jpg','ttb@gmail.com', N'Nữ','0987654321', '1985-11-25', 'tranthib', 'ad456'),
('QTV003', N'Lê Quốc Cường', N'avatar.jpg', N'default-bg.jpg', 'lqc@gmail.com', N'Nam', '0912233445', '1975-02-20', 'lequocc', 'ad123'),
('QTV004', N'Nguyễn Thị Thanh', N'avatar.jpg', N'default-bg.jpg', 'ntt@gmail.com', N'Nữ', '0912345690', '1982-07-15', 'nguyentt', 'ad123'),
('QTV005', N'Phạm Quang Huy', N'avatar.jpg', N'default-bg.jpg', 'pqh@gmail.com', N'Nam', '0923456781', '1979-03-19', 'phamqh', 'ad123'),
('QTV006', N'Ngô Mỹ Dung', N'avatar.jpg', N'default-bg.jpg', 'nmd@gmail.com', N'Nữ', '0934567892', '1983-12-01', 'ngomy', 'ad123'),
('QTV007', N'Vũ Quốc Tuấn', N'avatar.jpg', N'default-bg.jpg', 'vqt@gmail.com', N'Nam', '0945678903', '1977-05-05', 'vuqt', 'ad123'),
('QTV008', N'Hồ Ngọc Minh', N'avatar.jpg', N'default-bg.jpg', 'hnm@gmail.com', N'Nam', '0956789012', '1986-08-20', 'hongocminh', 'ad123'),
('QTV009', N'Tran Bảo Trân', N'avatar.jpg', N'default-bg.jpg', 'tbt@gmail.com', N'Nữ', '0967890123', '1989-11-30', 'tranbao', 'ad123'),
('QTV010', N'Bùi Văn Đông', N'avatar.jpg', N'default-bg.jpg', 'bvd@gmail.com', N'Nam', '0978901234', '1988-09-25', 'buivd', 'ad123');

-- Dữ liệu cho bảng ThanhVien
INSERT INTO ThanhVien (MaTV, HoTen, AnhDaiDien, AnhBia, Email, GioiTinh, SDT, NgaySinh, NgayThamGia, TenDangNhap, MatKhau) VALUES
('TV001', N'Lê Văn C', N'avatar.jpg', N'default-bg.jpg','lvc@gmail.com', N'Nam', '0911222333', '1999-03-21', '2023-01-01', 'levanc', '123'),
('TV002', N'Phạm Thị D', N'avatar.jpg', N'default-bg.jpg','ptd@gmail.com', N'Nữ', '0922333444', '2000-08-10', '2023-02-15', 'phamthid', '123'),
('TV003', N'Tạ Gia Bảo', N'avatar2.jpg', N'default-bg.jpg','baotg@gmail.com', N'Nam', '0909123456', '2003-01-01', '2023-04-22', 'banphuf29966', '123'),
('TV004', N'Nguyễn Văn Phong', N'avatar.jpg', N'default-bg.jpg', 'nvp@gmail.com', N'Nam', '0931234567', '2001-06-05', '2023-03-10', 'nguyenphong', '23'),
('TV005', N'Hoàng Thị Vân', N'avatar.jpg', N'default-bg.jpg', 'htv@gmail.com', N'Nữ', '0932345678', '1998-09-09', '2023-04-05', 'hoangtv', '123'),
('TV006', N'Lê Minh Tuấn', N'avatar.jpg', N'default-bg.jpg', 'lmt@gmail.com', N'Nam', '0933456789', '1997-07-19', '2023-05-02', 'leminht', '123'),
('TV007', N'Phạm Văn Hậu', N'avatar.jpg', N'default-bg.jpg', 'pvh@gmail.com', N'Nam', '0934567890', '2002-05-22', '2023-06-15', 'phamvh', '123'),
('TV008', N'Võ Thị Hồng', N'avatar.jpg', N'default-bg.jpg', 'vth@gmail.com', N'Nữ', '0935678901', '2000-10-10', '2023-07-08', 'vothong', '123'),
('TV009', N'Nguyễn Nhật Nam', N'avatar.jpg', N'default-bg.jpg', 'nnn@gmail.com', N'Nam', '0936789012', '2002-12-12', '2023-08-03', 'nguyennn', '123'),
('TV010', N'Phạm Thuỳ Linh', N'avatar.jpg', N'default-bg.jpg', 'ptl@gmail.com', N'Nữ', '0937890123', '2003-01-01', '2023-09-14', 'phamlinh', '123');

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
('BV002', N'Các phương pháp bảo mật mạng', N'<NoiDung>Những cách bảo vệ hệ thống mạng khỏi tấn công mạng</NoiDung>', N'2023-09-10', N'Đã duyệt', 'CD005', 'TV002'),
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
INSERT INTO ThongBao (MaTB, NoiDung, NgayTB, LoaiTB, MaTV, MaQTV, MaDoiTuong, LoaiDoiTuong, TrangThai)
VALUES 
('TB001', N'<NoiDung>Bài viết của bạn đã được duyệt</NoiDung>', '2024-10-01 10:00:00', N'Duyệt bài viết', 'TV001', NULL, 'BV001', N'BaiViet', 0),
('TB002', N'<NoiDung>Có bình luận mới trên bài viết của bạn</NoiDung>', '2024-10-02 12:00:00', N'Bình luận', 'TV002', NULL, 'BL002', N'BinhLuan', 0),
('TB003', N'<NoiDung>Chúc mừng năm mới 2024</NoiDung>', '2024-10-02 13:00:00', N'Thông báo hệ thống', NULL, NULL, NULL, NULL, 0),
('TB004', N'<NoiDung>Thầy cho nhóm em 10 điểm nha </NoiDung>', '2024-11-01 09:00:00', N'Thông báo hệ thống', NULL, NULL, NULL, NULL, 1);
