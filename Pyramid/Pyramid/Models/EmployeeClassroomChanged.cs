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
    
    public partial class EmployeeClassroomChanged
    {
        public int EmployeeClassroomChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int EmployeeClassroomPK { get; set; }
        public System.DateTime AssignDate { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public Nullable<System.DateTime> LeaveDate { get; set; }
        public string LeaveReasonSpecify { get; set; }
        public int ClassroomFK { get; set; }
        public Nullable<int> JobTypeCodeFK { get; set; }
        public Nullable<int> LeaveReasonCodeFK { get; set; }
        public int ProgramEmployeeFK { get; set; }
    }
}
