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
    
    public partial class ChildProgramChanged
    {
        public int ChildProgramChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int ChildProgramPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public Nullable<System.DateTime> DischargeDate { get; set; }
        public string DischargeReasonSpecify { get; set; }
        public System.DateTime EnrollmentDate { get; set; }
        public bool HasIEP { get; set; }
        public bool HasParentPermission { get; set; }
        public bool IsDLL { get; set; }
        public string ParentPermissionDocumentFileName { get; set; }
        public string ParentPermissionDocumentFilePath { get; set; }
        public string ProgramSpecificID { get; set; }
        public int ChildFK { get; set; }
        public Nullable<int> DischargeCodeFK { get; set; }
        public int ProgramFK { get; set; }
    }
}
