using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DienDanThaoLuan.Models
{
    public class BaiVietView
    {
        public string MaLoai { get; set; }
        public string MaCD { get; set; }
        public string MaBV { get; set; }
        public string TenTV { get; set; }
        public string TenLoai { get; set; }
        public string TenCD { get; set; }
        public string TieuDe { get; set; }
        public string ND { get; set; }
        public DateTime NgayDang { get; set;}
        public int SoBL { get; set; }
        public string MaBL { get; set; }
        public string NDBL { get; set;}
        public DateTime NgayGui { get; set;}
        public string MaTVGui { get; set; }
        public string avTvBl { get; set; }
        public string CodeContent { get; set; }
        public string ReplyToContent { get; set; }
        public string IDCha { get; set; }
        public string TrangThaiBV { get; set; }
    }
}