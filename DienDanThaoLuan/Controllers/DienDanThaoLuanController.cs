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
        public ActionResult _PartialCDThaoLuanNhieu()
        {
            var chuDeDuocThaoLuanNhieu = db.ChuDes
            .Select(cd => new SoBaiChuDe
            {
                TenCD = cd.TenCD,
                SoBai = db.BaiViets.Count(bv => bv.MaCD == cd.MaCD),
                MaLoai=cd.MaLoai
            })
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
                MaQTV = t.MaQTV,
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

            var c = db.ChuDes.Where(cd => loaiChuDeIds.Contains(cd.MaLoai))
            .Join(db.LoaiCDs, cd => cd.MaLoai, loai => loai.MaLoai, (cd, loai) => new SoBaiChuDe
            {
                TenLoai = loai.TenLoai,
                TenCD= cd.TenCD,
                MaLoai = loai.MaLoai,
                SoBai= db.BaiViets.Count(bv => bv.MaCD == cd.MaCD)
            }).ToList();
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
            var dscd = db.ChuDes.Where(cd => cd.MaLoai == id)
            .Join(db.LoaiCDs, cd => cd.MaLoai, loai => loai.MaLoai, (cd, loai) => new SoBaiChuDe
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                TenCD = cd.TenCD,
                MaCD = cd.MaCD,
                SoBai = db.BaiViets.Count(bv => bv.MaCD == cd.MaCD)
            }).ToList();
            return View(dscd);
        }
        [HttpGet]
        public ActionResult BaiVietTheoCD(string id, string tenloai)
        {
            var dsbv = db.BaiViets.Where(bv => bv.MaCD == id)
            .Join(db.ChuDes, bv => bv.MaCD, cd => cd.MaCD, (bv, cd) => new BaiVietView
            {
                MaLoai = cd.MaLoai,
                TenLoai = tenloai,
                MaCD = cd.MaCD,
                TenCD = cd.TenCD,
                MaBV = bv.MaBV,
                TieuDe = bv.TieuDeBV,
                ND = bv.NoiDung,
                TenTV = db.ThanhViens.Where(tv => tv.MaTV == bv.MaTV).Select(tv => tv.TenDangNhap).FirstOrDefault(),
                NgayDang = bv.NgayDang ?? DateTime.Now,
                SoBL = db.BinhLuans.Count(bl => bl.MaBV == bv.MaBV)
            })
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
            ViewBag.TitleSort = sortOrder == "az" ? "za" : "az";

            var posts = from p in db.BaiViets where p.MaCD == id
                        select p;

            switch (sortOrder)
            {
                case "newest":
                    posts = posts.OrderByDescending(p => p.NgayDang);
                    break;
                case "oldest":
                    posts = posts.OrderBy(p => p.NgayDang);
                    break;
                case "az":
                    posts = posts.OrderBy(p => p.TieuDeBV);
                    break;
                case "za":
                    posts = posts.OrderByDescending(p => p.TieuDeBV);
                    break;
                default:
                    break;
            }
            var baiVietViewList = posts.Select(b => new BaiVietView
            {
                MaBV = b.MaBV,
                TieuDe = b.TieuDeBV,
                NgayDang = b.NgayDang??DateTime.Now,
                SoBL = db.BinhLuans.Count(bl => bl.MaBV == b.MaBV),
                MaLoai = maloai,
                TenLoai = tenloai,
                MaCD = b.MaCD,
                TenCD = db.ChuDes.Where(cd => cd.MaCD == b.MaCD).Select(cd => cd.TenCD).FirstOrDefault(),
                TenTV = db.ThanhViens.Where(tv => tv.MaTV == b.MaTV).Select(tv => tv.TenDangNhap).FirstOrDefault(),
            }).ToList();
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
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(nd.NoiDung);
            foreach (XmlNode imgNode in xmlDoc.SelectNodes("//img"))
            {
                if (imgNode.Attributes["src"] != null)
                {
                    // Lấy giá trị src hiện tại
                    var srcimg = imgNode.Attributes["src"].Value;
                    // Kiểm tra và loại bỏ dấu .. ở đầu src
                    if (srcimg.StartsWith(".."))
                    {
                        srcimg = srcimg.Substring(2); // Loại bỏ 2 ký tự đầu tiên
                    }
                    // Cập nhật lại thuộc tính src
                    imgNode.Attributes["src"].Value = srcimg;
                }
            }
            var content = xmlDoc.SelectSingleNode("//NoiDung")?.InnerXml; // Lấy nội dung HTML
            ViewBag.NoiDung = content;

            var dsbl = db.BinhLuans.Where(bl => bl.MaBV == id) // Chỉ lấy bình luận của bài viết này
            .Select(bl => new BaiVietView
            {
                NDBL = bl.NoiDung,
                TVGui = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV) // Lấy tên người bình luận
                                    .Select(tv => tv.TenDangNhap)
                                    .FirstOrDefault(),
                avTvBl =  db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV) // Lấy tên người bình luận
                                    .Select(tv => tv.AnhDaiDien)
                                    .FirstOrDefault(),
                NgayGui = bl.NgayGui??DateTime.Now
            }).OrderByDescending(bl => bl.NgayGui).ToList();
            foreach (var bl in dsbl)
            {
                var blXmlDoc = new XmlDocument();
                blXmlDoc.LoadXml(bl.NDBL);
                bl.NDBL = blXmlDoc.SelectSingleNode("//NoiDung")?.InnerXml;
            }
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
                    var lastTV = db.BaiViets.OrderByDescending(b => b.MaTV).FirstOrDefault();
                    string newMaTV = "TV" + (Convert.ToInt32(lastTV.MaTV.Substring(2)) + 1).ToString("D3");
                    // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                    var existingUser = db.ThanhViens.FirstOrDefault(x => x.TenDangNhap == tv.TenDangNhap);
                    if (existingUser != null)
                    {
                        ViewBag.error = "Tên đăng nhập đã tồn tại!! Vui lòng thử lại";
                        ViewBag.tv.TenDangNhap = tv.TenDangNhap;
                        return View(tv);
                    }

                    tv.MaTV = newMaTV;

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