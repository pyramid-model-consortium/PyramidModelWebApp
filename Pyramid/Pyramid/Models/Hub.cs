//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pyramid.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Hub
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Hub()
        {
            this.Program = new HashSet<Program>();
            this.NewsEntry = new HashSet<NewsEntry>();
            this.UserFileUpload = new HashSet<UserFileUpload>();
        }
    
        public int HubPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string Name { get; set; }
        public int StateFK { get; set; }
    
        public virtual State State { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Program> Program { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NewsEntry> NewsEntry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserFileUpload> UserFileUpload { get; set; }
    }
}