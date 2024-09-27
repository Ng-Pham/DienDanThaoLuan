using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DienDanThaoLuan.Models;

namespace DienDanThaoLuan.Controllers
{
    public class DienDanThaoLuanController : Controller
    {
        // GET: DienDanThaoLuan
        DienDanThaoLuanEntities db = new DienDanThaoLuanEntities();
        public ActionResult Index()
        {
            var currentUser = Session["user"] as ThanhVien;
            if (currentUser != null)
            {
                ViewBag.Username = currentUser.TenDangNhap;
            }
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
                SoBai = db.BaiViets.Count(bv => bv.MaCD == cd.MaCD)
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

        //Dang Nhap && Dang Ky
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            //check null tài khoản && mật khẩu
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.error = "Không được để trống tài khoản hoặc mật khẩu!!!";
                return View();
            }

            //Lấy dữ liệu tài khoản mật khẩu
            var memberAcc = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());

            //Check tồn tại tài khoản
            if (memberAcc == null)
            {
                ViewBag.error = "Tài khoản không tồn tại!!";
                ViewBag.username = username;
                return View();
            }

            //Check đúng sai tài khoản mật khẩu
            if (memberAcc.MatKhau != password || memberAcc.TenDangNhap != username)
            {
                ViewBag.error = "Sai tên tài khoản hoặc mật khẩu!! Vui lòng thử lại";
                ViewBag.username = username;
                return View();
            }

            FormsAuthentication.SetAuthCookie(username, false);
            return  RedirectToAction("Index");
        }
        //Đăng xuất 
        public ActionResult Logout()
        {
            Session.Remove("user");
            return RedirectToAction("Login  ");
        }

    }
}