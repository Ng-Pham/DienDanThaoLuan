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
    
    public partial class BaiViet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BaiViet()
        {
            this.BinhLuans = new HashSet<BinhLuan>();
        }
    
        public string MaBV { get; set; }
        public string TieuDeBV { get; set; }
        public string NoiDung { get; set; }
        public Nullable<System.DateTime> NgayDang { get; set; }
        public string TrangThai { get; set; }
        public string MaCD { get; set; }
        public string MaTV { get; set; }
        public string MaQTV { get; set; }
    
        public virtual ChuDe ChuDe { get; set; }
        public virtual QuanTriVien QuanTriVien { get; set; }
        public virtual ThanhVien ThanhVien { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BinhLuan> BinhLuans { get; set; }
    }
}
