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
    
    public partial class rspChildDataDump_StatusHistory_Result
    {
        public int ChildStatusPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime StatusDate { get; set; }
        public int ChildStatusCodeFK { get; set; }
        public string StatusText { get; set; }
        public int ChildFK { get; set; }
        public int ChildProgramPK { get; set; }
        public string ChildIDNumber { get; set; }
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public int ProgramPK { get; set; }
        public string ProgramName { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}
