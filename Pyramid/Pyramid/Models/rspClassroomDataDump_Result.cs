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
    
    public partial class rspClassroomDataDump_Result
    {
        public int ClassroomPK { get; set; }
        public string ClassroomID { get; set; }
        public string ClassroomName { get; set; }
        public bool BeingServedSubstitute { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool IsInfantToddler { get; set; }
        public bool IsPreschool { get; set; }
        public string ClassroomLocation { get; set; }
        public int ProgramPK { get; set; }
        public string ProgramName { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}
