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
    
    public partial class rspLeadershipCoachDataDump_ProgramDebriefSessionAttendees_Result
    {
        public int ProgramLCMeetingDebriefSessionAttendeePK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int PLTMemberPK { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ProgramLCMeetingDebriefSessionPK { get; set; }
        public System.DateTime SessionStartDateTime { get; set; }
        public int ProgramPK { get; set; }
        public string ProgramName { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}
