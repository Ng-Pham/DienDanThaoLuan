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
using System.Data.SqlClient;

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
            var userId = Session["UserId"] as string;
            if (userId != null)
            {
                var dstb = db.ThongBaos.Where(n => n.MaTV == userId).OrderByDescending(n => n.NgayTB).ToList();

                var slchuadoc = dstb.Count(n => n.TrangThai == false);
                if(slchuadoc !=0 )
                    ViewBag.UnreadCount = slchuadoc;
            }
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
                            SoBL = db.BinhLuans.Count(bl => bl.MaBV == bv.MaBV),
                            TrangThaiBV = bv.TrangThai
                        })
                         .ToList();

            return dsbv; // Trả về danh sách bài viết
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

        private List<BaiVietView> LayDanhSachBinhLuan(string maBV)
        {
            var blList = db.Database.SqlQuery<BinhLuan>(
                @"WITH RecursiveComments AS (
                    SELECT MaBL, IDCha, CAST(NoiDung AS NVARCHAR(MAX)) AS NoiDung, NgayGui, MaTV, MaBV, TrangThai
                    FROM BinhLuan
                    WHERE MaBV = @maBV AND IDCha IS NULL

                    UNION ALL

                    SELECT bl.MaBL, bl.IDCha, CAST(bl.NoiDung AS NVARCHAR(MAX)) AS NoiDung, bl.NgayGui, bl.MaTV, bl.MaBV, bl.TrangThai
                    FROM BinhLuan bl
                    INNER JOIN RecursiveComments rc ON bl.IDCha = rc.MaBL
                )

                SELECT MaBL, IDCha, NoiDung, NgayGui, MaTV, MaBV, TrangThai
                FROM RecursiveComments
                ",
                new SqlParameter("@maBV", maBV)
            ).ToList();
            var dsbl = blList.Select(bl => new BaiVietView
            {
                // Gán các giá trị cần thiết từ BinhLuan
                MaBL = bl.MaBL,
                NDBL = bl.NoiDung,
                NgayGui = bl.NgayGui ?? DateTime.Now,
                MaTVGui = bl.MaTV,
                TenTV = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV)
                             .Select(tv => tv.TenDangNhap)
                             .FirstOrDefault(),
                avTvBl = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV)
                              .Select(tv => tv.AnhDaiDien)
                              .FirstOrDefault(),
                ReplyToContent = db.BinhLuans.Where(r => r.MaBL == bl.IDCha)
                                     .Select(r => r.NoiDung)
                                     .FirstOrDefault(),
            // Các giá trị khác nếu cần thêm
                MaBV = bl.MaBV,
                IDCha = bl.IDCha
            }).ToList();

            foreach (var bl in dsbl)
            {
                var (noiDungVanBan, codeContent) = XuLyNoiDungXML(bl.NDBL);
                bl.NDBL = noiDungVanBan;
                bl.CodeContent = codeContent;
                if (!string.IsNullOrEmpty(bl.ReplyToContent))
                {
                    var (noiDung, code) = XuLyNoiDungXML(bl.ReplyToContent);
                    bl.ReplyToContent = noiDung;
                }
            }

            return dsbl;
        }
        public string XuLyNoiDung(string noiDung, string codeContent)
        {
            // Decode the content from the request
            var decodedCodeContent = HttpUtility.UrlDecode(codeContent ?? string.Empty); // Ensure not null
            var decodedNoiDung = HttpUtility.UrlDecode(noiDung);

            string xmlString;
            if (!string.IsNullOrEmpty(decodedCodeContent))
            {
                xmlString = $"<NoiDung>{HttpUtility.HtmlDecode(decodedNoiDung)}<Code><![CDATA[{decodedCodeContent}]]></Code></NoiDung>";
            }
            else
            {
                xmlString = $"<NoiDung>{HttpUtility.HtmlDecode(decodedNoiDung)}</NoiDung>";
            }

            // Parse the XML string
            XElement xmlContent = XElement.Parse(xmlString);
            return xmlContent.ToString(); // Return as string
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
            var tb = db.ThongBaos.Where(t => t.LoaiDoiTuong==null)
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

        public ActionResult Loc(string sortOrder, string tenloai, string id, bool isAllPosts = false)
        {
            ViewBag.NewestSort = sortOrder == "newest" ? "newest_desc" : "newest";
            ViewBag.OldestSort = sortOrder == "oldest" ? "oldest_desc" : "oldest";
            var baiVietViewList = LayTatCaBaiViet();
            if (!isAllPosts)
            {
                baiVietViewList = baiVietViewList.Where(b => b.MaCD == id).ToList();
            }
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
                if (!isAllPosts)
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
            }

            return View(isAllPosts ? "BaiVietMoi" : "BaiVietTheoCD", baiVietViewList);
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
            if (ModelState.IsValid && !string.IsNullOrEmpty(post.NoiDung))
            {
                try
                {
                    
                    var nd = XuLyNoiDung(post.NoiDung, Request.Unvalidated.Form["CodeContent"]); // Store as string in database
                                                          // Tạo mã bài viết tự động
                    var lastPost = db.BaiViets.OrderByDescending(b => b.MaBV).FirstOrDefault();
                    string newMaBV = "BV" + (Convert.ToInt32(lastPost.MaBV.Substring(2)) + 1).ToString("D3");

                    post.MaBV = newMaBV; // Gán mã bài viết mới
                    post.NgayDang = DateTime.Now; // Gán ngày đăng bài viết
                    post.TrangThai = "Chờ duyệt"; // Gán trạng thái bài viết
                    post.MaTV = userId; // Gán mã thành viên
                    post.NoiDung = nd;

                    db.BaiViets.Add(post);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Bài viết đã được gửi chờ xét duyệt thành công!";

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
            ViewBag.Loi = "Vui lòng điền đầy đủ thông tin!";
            
            // Nếu model không hợp lệ hoặc có lỗi, quay lại view
            var cd = db.ChuDes.ToList();
            ViewBag.MaCD = new SelectList(cd, "MaCD", "TenCD");
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
        public ActionResult NDBaiViet(string id)
        {
            var nd = db.BaiViets.FirstOrDefault(ndct => ndct.MaBV == id);
            var (noiDungVanBan, codeContent) = XuLyNoiDungXML(nd.NoiDung);

            // Lưu nội dung vào ViewBag
            ViewBag.NoiDungVanBan = noiDungVanBan;
            ViewBag.CodeContent = codeContent; 
           
            var ngviet = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == nd.MaTV);
            ViewBag.TVVietBai = ngviet;

            var chuDe = LayThongTinCD().Where(cd => cd.MaCD == nd.MaCD).SingleOrDefault();
            if (chuDe != null)
            {
                ViewBag.maloai = chuDe.MaLoai;
                ViewBag.tenloai = chuDe.TenLoai;
                ViewBag.macd = chuDe.MaCD;
                ViewBag.tencd = chuDe.TenCD;
            }
            return View(nd);
        }
        [Authorize]
        public ActionResult ThongBao()
        {
            string Id;
            Id = Session["UserId"] as string;
            if(string.IsNullOrEmpty(Id))
            {
                Id = Session["AdminId"] as string;
            }

            var dstb = db.ThongBaos.Where(n => n.MaTV == Id).OrderByDescending(n => n.NgayTB).ToList();

            foreach (var thongBao in dstb)
            {
                if (!string.IsNullOrEmpty(thongBao.NoiDung))
                {
                    try
                    {
                        var (noiDungVanBan, codeContent) = XuLyNoiDungXML(thongBao.NoiDung);

                        // Lưu nội dung vào ViewBag
                        thongBao.NoiDung = noiDungVanBan;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            if (!dstb.Any())
                ViewBag.Message = "Không có thông báo nào gần đây";
            return View(dstb);
        }
        public ActionResult MarkAsRead(string id)
        {
            var tb = db.ThongBaos.Find(id);
            if (tb != null)
            {
                tb.TrangThai = true;
                db.SaveChanges(); 
            }
            if (tb.LoaiDoiTuong == "BaiViet")
            {
                return RedirectToAction("NDBaiViet", new {id = tb.MaDoiTuong});
            }
            if(tb.LoaiDoiTuong == "BinhLuan")
            {
                var baiViet = db.BinhLuans.FirstOrDefault(bv => bv.MaBL == tb.MaDoiTuong);
                TempData["BinhLuanId"] = tb.MaDoiTuong;
                return RedirectToAction("NDBaiViet", new { id = baiViet.MaBV });
            }
            return RedirectToAction("ThongBao");
        }
        public ActionResult BaiVietMoi()
        {
            var dsbv = LayTatCaBaiViet().Where(bv => bv.TrangThaiBV == "Đã duyệt").OrderByDescending(n => n.NgayDang).ToList();
            return View(dsbv);
        }
        public ActionResult PartialBinhLuan(string id)
        {
            var userId = Session["UserId"] as string;
            ViewBag.MaBV = id;
            if (userId == null)
            {
                ViewBag.User = null;
                return PartialView();
            }
            var tk = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == userId);

            return PartialView(tk);
        }
        public ActionResult PartialDSBL(string id)
        {
            var dsbl = LayDanhSachBinhLuan(id).OrderByDescending(bl => bl.NgayGui).ToList();
            ViewBag.MaBV = id;
            return PartialView(dsbl);
        }
        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult ThemBL(BinhLuan bl)
        {
            var userId = Session["UserId"] as string;
            if (ModelState.IsValid && !string.IsNullOrEmpty(bl.NoiDung))
            {
                try
                {
                    var nd = XuLyNoiDung(bl.NoiDung, Request.Unvalidated.Form["CodeContent"]); // Store as string in database
                                                                                               // Tạo mã bài viết tự động
                    var lastCmt = db.BinhLuans.OrderByDescending(c => c.MaBL).FirstOrDefault();
                    string newMaBL = "BL" + (Convert.ToInt32(lastCmt.MaBL.Substring(2)) + 1).ToString("D3");

                    bl.MaBL = newMaBL; // Gán mã bài viết mới
                    bl.NgayGui = DateTime.Now; // Gán ngày đăng bài viết
                    bl.TrangThai = "Hiển thị"; // Gán trạng thái bài viết
                    bl.MaTV = userId; // Gán mã thành viên
                    bl.NoiDung = nd;
                    if (string.IsNullOrEmpty(bl.IDCha))
                    {
                        bl.IDCha = null;
                    }
                    else
                    {
                        bl.IDCha = bl.IDCha;
                    }
                    bl.MaBV = bl.MaBV;

                    db.BinhLuans.Add(bl);
                    db.SaveChanges();

                    var maTV = db.BaiViets.Where(bv => bv.MaBV == bl.MaBV).Select(bv => bv.MaTV).FirstOrDefault();
                    GuiThongBao("Bình luận", maTV, bl.MaBL, "BinhLuan");
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi trong quá trình xử lý XML hoặc lưu vào database
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình lưu: " + ex.Message);
                }
            }
            else
            {
                TempData["Loi"] = "Vui lòng điền đầy đủ thông tin!";
            }
            return RedirectToAction("NDBaiViet", new { id = bl.MaBV });
        }
        public void GuiThongBao(string loaitb,string maTVNhan, string maDoiTuong, string loaidt)
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
            if( loaidt == "BinhLuan")
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
            else if(loaidt == "BaiViet")
            {
                var tieuDeBV = db.BaiViets.Where(bv => bv.MaBV == maDoiTuong).Select(bv => bv.TieuDeBV).FirstOrDefault();
                thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn đã được phê duyệt.</NoiDung>";
            }    
            // Lưu thông báo vào cơ sở dữ liệu
            db.ThongBaos.Add(thongBao);
            db.SaveChanges();
        }
        [HttpGet]
        [Authorize]
        public ActionResult GopY()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GopY(GopY gopY)
        {
            var userId = Session["UserId"] as string;
            if (ModelState.IsValid && !string.IsNullOrEmpty(gopY.NoiDung))
            {
                var nd = XuLyNoiDung(gopY.NoiDung, null);

                gopY.NoiDung = nd;
                gopY.NgayGui = DateTime.Now;
                gopY.MaTV = userId;
                gopY.TrangThai = false;

                db.Gopies.Add(gopY);
                db.SaveChanges();
                ViewBag.ThongBao = "Đã gửi góp ý! Cảm ơn bạn đã gửi góp ý!";
            }
            else
            {
                ViewBag.Loi = "Vui lòng điền đầy đủ thông tin!";
            }    
            return View();
        }
    }
}