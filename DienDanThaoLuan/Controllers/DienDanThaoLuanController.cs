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
using PagedList;
using System.Web.UI;
using HtmlAgilityPack;

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
            var adId = Session["AdminId"] as string;
            if (userId != null)
            {
                var dstb = db.ThongBaos.Where(n => n.MaTV == userId).OrderByDescending(n => n.NgayTB).ToList();

                var slchuadoc = dstb.Count(n => n.TrangThai == false);
                if(slchuadoc !=0 )
                    ViewBag.UnreadCount = slchuadoc;
            }
            if (adId != null)
            {
                var dstb = db.ThongBaos.Where(n => n.MaQTV == adId || n.LoaiTB.Contains("Tố cáo")).OrderByDescending(n => n.NgayTB).ToList();

                var slchuadoc = dstb.Count(n => n.TrangThai == false);
                if (slchuadoc != 0)
                    ViewBag.UnreadCount = slchuadoc;

                var tb = db.BaiViets.Where(n => n.TrangThai.Contains("Chờ duyệt")).OrderByDescending(n => n.NgayDang).ToList();

                var slchuaduyet = tb.Count();
                if (slchuaduyet != 0)
                    ViewBag.SLBV = slchuaduyet;
                var dsgy = db.Gopies.Where(l => l.TrangThai == false).OrderByDescending(l => l.NgayGui).ToList();
                var slgy = dsgy.Count();
                if (slgy != 0)
                    ViewBag.SLGY = slgy;
            }
            return PartialView();
        }
        public ActionResult _PartialChuDe()
        {
            var listcd = db.LoaiCDs.OrderBy(c => c.TenLoai);
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
                SoBai = db.BaiViets.Where(bv => bv.TrangThai.Contains("Đã duyệt")).Count(bv => bv.MaCD == cd.MaCD)
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
                            TenNguoiViet = bv.MaTV != null
                                            ? db.ThanhViens.Where(tv => tv.MaTV == bv.MaTV).Select(tv => tv.TenDangNhap).FirstOrDefault()
                                            : db.QuanTriViens.Where(qtv => qtv.MaQTV == bv.MaQTV).Select(qtv => qtv.TenDangNhap).FirstOrDefault(),
                            NgayDang = bv.NgayDang ?? DateTime.Now,
                            SoBL = db.BinhLuans.Count(bl => bl.MaBV == bv.MaBV),
                            TrangThaiBV = bv.TrangThai,
                            IsAdmin = bv.MaQTV != null
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

            foreach (XmlNode imgNode in xmlDoc.SelectNodes("//img"))
            {
                if (imgNode.Attributes["src"] != null)
                {
                    var srcimg = imgNode.Attributes["src"].Value;
                    // Thay thế đường dẫn để đảm bảo không có ký tự ".."
                    srcimg = srcimg.Replace("../", ""); // Loại bỏ dấu ".." từ đường dẫn
                    imgNode.Attributes["src"].Value = Url.Content("/" + srcimg);
                }
            }
            // Lấy nội dung văn bản, bỏ phần mã code
            var noiDungVanBanNode = xmlDoc.SelectSingleNode("//NoiDung");
            string noiDungVanBan = noiDungVanBanNode != null ? noiDungVanBanNode.InnerXml : string.Empty;
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
                    SELECT MaBL, IDCha, CAST(NoiDung AS NVARCHAR(MAX)) AS NoiDung, NgayGui, MaTV, MaBV, TrangThai, MaQTV
                    FROM BinhLuan
                    WHERE MaBV = @maBV AND IDCha IS NULL AND TrangThai <> N'Đã xóa'

                    UNION ALL

                    SELECT bl.MaBL, bl.IDCha, CAST(bl.NoiDung AS NVARCHAR(MAX)) AS NoiDung, bl.NgayGui, bl.MaTV, bl.MaBV, bl.TrangThai, bl.MaQTV
                    FROM BinhLuan bl
                    INNER JOIN RecursiveComments rc ON bl.IDCha = rc.MaBL
                )

                SELECT MaBL, IDCha, NoiDung, NgayGui, MaTV, MaBV, TrangThai, MaQTV
                FROM RecursiveComments
                WHERE TrangThai <> N'Đã xóa'
                ",
                new SqlParameter("@maBV", maBV)
            ).ToList();
            var dsbl = blList.Select(bl => {
                // Biến tạm để lưu các giá trị cần thiết
                string maNgGui;
                string tenNgGui;
                string avNgBl;
                bool isAd;
                if (bl.MaTV != null)
                {
                    maNgGui = bl.MaTV;
                    tenNgGui = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV).Select(tv => tv.TenDangNhap).FirstOrDefault();
                    avNgBl = db.ThanhViens.Where(tv => tv.MaTV == bl.MaTV).Select(tv => tv.AnhDaiDien).FirstOrDefault();
                    isAd = false;
                }
                else
                {
                    maNgGui = bl.MaQTV;
                    tenNgGui = db.QuanTriViens.Where(qtv => qtv.MaQTV == bl.MaQTV).Select(qtv => qtv.TenDangNhap).FirstOrDefault();
                    avNgBl = db.QuanTriViens.Where(qtv => qtv.MaQTV == bl.MaQTV).Select(qtv => qtv.AnhDaiDien).FirstOrDefault();
                    isAd = true;
                }
                return new BaiVietView
                {
                    // Gán các giá trị cần thiết từ BinhLuan
                    MaBL = bl.MaBL,
                    NDBL = bl.NoiDung,
                    NgayGui = bl.NgayGui ?? DateTime.Now,
                    MaNguoiGui = maNgGui,
                    TenNguoiViet = tenNgGui,
                    avatarNguoiBL = avNgBl,
                    ReplyToContent = db.BinhLuans.Where(r => r.MaBL == bl.IDCha).Select(r => r.NoiDung).FirstOrDefault(),
                    MaBV = bl.MaBV,
                    IDCha = bl.IDCha,
                    IsAdmin = isAd,
                };
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
            var decodedCodeContent = codeContent ?? string.Empty; // Ensure not null
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
            var tb = db.ThongBaos.Where(t => t.LoaiTB == "Thông báo hệ thống")
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
            var listloaicd = LayThongTinCD().GroupBy(cd => cd.MaLoai)
                            .Select(group => new
                            {
                                MaLoai = group.Key,
                                SoBaiTrongLoaiCD = group.Sum(cd => cd.SoBai)
                            })
                            .OrderByDescending(t => t.SoBaiTrongLoaiCD).Take(2).ToList();
            var lstloaicdtop2 = LayThongTinCD().Where(l => listloaicd.Any(top2 => top2.MaLoai == l.MaLoai)).ToList();

            return PartialView(lstloaicdtop2);
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
        public ActionResult ChuDe(int? page, string id)
        {
            var dscd = LayThongTinCD().Where(cd => cd.MaLoai == id).OrderBy(cd => cd.TenCD).ToList();
            if (!dscd.Any()) 
            {
                ViewBag.Message = "Chưa có chủ đề nào cho loại chủ đề này";
            }
            var ttcd = db.LoaiCDs.Where(l => l.MaLoai == id).FirstOrDefault();
            ViewBag.MaLoai = ttcd.MaLoai;
            ViewBag.TenLoai = ttcd.TenLoai;
            int iSize = 14;
            int iPageNumber = (page ?? 1);
            return View(dscd.ToPagedList(iPageNumber, iSize));
        }
        [HttpGet]
        public ActionResult BaiVietTheoCD(int? page, string id, string tenloai)
        {
            var dsbv = LayTatCaBaiViet().Where(bv => bv.MaCD == id && bv.TrangThaiBV.Contains("Đã duyệt"))
            .OrderByDescending(bv => bv.NgayDang)
            .ToList();
            if (!dsbv.Any())
            {                
                ViewBag.Message = "Chưa có bài viết nào cho chủ đề này";
            }
            var cd = db.ChuDes.FirstOrDefault(c => c.MaCD == id);
            ViewBag.MaCD = cd.MaCD;
            ViewBag.TenCD = cd.TenCD;
            ViewBag.TenLoai = tenloai;
            ViewBag.MaLoai = cd.MaLoai;
            int iSize = 14;
            int iPageNumber = (page ?? 1);
            return View(dsbv.ToPagedList(iPageNumber, iSize));
        }

        public ActionResult Loc(int? page, string sortOrder, string tenloai, string id, bool isAllPosts = false)
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
                    baiVietViewList = baiVietViewList.Where(b => b.TrangThaiBV=="Đã duyệt").OrderByDescending(b => b.NgayDang).ToList();
                    break;
                case "oldest":
                    baiVietViewList = baiVietViewList.Where(b => b.TrangThaiBV == "Đã duyệt").OrderBy(b => b.NgayDang).ToList();
                    break;
                case "az":
                    baiVietViewList = baiVietViewList.Where(b => b.TrangThaiBV == "Đã duyệt").OrderBy(b => b.TieuDe).ToList();
                    break;
                case "za":
                    baiVietViewList = baiVietViewList.Where(b => b.TrangThaiBV == "Đã duyệt").OrderByDescending(b => b.TieuDe).ToList();
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
            int iSize = 8;
            int iPageNumber = (page ?? 1);
            return View(isAllPosts ? "BaiVietMoi" : "BaiVietTheoCD", baiVietViewList.ToPagedList(iPageNumber, iSize));
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

                    post.MaBV = newMaBV; 
                    post.NgayDang = DateTime.Now; 
                    if( userId != null)
                    { 
                        post.TrangThai = "Chờ duyệt";
                        post.MaTV = userId;
                    }
                    else
                    {
                        var adId = Session["AdminId"] as string;
                        post.TrangThai = "Đã duyệt";
                        post.MaQTV = adId;
                    } 
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
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/Upload_images/"), fileName);
                file.SaveAs(path);
                return Json(new { location = Url.Content("~/Upload_images/" + fileName) });
            }
            return Json(new { error = "File upload failed." });
        }
        public ActionResult NDBaiViet(string id)
        {
            var nd = db.BaiViets.FirstOrDefault(ndct => ndct.MaBV == id);
            ViewBag.PostTitle = nd.TieuDeBV;
            ViewBag.PostURL = Request.Url.AbsoluteUri;
            var (noiDungVanBan, codeContent) = XuLyNoiDungXML(nd.NoiDung);

            // Lưu nội dung vào ViewBag
            ViewBag.NoiDungVanBan = noiDungVanBan;
            ViewBag.CodeContent = codeContent;
            var tvViet = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == nd.MaTV);
            if (tvViet != null)
            {
                ViewBag.NguoiVietBai = tvViet;
            }
            else
            {
                var qtvViet = db.QuanTriViens.FirstOrDefault(tv => tv.MaQTV == nd.MaQTV);
                ViewBag.NguoiVietBai = qtvViet;
            }
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
        public ActionResult ThongBao(int? page)
        {
            string Id;
            List <ThongBao> dstb;
            Id = Session["UserId"] as string;
            if (string.IsNullOrEmpty(Id))
            {
                Id = Session["AdminId"] as string;
                dstb = db.ThongBaos.Where(n => n.MaQTV == Id || n.LoaiTB.Contains("Tố cáo")).OrderByDescending(n => n.NgayTB).ToList();
            }
            else
            {
                dstb = db.ThongBaos.Where(n => n.MaTV == Id).OrderByDescending(n => n.NgayTB).ToList();
            }
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
            int iSize = 10;
            int iPageNumber = (page ?? 1);
            if (!dstb.Any())
                ViewBag.Message = "Không có thông báo nào gần đây";
            return View(dstb.ToPagedList(iPageNumber, iSize));
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
                if (tb.LoaiTB == "Từ chối bài viết")
                {
                    return RedirectToAction("ChinhSuaBV", new { id = tb.MaDoiTuong });
                }
                else if(tb.LoaiTB == "Xóa bài viết") 
                {
                    return RedirectToAction("XemLai", new { id = tb.MaDoiTuong });
                }
                    
                return RedirectToAction("NDBaiViet", new {id = tb.MaDoiTuong});
            }
            if(tb.LoaiDoiTuong == "BinhLuan")
            {
                if(tb.LoaiTB == "Xóa bình luận") 
                {
                    return RedirectToAction("XemLai", new { id = tb.MaDoiTuong });
                }
                else 
                {
                    var baiViet = db.BinhLuans.FirstOrDefault(bv => bv.MaBL == tb.MaDoiTuong);
                    TempData["BinhLuanId"] = tb.MaDoiTuong;
                    return RedirectToAction("NDBaiViet", new { id = baiViet.MaBV });
                }
            }
            return RedirectToAction("ThongBao");
        }
        public ActionResult BaiVietMoi(int? page)
        {
            var dsbv = LayTatCaBaiViet().Where(bv => bv.TrangThaiBV.Contains("Đã duyệt")).OrderByDescending(n => n.NgayDang).ToList();
            int iSize = 8;
            int iPageNumber = (page ?? 1);
            return View(dsbv.ToPagedList(iPageNumber, iSize));
        }
        public ActionResult PartialBinhLuan(string id)
        {
            var userId = Session["UserId"] as string;
            var adId = Session["AdminId"] as string;
            ViewBag.MaBV = id;
            var tk = new ThanhVien_QuanTriVien();
            if (userId == null && adId == null)
            {
                ViewBag.User = null;
                return PartialView();
            }
            else if (userId != null)
            {
                tk.ThanhVien = db.ThanhViens.FirstOrDefault(tv => tv.MaTV == userId);
                return PartialView(tk);
            }
            else
            {
                tk.QuanTriVien = db.QuanTriViens.FirstOrDefault(tv => tv.MaQTV == adId);
            }
            return PartialView(tk);
        }
        public ActionResult PartialDSBL(int? page, string id)
        {
            var dsbl = LayDanhSachBinhLuan(id).OrderByDescending(bl => bl.NgayGui).ToList();
            ViewData["MaBV"] = id;
            int iSize = 6;
            int iPageNumber = (page ?? 1);
            return PartialView(dsbl.ToPagedList(iPageNumber, iSize));
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

                    bl.MaBL = newMaBL; 
                    bl.NgayGui = DateTime.Now; 
                    bl.TrangThai = "Hiển thị"; 
                    if (userId != null)
                    {
                        bl.MaTV = userId;
                    }
                    else
                    {
                        var adId = Session["AdminId"] as string;
                        bl.MaQTV = adId;
                    }
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

                    var maNgVietBai = db.BaiViets.Where(bv => bv.MaBV == bl.MaBV).Select(bv => bv.MaTV).FirstOrDefault();
                    if (maNgVietBai != null)
                    {
                        GuiThongBao("Bình luận", maNgVietBai, bl.MaBL, "BinhLuan");
                    }
                    else
                    {
                        maNgVietBai = db.BaiViets.Where(bv => bv.MaBV == bl.MaBV).Select(bv => bv.MaQTV).FirstOrDefault();
                        GuiThongBao("Bình luận", maNgVietBai, bl.MaBL, "BinhLuan");
                    }
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
        public void GuiThongBao(string loaitb,string maNguoiNhan, string maDoiTuong, string loaidt)
        {
            var lastTB = db.ThongBaos.OrderByDescending(c => c.MaTB).FirstOrDefault();
            string newMaTB = "TB" + (Convert.ToInt32(lastTB.MaTB.Substring(2)) + 1).ToString("D3");
            // Tạo thông báo
            ThongBao thongBao = new ThongBao
            {
                MaTB = newMaTB,
                NgayTB = DateTime.Now,
                LoaiTB = loaitb,
                MaDoiTuong = maDoiTuong,
                LoaiDoiTuong = loaidt,
                TrangThai = false
            };
            if (db.ThanhViens.Find(maNguoiNhan) != null)
            {
                thongBao.MaTV = maNguoiNhan;
            }
            else
            {
                thongBao.MaQTV = maNguoiNhan;
            }
            if ( loaidt == "BinhLuan")
            {
                var maBaiViet = db.BinhLuans.Where(bl => bl.MaBL == maDoiTuong).Select(bl => bl.MaBV).FirstOrDefault();
                var tieuDeBV = db.BaiViets.Where(bv => bv.MaBV == maBaiViet).Select(bv => bv.TieuDeBV).FirstOrDefault();
                thongBao.NoiDung = $"<NoiDung>Bài viết '{tieuDeBV}' của bạn vừa có bình luận mới.</NoiDung>";
                db.SaveChanges();
                var idCha = db.BinhLuans.Where(bl => bl.MaBL == maDoiTuong).Select(bl => bl.IDCha).FirstOrDefault();
                if (!string.IsNullOrEmpty(idCha))
                {
                    ThongBao replyThongBao = new ThongBao
                    {
                        MaTB = "TB" + (Convert.ToInt32(lastTB.MaTB.Substring(2)) + 2).ToString("D3"), // Tạo mã TB mới cho người reply
                        NgayTB = DateTime.Now,
                        LoaiTB = loaitb,
                        MaDoiTuong = maDoiTuong,
                        LoaiDoiTuong = loaidt,
                        NoiDung = $"<NoiDung>Bình luận của bạn ở bài viết '{tieuDeBV}' đã có phản hồi mới.</NoiDung>",
                        TrangThai = false
                    };
                    var replyTV = db.BinhLuans.Where(bl => bl.MaBL == idCha).Select(bl => bl.MaTV).FirstOrDefault();
                    if (replyTV != null)
                    {
                        replyThongBao.MaTV = replyTV;
                    }
                    else
                    {
                        replyTV = db.BinhLuans.Where(bl => bl.MaBL == idCha).Select(bl => bl.MaQTV).FirstOrDefault();
                        replyThongBao.MaQTV = replyTV;
                    }
                    db.ThongBaos.Add(replyThongBao);
                }
            }
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
        public ActionResult ChinhSuaBV(string id)
        {
            var bv = db.BaiViets.Where(b => b.MaBV == id).SingleOrDefault();
            if(bv.TrangThai == "Từ chối") 
            {   
                var (noiDungVanBan, codeContent) = XuLyNoiDungXML(bv.NoiDung);
                ViewBag.NDVB = noiDungVanBan;
                ViewBag.Code = codeContent;
                return View(bv); 
            }
            else
            { return RedirectToAction("ThongBao"); }
            
        }
        [ValidateInput(false)]
        public ActionResult CapNhapBV(BaiViet post)
        {
            var bv = db.BaiViets.Find(post.MaBV);
            if (!string.IsNullOrEmpty(post.NoiDung))
            {
                var nd = XuLyNoiDung(post.NoiDung, Request.Unvalidated.Form["CodeContent"]);
                bv.NoiDung = nd;
                bv.TrangThai = "Chờ duyệt";
                db.SaveChanges();
            }
            else 
            {
                ViewBag.Loi = "Vui lòng điền đầy đủ thông tin!";
                return View();
            }
                
            return RedirectToAction("ThongBao");
        }
        public ActionResult LuuLyDoToCao(string IdDoiTuong, string LyDoToCao)
        {
            var lastTB = db.ThongBaos.OrderByDescending(c => c.MaTB).FirstOrDefault();
            string newMaTB = "TB" + (Convert.ToInt32(lastTB.MaTB.Substring(2)) + 1).ToString("D3");
            // Tạo thông báo
            ThongBao thongBao = new ThongBao
            {
                MaTB = newMaTB,
                NgayTB = DateTime.Now,
                LoaiTB = "Tố cáo",
                MaDoiTuong = IdDoiTuong,
                TrangThai = false
            };
            var dt = db.BaiViets.Where(b => b.MaBV == IdDoiTuong).Select(b => b.TieuDeBV).FirstOrDefault();
            string mabv;
            if(dt == null)
            {
                mabv = db.BinhLuans.Where(bl => bl.MaBL == IdDoiTuong).Select(bl => bl.MaBV).FirstOrDefault();
                dt = db.BinhLuans.Where(bl => bl.MaBL == IdDoiTuong).Select(bl => bl.BaiViet.TieuDeBV).FirstOrDefault();
                thongBao.NoiDung = $"<NoiDung>Bài viết '{dt}'có bình luận bị tố cáo vì lý do '{LyDoToCao}'</NoiDung>";
                thongBao.LoaiDoiTuong = "BinhLuan";
            }    
            else
            {
                mabv = IdDoiTuong;
                thongBao.NoiDung = $"<NoiDung>Bài viết '{dt}' đã bị tố cáo vì lý do '{LyDoToCao}'</NoiDung>";
                thongBao.LoaiDoiTuong = "BaiViet";
            }
            db.ThongBaos.Add(thongBao);
            db.SaveChanges();
            return RedirectToAction("NDBaiViet", new {id = mabv} );
        }
        public ActionResult XemLai(string id)
        {
            var bv = db.BaiViets.Where(b => b.MaBV == id).SingleOrDefault();
            if (bv == null)
            {
                var bl = db.BinhLuans.Where(b => b.MaBL == id).SingleOrDefault();
                var baiVietLienQuan = db.BaiViets.SingleOrDefault(b => b.MaBV == bl.MaBV);
                var (noiDungVanBan, codeContent) = XuLyNoiDungXML(bl.NoiDung);
                ViewBag.NoiDung = noiDungVanBan;
                ViewBag.Code = codeContent;
                // Khởi tạo mô hình BaiViet_BinhLuan
                var model = new BaiViet_BinhLuan
                {
                    BinhLuan = bl,
                    BaiViet = baiVietLienQuan // Gán bài viết liên quan
                };
                return View(model);
            }
            else
            {
                var model = new BaiViet_BinhLuan
                {
                    BaiViet = bv
                };

                var (noiDungVanBan, codeContent) = XuLyNoiDungXML(bv.NoiDung);
                ViewBag.NoiDung = noiDungVanBan;
                ViewBag.Code = codeContent;
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult XoaBai(string maBV)
        {
            var baiViet = db.BaiViets.SingleOrDefault(b => b.MaBV.Contains(maBV));
            if (baiViet != null)
            {
                db.BaiViets.Remove(baiViet);

                var tb = db.ThongBaos.Where(t => t.MaDoiTuong.Contains(maBV)).ToList();
                if (tb.Any())
                {
                    db.ThongBaos.RemoveRange(tb); 
                }

                db.SaveChanges();
            }

            return RedirectToAction("ThongBao");
        }
        [HttpPost]
        public ActionResult XoaThongBao(string MaThongBao)
        {
            var tb = db.ThongBaos.Where(t => t.MaTB.Contains(MaThongBao)).SingleOrDefault();
            db.ThongBaos.Remove(tb);
            db.SaveChanges();
            return RedirectToAction("ThongBao");
        }
    }
}