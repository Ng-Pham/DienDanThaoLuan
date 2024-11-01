using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using DienDanThaoLuan.Controllers;
using DienDanThaoLuan.Models;
using PagedList;


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
        public ActionResult DuyetBai(int? page)
        {
            var dsbv = db.BaiViets.Where(bv => bv.TrangThai == "Chờ duyệt").OrderByDescending(n => n.NgayDang).ToList();
            int iSize = 10;
            int iPageNumber = (page ?? 1);
            if (!dsbv.Any())
                ViewBag.Message = "Không có bài viết chờ duyệt nào gần đây";
            return View(dsbv.ToPagedList(iPageNumber, iSize));
        }
        public ActionResult MarkAsRead(string id)
        {
            var tb = db.BaiViets.Find(id);
            if (tb != null)
            {
                tb.TrangThai = "Đã duyệt";
                db.SaveChanges();
            }
            return RedirectToAction("DuyetBai");
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
        public ActionResult LuuTTBai(string id, string trangthai, string lydo)
        {
            var baiviet = db.BaiViets.Find(id);
            if (trangthai == "duyet")
            {
                baiviet.TrangThai = "Đã duyệt";
            }
            else if(trangthai == "tuChoi")
            {
                baiviet.TrangThai = "Từ chối";
                TempData["LyDoTuChoi"] = lydo;
                var idTV = db.BaiViets.Where(bv => bv.MaBV == id).Select(bv => bv.MaTV).FirstOrDefault();
                GuiThongBao("Từ chối bài viết", idTV, id, "BaiViet");
                return RedirectToAction("DuyetBai");
            }    
            db.SaveChanges();
            var maTV = db.BaiViets.Where(bv => bv.MaBV == id).Select(bv => bv.MaTV).FirstOrDefault();
            GuiThongBao("Bài viết", maTV, id, "BaiViet");
            return RedirectToAction("DuyetBai");
        }
        public void GuiThongBao(string loaitb, string maTVNhan, string maDoiTuong, string loaidt)
        {
            var lastTB = db.ThongBaos.OrderByDescending(c => c.MaTB).FirstOrDefault();
            string newMaTB = "TB" + (Convert.ToInt32(lastTB.MaTB.Substring(2)) + 1).ToString("D3");
            // Tạo thông báo
            ThongBao thongBao = new ThongBao
            {
                MaTB = newMaTB,
                NgayTB = DateTime.Now,
                LoaiTB = loaitb,
                MaTV = maTVNhan,
                MaDoiTuong = maDoiTuong,
                LoaiDoiTuong = loaidt,
                TrangThai = false
            };
            if (loaidt == "BinhLuan")
            {
                if(loaitb == "Xóa bình luận")
                {
                    var lyDoXoa = TempData["lydoxoa"] as string;
                    var maBV = db.BinhLuans.Where(bl => bl.MaBL == maDoiTuong).Select(bl => bl.MaBV).FirstOrDefault();
                    var tieuDeBV = db.BaiViets.Where(bv => bv.MaBV == maBV).Select(bv => bv.TieuDeBV).FirstOrDefault();
                    thongBao.NoiDung = $"<NoiDung>Có bình luận của bạn ở bài viết '{tieuDeBV}' đã bị xóa vì {lyDoXoa}.</NoiDung>";
                    db.SaveChanges();
                } 
                else
                {
                    var maBaiViet = db.BinhLuans.Where(bl => bl.MaBL == maDoiTuong).Select(bl => bl.MaBV).FirstOrDefault();
                    var tieuDeBV = db.BaiViets.Where(bv => bv.MaBV == maBaiViet).Select(bv => bv.TieuDeBV).FirstOrDefault();
                    thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn vừa có bình luận mới.</NoiDung>";
                    db.SaveChanges();
                    var idCha = db.BinhLuans.Where(bl => bl.MaBL == maDoiTuong).Select(bl => bl.IDCha).FirstOrDefault();
                    if (!string.IsNullOrEmpty(idCha))
                    {
                        var replyTV = db.BinhLuans.Where(bl => bl.MaBL == idCha).Select(bl => bl.MaTV).FirstOrDefault();
                        ThongBao replyThongBao = new ThongBao
                        {
                            MaTB = "TB" + (Convert.ToInt32(lastTB.MaTB.Substring(2)) + 2).ToString("D3"), // Tạo mã TB mới cho người reply
                            NgayTB = DateTime.Now,
                            LoaiTB = loaitb,
                            MaTV = replyTV,
                            MaDoiTuong = maDoiTuong,
                            LoaiDoiTuong = loaidt,
                            NoiDung = $"<NoiDung>Bình luận của bạn ở bài viết '{tieuDeBV}' đã có phản hồi mới.</NoiDung>",
                            TrangThai = false
                        };
                        db.ThongBaos.Add(replyThongBao);
                    }
                }    
            }
            else if (loaidt == "BaiViet")
            {
                var tieuDeBV = db.BaiViets.Where(bv => bv.MaBV == maDoiTuong).Select(bv => bv.TieuDeBV).FirstOrDefault();
                if (loaitb == "Bài viết")
                {
                    thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn đã được phê duyệt.</NoiDung>";
                }
                else if (loaitb == "Từ chối bài viết")
                {
                    var lyDoTuChoi = TempData["LyDoTuChoi"] as string;
                    thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn đã bị từ chối vì '{lyDoTuChoi}'</NoiDung>";
                }
                else if(loaitb == "Xóa bài viết")
                {
                    var lyDoXoaBai = TempData["lydoxoa"] as string;
                    thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn đã bị xóa vì '{lyDoXoaBai}'</NoiDung>";
                }    

            }
            // Lưu thông báo vào cơ sở dữ liệu
            db.ThongBaos.Add(thongBao);
            db.SaveChanges();
        }
        public ActionResult XoaBV_BL(string IdDoiTuong, string LyDoXoa)
        {
            TempData["lydoxoa"] = LyDoXoa;
            string idTV;
            var bv = db.BaiViets.Find(IdDoiTuong);
            if (bv == null)
            {
                var bl = db.BinhLuans.Find(IdDoiTuong);
                bl.TrangThai = "Đã xóa";
                idTV = db.BinhLuans.Where(b => b.MaBL == IdDoiTuong).Select(b => b.MaTV).FirstOrDefault();
                GuiThongBao("Xóa bình luận", idTV, IdDoiTuong, "BinhLuan");
                return RedirectToAction("NDBaiViet", "DienDanThaoLuan", new { id = bl.MaBV, area = "" });
            }
            else
            {
                bv.TrangThai = "Đã xóa";
                idTV = db.BaiViets.Where(b => b.MaBV == IdDoiTuong).Select(b => b.MaTV).FirstOrDefault();
                GuiThongBao("Xóa bài viết", idTV, IdDoiTuong, "BaiViet");
            } 
            db.SaveChanges();
            return RedirectToAction("BaiVietMoi", "DienDanThaoLuan", new { area = "" });
        }
    }
}