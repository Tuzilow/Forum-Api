//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ForumApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RoleTb
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RoleTb()
        {
            this.ArticleTb = new HashSet<ArticleTb>();
            this.CommentTb = new HashSet<CommentTb>();
            this.LikeTb = new HashSet<LikeTb>();
        }
    
        public int roleId { get; set; }
        public string account { get; set; }
        public string nickName { get; set; }
        public string pwd { get; set; }
        public int powerNum { get; set; }
        public bool isDel { get; set; }
        public string avatarUrl { get; set; }
        public string colm2 { get; set; }
        public string openid { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ArticleTb> ArticleTb { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommentTb> CommentTb { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LikeTb> LikeTb { get; set; }
    }
}
