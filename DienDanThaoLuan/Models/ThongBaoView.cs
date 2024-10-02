using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DienDanThaoLuan.Models
{
    public class ThongBaoView
    {
        public string NoiDung { get; set; }
        public DateTime? NgayTB { get; set; }
        public string MaTB { get; set; }
        public string LoaiTB { get; set; }
    }
}