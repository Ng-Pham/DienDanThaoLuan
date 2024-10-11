using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DienDanThaoLuan.Models;

namespace DienDanThaoLuan.Controllers
{
    public class UserInfoController : Controller
    {
        private DienDanThaoLuanEntities db = new DienDanThaoLuanEntities();
        // GET: UserInfo

        [Authorize]
        public ActionResult Index()
        {
            string username = User.Identity.Name;

            var member = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());

            if (member == null)
            {
                var admin = db.QuanTriViens.SingleOrDefault(a => a.TenDangNhap.ToLower() == username.ToLower());
                if (admin != null)
                {
                    return View("AdminInfo", admin);
                }
            }
            return View("MemberInfo",member);
        }
        [Authorize]
        [HttpPost]
        public ActionResult UpdateMember(ThanhVien model)
        {
            if (ModelState.IsValid)
            {
                var member = db.ThanhViens.Find(model.MaTV);
                if (member != null)
                {
                    member.HoTen = model.HoTen;
                    member.Email = model.Email;
                    member.GioiTinh = model.GioiTinh;
                    member.SDT = model.SDT;
                    member.NgaySinh = model.NgaySinh;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateAdmin(QuanTriVien model)
        {
            if (ModelState.IsValid)
            {
                var admin = db.QuanTriViens.Find(model.MaQTV);
                if (admin != null)
                {
                    admin.HoTen = model.HoTen;
                    admin.Email = model.Email;
                    admin.SDT = model.SDT;
                    admin.NgaySinh = model.NgaySinh;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            string username = User.Identity.Name;
            var member = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());

            if (member == null)
            {
                var admin = db.QuanTriViens.SingleOrDefault(a => a.TenDangNhap.ToLower() == username.ToLower());
                if (admin != null)
                {
                    if (admin.MatKhau != currentPassword)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                    }
                    else if (newPassword != confirmPassword)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                    }
                    else
                    {
                        admin.MatKhau = newPassword;
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    }
                }
            }
            else
            {
                if (member.MatKhau != currentPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                }
                else if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                }
                else
                {
                    member.MatKhau = newPassword;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                }
            }
            return RedirectToAction("Index");
        }
    }
}