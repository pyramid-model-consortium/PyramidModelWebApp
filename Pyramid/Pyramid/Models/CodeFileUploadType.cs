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
    
    public partial class CodeFileUploadType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CodeFileUploadType()
        {
            this.UserFileUpload = new HashSet<UserFileUpload>();
        }
    
        public int CodeFileUploadTypePK { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public int OrderBy { get; set; }
        public string RolesAuthorizedToModify { get; set; }
        public System.DateTime StartDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserFileUpload> UserFileUpload { get; set; }
    }
}
