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
    
    public partial class rspClassroom_EmployeeClassroomAssignments_Result
    {
        public int EmployeeClassroomPK { get; set; }
        public System.DateTime AssignDate { get; set; }
        public Nullable<System.DateTime> LeaveDate { get; set; }
        public string LeaveReasonSpecify { get; set; }
        public int ClassroomPK { get; set; }
        public string ClassroomName { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public string ClassroomJob { get; set; }
        public string LeaveReason { get; set; }
    }
}
