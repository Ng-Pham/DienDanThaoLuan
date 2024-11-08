using DienDanThaoLuan.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace DienDanThaoLuan.Areas.Admin.Controllers
{
    public class ThongBaoTongController : Controller
    {
        DienDanThaoLuanEntities db = new DienDanThaoLuanEntities();
        // GET: Admin/ThongBaoTong
        public ActionResult Index(int? page)
        {
            var ds = db.ThongBaos.Where(l => l.LoaiTB.Contains("Thông báo hệ thống")).ToList();
            int iSize = 10;
            int iPageNumber = (page ?? 1);
            return View(ds.ToPagedList(iPageNumber, iSize));
        }
        [HttpPost]
        public ActionResult Them(string noidung)
        { 
            try
            { 
                var lasttb = db.ThongBaos.OrderByDescending(c => c.MaTB).FirstOrDefault();
                string newMaTB = "TB" + (Convert.ToInt32(lasttb.MaTB.Substring(2)) + 1).ToString("D3");

                var tbnew = new ThongBao();
                tbnew.MaTB = newMaTB;
                tbnew.NoiDung = $"<NoiDung>{noidung}</NoiDung>";
                tbnew.NgayTB = DateTime.Now;
                tbnew.LoaiTB = "Thông báo hệ thống";
                db.ThongBaos.Add(tbnew);
                db.SaveChanges();

                return Json(new { success = true, message = "Thông báo đã được thêm thành công!" });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu xảy ra
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Sua(string maTB, string noidung)
        {
            try
            {
                var tb = db.ThongBaos.Find(maTB);
                tb.NoiDung = $"<NoiDung>{noidung}</NoiDung>";
                tb.NgayTB = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Thông báo đã được sửa thành công!" });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu xảy ra
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Xoa(string maTB)
        {
            try
            {
                var tb = db.ThongBaos.SingleOrDefault(t => t.MaTB == maTB);
                db.ThongBaos.Remove(tb);
                db.SaveChanges();

                return Json(new { success = true, message = "Thông báo đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu xảy ra
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult ChonHien(string maTB)
        {
            try
            {
                var tb = db.ThongBaos.SingleOrDefault(t => t.MaTB == maTB);
                tb.NgayTB = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Thông báo đã được thay thành công!" });
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu xảy ra
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}