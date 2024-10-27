//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DienDanThaoLuan.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class QuanTriVien
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuanTriVien()
        {
            this.BaiViets = new HashSet<BaiViet>();
            this.BinhLuans = new HashSet<BinhLuan>();
            this.ThongBaos = new HashSet<ThongBao>();
        }
    
        public string MaQTV { get; set; }
        public string HoTen { get; set; }
        public string AnhDaiDien { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string GioiTinh { get; set; }
        public string AnhBia { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BaiViet> BaiViets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BinhLuan> BinhLuans { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ThongBao> ThongBaos { get; set; }
    }
}
