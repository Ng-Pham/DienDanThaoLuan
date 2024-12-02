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
	MatKhau VARCHAR(60)
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
	MatKhau VARCHAR(60)
)
ALTER TABLE ThanhVien
ALTER COLUMN MatKhau VARCHAR(60);

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
	TieuDeBV NVARCHAR(60),
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
--pass chung ad123456
('QTV001', N'Nguyễn Văn A', N'avatar.jpg', N'default-bg.jpg','nva@gmail.com', N'Nam' ,'0912345678', '1980-05-15', 'nguyenvana', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV002', N'Trần Thị B', N'avatar.jpg', N'default-bg.jpg','ttb@gmail.com', N'Nữ','0987654321', '1985-11-25', 'tranthib', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV003', N'Lê Quốc Cường', N'avatar.jpg', N'default-bg.jpg', 'lqc@gmail.com', N'Nam', '0912233445', '1975-02-20', 'lequocc', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV004', N'Nguyễn Thị Thanh', N'avatar.jpg', N'default-bg.jpg', 'ntt@gmail.com', N'Nữ', '0912345690', '1982-07-15', 'nguyentt', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV005', N'Phạm Quang Huy', N'avatar.jpg', N'default-bg.jpg', 'pqh@gmail.com', N'Nam', '0923456781', '1979-03-19', 'phamqh', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV006', N'Ngô Mỹ Dung', N'avatar.jpg', N'default-bg.jpg', 'nmd@gmail.com', N'Nữ', '0934567892', '1983-12-01', 'ngomy', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV007', N'Vũ Quốc Tuấn', N'avatar.jpg', N'default-bg.jpg', 'vqt@gmail.com', N'Nam', '0945678903', '1977-05-05', 'vuqt', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV008', N'Hồ Ngọc Minh', N'avatar.jpg', N'default-bg.jpg', 'hnm@gmail.com', N'Nam', '0956789012', '1986-08-20', 'hongocminh', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV009', N'Tran Bảo Trân', N'avatar.jpg', N'default-bg.jpg', 'tbt@gmail.com', N'Nữ', '0967890123', '1989-11-30', 'tranbao', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m'),
('QTV010', N'Bùi Văn Đông', N'avatar.jpg', N'default-bg.jpg', 'bvd@gmail.com', N'Nam', '0978901234', '1988-09-25', 'buivd', '$2a$11$bJjUvKBItEjHIzcNh/jSce0efbG/gwU38VsI.IInXmuz7ZW3ZLJ3m');

-- Dữ liệu cho bảng ThanhVien
INSERT INTO ThanhVien (MaTV, HoTen, AnhDaiDien, AnhBia, Email, GioiTinh, SDT, NgaySinh, NgayThamGia, TenDangNhap, MatKhau) VALUES
--pass chung 12345678
('TV001', N'Lê Văn C', N'avatar.jpg', N'default-bg.jpg','lvc@gmail.com', N'Nam', '0911222333', '1999-03-21', '2023-01-01', 'levanc', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV002', N'Phạm Thị D', N'avatar.jpg', N'default-bg.jpg','ptd@gmail.com', N'Nữ', '0922333444', '2000-08-10', '2023-02-15', 'phamthid', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV003', N'Tạ Gia Bảo', N'avatar2.jpg', N'default-bg.jpg','baotg@gmail.com', N'Nam', '0909123456', '2003-01-01', '2023-04-22', 'banphuf29966', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV004', N'Nguyễn Văn Phong', N'avatar.jpg', N'default-bg.jpg', 'nvp@gmail.com', N'Nam', '0931234567', '2001-06-05', '2023-03-10', 'nguyenphong', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV005', N'Hoàng Thị Vân', N'avatar.jpg', N'default-bg.jpg', 'htv@gmail.com', N'Nữ', '0932345678', '1998-09-09', '2023-04-05', 'hoangtv', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV006', N'Lê Minh Tuấn', N'avatar.jpg', N'default-bg.jpg', 'lmt@gmail.com', N'Nam', '0933456789', '1997-07-19', '2023-05-02', 'leminht', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV007', N'Phạm Văn Hậu', N'avatar.jpg', N'default-bg.jpg', 'pvh@gmail.com', N'Nam', '0934567890', '2002-05-22', '2023-06-15', 'phamvh', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV008', N'Võ Thị Hồng', N'avatar.jpg', N'default-bg.jpg', 'vth@gmail.com', N'Nữ', '0935678901', '2000-10-10', '2023-07-08', 'vothong', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV009', N'Nguyễn Nhật Nam', N'avatar.jpg', N'default-bg.jpg', 'nnn@gmail.com', N'Nam', '0936789012', '2002-12-12', '2023-08-03', 'nguyennn', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu'),
('TV010', N'Phạm Thuỳ Linh', N'avatar.jpg', N'default-bg.jpg', 'ptl@gmail.com', N'Nữ', '0937890123', '2003-01-01', '2023-09-14', 'phamlinh', '$2a$11$v/0O4f5Ya2.WljHWHZ9y5O5q6htDckM.P0mokifPOv8hRaUO.g9tu');

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
('BV004', N'Quản trị SQL Server', N'<NoiDung>Cách quản trị cơ sở dữ liệu bằng SQL Server</NoiDung>', '2023-09-18', N'Đã duyệt', 'CD013', 'TV002'),
('BV005', N'Tối ưu hóa mã Python', N'<NoiDung>Các phương pháp tối ưu hóa mã Python</NoiDung>', '2023-09-20', N'Đã duyệt', 'CD001', 'TV003'),
('BV006', N'Java cơ bản cho người mới', N'<NoiDung>Giới thiệu ngôn ngữ lập trình Java cơ bản</NoiDung>', '2023-09-25', N'Đã duyệt', 'CD002', 'TV004'),
('BV007', N'Hệ thống bảo mật mạng', N'<NoiDung>Chiến lược bảo mật cho mạng doanh nghiệp</NoiDung>', '2023-09-30', N'Đã duyệt', 'CD005', 'TV005'),
('BV008', N'Học C++ qua các ví dụ', N'<NoiDung>Ví dụ minh họa cho người học C++</NoiDung>', '2023-10-05', N'Đã duyệt', 'CD003', 'TV006'),
('BV009', N'Các Nguyên Tắc Thiết Kế Cơ Sở Dữ Liệu', N'<NoiDung>Hướng dẫn thiết kế cơ sở dữ liệu hiệu quả</NoiDung>', '2023-10-07', N'Đã duyệt', 'CD016', 'TV007'),
('BV010', N'Quản lý dự án Agile', N'<NoiDung>Áp dụng phương pháp Agile trong phát triển phần mềm</NoiDung>', '2023-10-10', N'Đã duyệt', 'CD017', 'TV008'),
('BV011', N'Xử lý ngôn ngữ tự nhiên', N'<NoiDung>Tìm hiểu về xử lý ngôn ngữ tự nhiên</NoiDung>', '2023-10-15', N'Đã duyệt', 'CD011', 'TV009'),
('BV012', N'Cảm biến trong IoT', N'<NoiDung>Các loại cảm biến sử dụng trong IoT</NoiDung>', '2023-10-20', N'Đã duyệt', 'CD021', 'TV010'),
('BV013', N'Kỹ thuật phòng thủ mạng nâng cao', N'<NoiDung>Kỹ thuật phòng thủ mạng cho doanh nghiệp</NoiDung>', '2023-10-25', N'Đã duyệt', 'CD007', 'TV001'),
('BV014', N'Kiểm thử phần mềm với JUnit', N'<NoiDung>Hướng dẫn kiểm thử phần mềm bằng JUnit</NoiDung>', '2023-10-28', N'Đã duyệt', 'CD020', 'TV002'),
('BV015', N'Lập trình JavaScript cơ bản', N'<NoiDung>Những khái niệm cơ bản về JavaScript</NoiDung>', '2023-11-01', N'Đã duyệt', 'CD004', 'TV003'),
('BV016', N'Quản trị NoSQL', N'<NoiDung>Hướng dẫn quản trị cơ sở dữ liệu NoSQL</NoiDung>', '2023-11-05', N'Đã duyệt', 'CD014', 'TV004'),
('BV017', N'Trung tâm dữ liệu thời gian thực', N'<NoiDung>Xây dựng trung tâm dữ liệu hiệu quả</NoiDung>', '2023-11-08', N'Đã duyệt', 'CD012', 'TV005'),
('BV018', N'Giới thiệu về TensorFlow', N'<NoiDung>Bài viết giới thiệu TensorFlow</NoiDung>', '2023-11-10', N'Đã duyệt', 'CD010', 'TV006'),
('BV019', N'Lập trình Arduino cơ bản', N'<NoiDung>Hướng dẫn lập trình với Arduino cho người mới</NoiDung>', '2023-11-15', N'Đã duyệt', 'CD024', 'TV007'),
('BV020', N'Kỹ thuật tối ưu SQL', N'<NoiDung>Các kỹ thuật tối ưu truy vấn SQL</NoiDung>', '2023-11-18', N'Đã duyệt', 'CD015', 'TV008'),
('BV021', N'Phân tích dữ liệu với Python', N'<NoiDung>Những công cụ phân tích dữ liệu Python</NoiDung>', '2023-11-20', N'Đã duyệt', 'CD001', 'TV009'),
('BV022', N'Tự động hóa kiểm thử phần mềm', N'<NoiDung>Cách sử dụng công cụ kiểm thử tự động</NoiDung>', '2023-11-22', N'Đã duyệt', 'CD020', 'TV010'),
('BV023', N'An toàn hệ thống với Firewall', N'<NoiDung>Cấu hình và bảo vệ hệ thống với Firewall</NoiDung>', '2023-11-25', N'Đã duyệt', 'CD008', 'TV001'),
('BV024', N'Học sâu cơ bản', N'<NoiDung>Những kiến thức cơ bản về học sâu</NoiDung>', '2023-11-28', N'Đã duyệt', 'CD010', 'TV002');

-- Dữ liệu cho bảng BinhLuan
INSERT INTO BinhLuan (MaBL, IDCha, NoiDung, NgayGui, TrangThai, MaBV, MaTV) VALUES
('BL001',null, N'<NoiDung>Bài viết rất hữu ích</NoiDung>', '2023-09-02', N'Hiển thị', 'BV001', 'TV002'),
('BL002',null, N'<NoiDung>Tôi đã học được nhiều điều mới</NoiDung>', '2024-10-02', N'Hiển thị', 'BV002', 'TV001'),
('BL003', NULL, N'<NoiDung>Tôi đồng ý với quan điểm của bạn</NoiDung>', '2023-09-03', N'Hiển thị', 'BV001', 'TV003'),
('BL004', NULL, N'<NoiDung>Những kiến thức này rất hữu ích</NoiDung>', '2023-09-04', N'Hiển thị', 'BV002', 'TV004'),
('BL005', NULL, N'<NoiDung>Tôi cần tìm hiểu thêm về chủ đề này</NoiDung>', '2023-09-05', N'Hiển thị', 'BV003', 'TV005'),
('BL006', NULL, N'<NoiDung>Cảm ơn bạn vì thông tin hữu ích</NoiDung>', '2023-09-06', N'Hiển thị', 'BV004', 'TV006'),
('BL007', NULL, N'<NoiDung>Rất bổ ích!</NoiDung>', '2023-09-07', N'Hiển thị', 'BV001', 'TV007'),
('BL008', NULL, N'<NoiDung>Những điểm này cần bổ sung thêm</NoiDung>', '2023-09-08', N'Hiển thị', 'BV002', 'TV008'),
('BL009', NULL, N'<NoiDung>Chờ đợi những phần tiếp theo</NoiDung>', '2023-09-09', N'Hiển thị', 'BV003', 'TV009'),
('BL010', NULL, N'<NoiDung>Bài viết rất chi tiết và dễ hiểu</NoiDung>', '2023-09-10', N'Hiển thị', 'BV004', 'TV010'),
('BL011', NULL, N'<NoiDung>Bài viết này đã giải đáp thắc mắc của tôi</NoiDung>', '2023-09-11', N'Hiển thị', 'BV001', 'TV002'),
('BL012', NULL, N'<NoiDung>Học hỏi được nhiều điều qua bài viết</NoiDung>', '2023-09-12', N'Hiển thị', 'BV002', 'TV001'),
('BL013', NULL, N'<NoiDung>Tôi rất thích cách diễn đạt của bạn</NoiDung>', '2023-09-13', N'Hiển thị', 'BV003', 'TV003'),
('BL014', NULL, N'<NoiDung>Cần thêm nhiều ví dụ minh hoạ</NoiDung>', '2023-09-14', N'Hiển thị', 'BV004', 'TV004'),
('BL015', NULL, N'<NoiDung>Chủ đề này rất hay</NoiDung>', '2023-09-15', N'Hiển thị', 'BV001', 'TV005'),
('BL016', NULL, N'<NoiDung>Hy vọng có thêm nhiều bài viết về chủ đề này</NoiDung>', '2024-10-02', N'Hiển thị', 'BV004', 'TV006');

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
