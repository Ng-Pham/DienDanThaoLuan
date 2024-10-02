using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using DienDanThaoLuan.Models;
using System.Web.Security;

namespace DienDanThaoLuan.Controllers
{
    public class DienDanThaoLuanController : Controller
    {
        // GET: DienDanThaoLuan
        DienDanThaoLuanEntities db = new DienDanThaoLuanEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _PartialHeader()
        {
            return PartialView();
        }
        public ActionResult _PartialMenu()
        {
            return PartialView();
        }
        public ActionResult _PartialChuDe()
        {
            var listcd = from cd in db.LoaiCDs select cd;
            return PartialView(listcd);
        }
        private List<SoBaiChuDe> LayThongTinCD()
        {
            var dsttcd = db.ChuDes
            .Join(db.LoaiCDs, cd => cd.MaLoai, loai => loai.MaLoai, (cd, loai) => new SoBaiChuDe
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                TenCD = cd.TenCD,
                MaCD = cd.MaCD,
                SoBai = db.BaiViets.Count(bv => bv.MaCD == cd.MaCD)
            }).ToList();
            return dsttcd;
        }
        private List<BaiVietView> LayTatCaBaiViet()
        {
            var dsbv = (from bv in db.BaiViets
                        join cd in db.ChuDes on bv.MaCD equals cd.MaCD
                        join loai in db.LoaiCDs on cd.MaLoai equals loai.MaLoai 
                        select new BaiVietView
                        {
                            MaLoai = cd.MaLoai,
                            TenLoai = loai.TenLoai, 
                            MaCD = cd.MaCD,
                            TenCD = cd.TenCD,
                            MaBV = bv.MaBV,
                            TieuDe = bv.TieuDeBV,
                            ND = bv.NoiDung,
                            TenTV = db.ThanhViens.Where(tv => tv.MaTV == bv.MaTV).Select(tv => tv.TenDangNhap).FirstOrDefault(),
                            NgayDang = bv.NgayDang ?? DateTime.Now,
                            SoBL = db.BinhLuans.Count(bl => bl.MaBV == bv.MaBV)
                        })
                         .ToList();

            return dsbv; // Trả về danh sách bài viết
        }
        private string XuLyNoiDungXML(string noiDungXml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(noiDungXml);
            foreach (XmlNode imgNode in xmlDoc.SelectNodes("//img"))
            {
                if (imgNode.Attributes["src"] != null)
                {
                    var srcimg = imgNode.Attributes["src"].Value;
                    if (srcimg.StartsWith(".."))
                    {
                        srcimg = srcimg.Substring(2);
                    }
                    imgNode.Attributes["src"].Value = srcimg;
                }
            }
            return xmlDoc.SelectSingleNode("//NoiDung")?.InnerXml;
        }
        private List<BaiVietView> LayDanhSachBinhLuan(string maBV)
        {
            var dsbl = db.BinhLuans.Where(bl => bl.MaBV == maBV)
            .Select(bl => new BaiVietView
            {
                NDBL = bl.NoiDung,
                TenTV = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV)
                                     .Select(tv => tv.TenDangNhap)
                                     .FirstOrDefault(),
                avTvBl = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV)
                                      .Select(tv => tv.AnhDaiDien)
                                      .FirstOrDefault(),
                NgayGui = bl.NgayGui ?? DateTime.Now
            }).OrderByDescending(bl => bl.NgayGui).ToList();

            foreach (var bl in dsbl)
            {
                bl.NDBL = XuLyNoiDungXML(bl.NDBL); 
            }

            return dsbl;
        }


        public ActionResult _PartialCDThaoLuanNhieu()
        {
            var chuDeDuocThaoLuanNhieu = LayThongTinCD()
            .OrderByDescending(cd => cd.SoBai)
            .Take(3) // Lấy 3 chủ đề thảo luận nhiều nhất
            .ToList();

            return PartialView(chuDeDuocThaoLuanNhieu);
        }
        public ActionResult _PartialThongBao()
        {
            var tb = db.ThongBaos
            .Select(t => new  ThongBaoView
            {
                MaTB = t.MaTB,
                NoiDung = t.NoiDung,
                NgayTB = t.NgayTB,
                LoaiTB= "Thông báo hệ thống",
            }).OrderByDescending(t => t.NgayTB).ToList();
            
            return PartialView(tb);
        }
        public ActionResult _PartialBanner()
        {
            return PartialView();
        }
        public ActionResult _PartialMotSoCD()
        {
            var loaiChuDeIds = db.LoaiCDs
            .Where(l => l.TenLoai == "Ngôn ngữ lập trình" || l.TenLoai == "Bảo mật và an ninh mạng")
            .Select(l => l.MaLoai).ToList();

            var c = LayThongTinCD().Where(cd => loaiChuDeIds.Contains(cd.MaLoai)).ToList();
            return PartialView(c);
        }
        public ActionResult PartialQTV()
        {
            var q = from qtv in db.QuanTriViens select qtv;
            return PartialView(q);
        }
        public ActionResult _PartialFooter()
        {
            return PartialView();
        }
        [HttpGet]
        //Dang Nhap && Dang Ky
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            //check null tài khoản && mật khẩu
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.error = "*Không được để trống tài khoản hoặc mật khẩu!!!";
                return View();
            }
            //Lấy dữ liệu tài khoản mật khẩu
            var memberAcc = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());

            //Check tồn tại tài khoản
            if (memberAcc == null)
            {
                var adminAcc = db.QuanTriViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());

                // Check tồn tại tài khoản trong bảng QuanTriVien
                if (adminAcc == null)
                {
                    ViewBag.error = "Tài khoản không tồn tại!!";
                    ViewBag.username = username;
                    return View();
                }

                // Check đúng sai tài khoản mật khẩu của QuanTriVien
                if (adminAcc.MatKhau != password)
                {
                    ViewBag.error = "Sai tên tài khoản hoặc mật khẩu!! Vui lòng thử lại";
                    ViewBag.username = username;
                    return View();
                }

                // Đăng nhập thành công với tài khoản QuanTriVien
                FormsAuthentication.SetAuthCookie(username, false);
                Session["AdminId"] = adminAcc.MaQTV;
                return RedirectToAction("Index", "DienDanThaoLuan");
            }
            //Check đúng sai tài khoản mật khẩu
            if (memberAcc.MatKhau != password || memberAcc.TenDangNhap != username)
            {
                ViewBag.error = "Sai tên tài khoản hoặc mật khẩu!! Vui lòng thử lại";
                ViewBag.username = username;
                return View();
            }
            //Đăng nhập thành công
            FormsAuthentication.SetAuthCookie(username, false);
            Session["UserId"] = memberAcc.MaTV;
            return  RedirectToAction("Index");
        }
        //Đăng xuất 
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
        public ActionResult ChuDe(string id)
        {
            var dscd = LayThongTinCD().Where(cd => cd.MaLoai == id).ToList();
            return View(dscd);
        }
        [HttpGet]
        public ActionResult BaiVietTheoCD(string id, string tenloai)
        {
            var dsbv = LayTatCaBaiViet().Where(bv => bv.MaCD == id)
            .OrderByDescending(bv => bv.NgayDang)
            .ToList();
            if (!dsbv.Any())
            {
                var cd = db.ChuDes.FirstOrDefault(c => c.MaCD == id);
                if (cd != null)
                {
                    ViewBag.MaCD = cd.MaCD;
                    ViewBag.TenCD = cd.TenCD;
                    ViewBag.TenLoai = tenloai;
                    ViewBag.MaLoai= cd.MaLoai;
                }
                ViewBag.Message = "Chưa có bài viết nào cho chủ đề này";
            }
            return View(dsbv);
        }

        public ActionResult Loc(string sortOrder, string maloai, string tenloai, string id)
        {
            ViewBag.NewestSort = sortOrder == "newest" ? "newest_desc" : "newest";
            ViewBag.OldestSort = sortOrder == "oldest" ? "oldest_desc" : "oldest";
            var baiVietViewList = LayTatCaBaiViet()
                .Where(b => b.MaCD == id)
                .ToList();
            switch (sortOrder)
            {
                case "newest":
                    baiVietViewList = baiVietViewList.OrderByDescending(b => b.NgayDang).ToList();
                    break;
                case "oldest":
                    baiVietViewList = baiVietViewList.OrderBy(b => b.NgayDang).ToList();
                    break;
                case "az":
                    baiVietViewList = baiVietViewList.OrderBy(b => b.TieuDe).ToList();
                    break;
                case "za":
                    baiVietViewList = baiVietViewList.OrderByDescending(b => b.TieuDe).ToList();
                    break;
                default:
                    break;
            }

            if (!baiVietViewList.Any())
            {
                var cd = db.ChuDes.FirstOrDefault(c => c.MaCD == id);
                if (cd != null)
                {
                    ViewBag.MaCD = cd.MaCD;
                    ViewBag.TenCD = cd.TenCD;
                    ViewBag.TenLoai = tenloai;
                    ViewBag.MaLoai = cd.MaLoai;
                }
                ViewBag.Message = "Chưa có bài viết nào cho chủ đề này";
            }

            return View("BaiVietTheoCD", baiVietViewList);
        }
        [Authorize]
        [HttpGet]
        public ActionResult ThemBV()
        {
            var cd = db.ChuDes.ToList();
            ViewBag.MaCD = new SelectList(cd, "MaCD", "TenCD");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemBV(BaiViet post)
        {
            var userId = Session["UserId"] as string;
            if (ModelState.IsValid)
            {
                try
                {
                    // Chuyển đổi nội dung bài viết từ chuỗi sang XML
                    XElement xmlContent = XElement.Parse(post.NoiDung); // Assuming NoiDung is a string containing valid XML
                    // Gán nội dung XML vào thuộc tính NoiDung (nếu kiểu dữ liệu trong model là XML)
                    post.NoiDung = xmlContent.ToString(); // Store as string in database
                                                          // Tạo mã bài viết tự động
                    var lastPost = db.BaiViets.OrderByDescending(b => b.MaBV).FirstOrDefault();
                    string newMaBV = "BV" + (Convert.ToInt32(lastPost.MaBV.Substring(2)) + 1).ToString("D3");

                    post.MaBV = newMaBV; // Gán mã bài viết mới
                    post.NgayDang = DateTime.Now; // Gán ngày đăng bài viết
                    post.TrangThai = "Đã duyệt"; // Gán trạng thái bài viết
                    post.MaTV = userId; // Gán mã thành viên
                    post.NoiDung = $"<NoiDung>{post.NoiDung}</NoiDung>";

                    db.BaiViets.Add(post);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Bài viết đã được thêm thành công!";

                    // Quay lại trang trước đó (nếu có)
                    if (Request.UrlReferrer != null)
                    {
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                    else
                    {
                        // Nếu không có trang trước, chuyển hướng đến Index hoặc một trang mặc định
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi trong quá trình xử lý XML hoặc lưu vào database
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình lưu bài viết: " + ex.Message);
                }
            }

            // Nếu model không hợp lệ hoặc có lỗi, quay lại view
            return View(post);
        }
        [HttpPost]
        public JsonResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    int maxWidth = 800; // Chiều rộng tối đa
                    int maxHeight = 600;  // Chiều cao tối đa

                    if (img.Width > maxWidth || img.Height > maxHeight)
                    {
                        return Json(new { error = $"Kích thước hình ảnh vượt quá giới hạn ({maxWidth}x{maxHeight}px)." });
                    }
                }
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("/Upload_images/"), fileName);
                file.SaveAs(path);
                return Json(new { location = Url.Content("/Upload_images/" + fileName) });
            }
            return Json(new { error = "File upload failed." });
        }
        public ActionResult NDBaiViet(string id, string maloai, string tenloai, string macd, string tencd)
        {
            var userId = Session["UserId"] as string;
            var nd = db.BaiViets.FirstOrDefault(ndct => ndct.MaBV == id);
            
            ViewBag.NoiDung = XuLyNoiDungXML(nd.NoiDung);

            var dsbl = LayDanhSachBinhLuan(id).OrderByDescending(bl => bl.NgayGui).ToList();
            ViewBag.BinhLuans = dsbl;

            var ngviet = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == nd.MaTV);
            ViewBag.TVVietBai = ngviet;

            ViewBag.maloai = maloai;
            ViewBag.tenloai = tenloai;
            ViewBag.macd = macd;
            ViewBag.tencd = tencd;
            if (userId != null)
            {
                var tk = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == userId);
                ViewBag.User = tk;
            }
            else
            {
                ViewBag.User = null;
            }

            return View(nd);
        }
        //Đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(ThanhVien tv)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var lastTV = db.ThanhViens.OrderByDescending(t => t.MaTV).FirstOrDefault();
                    string newMaTV = "TV" + (Convert.ToInt32(lastTV.MaTV.Substring(2)) + 1).ToString("D3");
                    // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                    var existingUser = db.ThanhViens.FirstOrDefault(x => x.TenDangNhap == tv.TenDangNhap);
                    if (existingUser != null)
                    {
                        ViewBag.error = "Tên đăng nhập đã tồn tại!! Vui lòng thử lại";
                        ViewBag.tv.TenDangNhap = tv.TenDangNhap;
                        return View(tv);
                    }
                    tv.NgayThamGia = DateTime.Now;
                    tv.MaTV = newMaTV;
                    tv.AnhDaiDien = "avatar.jpg";
                    // Thêm thành viên mới vào database
                    db.ThanhViens.Add(tv);
                    db.SaveChanges();

                    // Điều hướng đến trang thành công hoặc đăng nhập
                    return RedirectToAction("Login", "DienDanThaoLuan");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại! " + ex.Message);
                }
            }
            return View(tv); ;
        }


    }
}