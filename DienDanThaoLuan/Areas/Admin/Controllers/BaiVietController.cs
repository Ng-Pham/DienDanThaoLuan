using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using DienDanThaoLuan.Controllers;
using DienDanThaoLuan.Models;


namespace DienDanThaoLuan.Areas.Admin.Controllers
{
    public class BaiVietController : Controller
    {
        DienDanThaoLuanEntities db = new DienDanThaoLuanEntities();
        // GET: Admin/BaiViet
        public ActionResult Index()
        {
            return View();
        }
        private (string vanBan, string codeContent) XuLyNoiDungXML(string noiDungXml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(noiDungXml);

            // Tìm mã code
            var codeNode = xmlDoc.SelectSingleNode("//Code");
            string codeContent = codeNode != null ? codeNode.InnerText : string.Empty;

            // Lấy nội dung văn bản, bỏ phần mã code
            var noiDungVanBanNode = xmlDoc.SelectSingleNode("//NoiDung");
            string noiDungVanBan = noiDungVanBanNode != null ? noiDungVanBanNode.InnerXml : string.Empty;
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
            // Loại bỏ các thẻ <Code> khỏi nội dung văn bản
            if (!string.IsNullOrEmpty(codeContent))
            {
                noiDungVanBan = noiDungVanBan.Replace(codeNode.OuterXml, string.Empty);
            }

            return (noiDungVanBan, codeContent);
        }
        [HttpGet]
        public ActionResult DuyetBai()
        {
            var dsbv = db.BaiViets.Where(bv => bv.TrangThai == "Chờ duyệt").OrderByDescending(n => n.NgayDang).ToList();
            if (!dsbv.Any())
                ViewBag.Message = "Không có bài viết chờ duyệt nào gần đây";
            return View(dsbv);
        }
        public ActionResult MarkAsRead(string id)
        {
            var tb = db.BaiViets.Find(id);
            if (tb != null)
            {
                tb.TrangThai = "Đã duyệt";
                db.SaveChanges();
            }
            return RedirectToAction("ThongBao");
        }
        public ActionResult PartialThongTinTV(string id)
        {
            var tttv = db.ThanhViens.Where(tv => tv.MaTV == id).FirstOrDefault();
            
            return PartialView(tttv);
        }
        public ActionResult ChiTietBV(string id)
        {
            var ttbv = db.BaiViets.Where(bv => bv.MaBV == id).FirstOrDefault();
            var (noiDungVanBan, codeContent) = XuLyNoiDungXML(ttbv.NoiDung);
            ViewBag.NDVB = noiDungVanBan;
            ViewBag.Code = codeContent;
            return View(ttbv);
        }
        public ActionResult LuuBai(string id)
        {
            var baiviet = db.BaiViets.Find(id);
            var diendanController = new DienDanThaoLuanController();
            baiviet.TrangThai = "Đã duyệt";
            db.SaveChanges();
            var maTV = db.BaiViets.Where(bv => bv.MaBV == id).Select(bv => bv.MaTV).FirstOrDefault();
            diendanController.GuiThongBao("Bài viết", maTV, id, "BaiViet");
            return RedirectToAction("DuyetBai");
        }
    }
}