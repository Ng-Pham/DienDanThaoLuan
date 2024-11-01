using System;
using System.Collections.Generic;
using System.IO;
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

        //Thay đổi ảnh đại diện (Avatar)
        [Authorize]
        [HttpPost]
        public ActionResult ChangeAvatar(HttpPostedFileBase avatar)
        {
            if (avatar != null && avatar.ContentLength > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatar.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif";
                    return RedirectToAction("Index");
                }

                // Kiểm tra kích thước file (giới hạn 5MB)
                if (avatar.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước file không được vượt quá 5MB";
                    return RedirectToAction("Index");
                }

                try
                {
                    string username = User.Identity.Name;
                    var member = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());
                    string fileName;

                    if (member != null)
                    {
                        // Xử lý cho Thành viên
                        fileName = $"avatar_member_{member.MaTV}_{DateTime.Now.Ticks}{fileExtension}";
                        string path = Path.Combine(Server.MapPath("~/Images"), fileName);

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(member.AnhDaiDien) && member.AnhDaiDien != "avatar.jpg")
                        {
                            string oldPath = Path.Combine(Server.MapPath("~/Images"), member.AnhDaiDien);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Lưu file mới
                        avatar.SaveAs(path);
                        member.AnhDaiDien = fileName;
                        db.SaveChanges();
                    }
                    else
                    {
                        // Kiểm tra nếu là Admin
                        var admin = db.QuanTriViens.SingleOrDefault(a => a.TenDangNhap.ToLower() == username.ToLower());
                        if (admin != null)
                        {
                            // Xử lý cho Quản trị viên
                            fileName = $"avatar_admin_{admin.MaQTV}_{DateTime.Now.Ticks}{fileExtension}";
                            string path = Path.Combine(Server.MapPath("~/Images"), fileName);

                            // Xóa ảnh cũ nếu có
                            if (!string.IsNullOrEmpty(admin.AnhDaiDien) && admin.AnhDaiDien != "avatar.jpg")
                            {
                                string oldPath = Path.Combine(Server.MapPath("~/Images"), admin.AnhDaiDien);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }

                            // Lưu file mới
                            avatar.SaveAs(path);
                            admin.AnhDaiDien = fileName;
                            db.SaveChanges();
                        }
                    }

                    TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh đại diện: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file ảnh";
            }

            return RedirectToAction("Index");
        }
        //Thay đổi ảnh bìa (Cover)
        [Authorize]
        [HttpPost]
        public ActionResult ChangeCover(HttpPostedFileBase cover)
        {
            if (cover != null && cover.ContentLength > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(cover.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif";
                    return RedirectToAction("Index");
                }

                // Kiểm tra kích thước file (giới hạn 5MB)
                if (cover.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước file không được vượt quá 5MB";
                    return RedirectToAction("Index");
                }

                try
                {
                    string username = User.Identity.Name;
                    var member = db.ThanhViens.SingleOrDefault(m => m.TenDangNhap.ToLower() == username.ToLower());
                    string fileName;

                    if (member != null)
                    {
                        // Xử lý cho Thành viên
                        fileName = $"cover_member_{member.MaTV}_{DateTime.Now.Ticks}{fileExtension}";
                        string path = Path.Combine(Server.MapPath("~/Images"), fileName);

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(member.AnhBia) && member.AnhBia != "defaulth-bg.jpg")
                        {
                            string oldPath = Path.Combine(Server.MapPath("~/Images"), member.AnhBia);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Lưu file mới
                        cover.SaveAs(path);
                        member.AnhBia = fileName;
                    }
                    else
                    {
                        // Kiểm tra nếu là Admin
                        var admin = db.QuanTriViens.SingleOrDefault(a => a.TenDangNhap.ToLower() == username.ToLower());
                        if (admin != null)
                        {
                            // Xử lý cho Quản trị viên
                            fileName = $"cover_admin_{admin.MaQTV}_{DateTime.Now.Ticks}{fileExtension}";
                            string path = Path.Combine(Server.MapPath("~/Images"), fileName);

                            // Xóa ảnh cũ nếu có
                            if (!string.IsNullOrEmpty(admin.AnhBia) && admin.AnhBia != "defaulth-bg.jpg")
                            {
                                string oldPath = Path.Combine(Server.MapPath("~/Images"), admin.AnhBia);
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }

                            // Lưu file mới
                            cover.SaveAs(path);
                            admin.AnhBia = fileName;
                        }
                    }

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật ảnh bìa thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh bìa: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file ảnh";
            }

            return RedirectToAction("Index");
        }
    }
}