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
    
    public partial class CWLTActionPlanMeetingChanged
    {
        public int CWLTActionPlanMeetingChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int CWLTActionPlanMeetingPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool LeadershipCoachAttendance { get; set; }
        public System.DateTime MeetingDate { get; set; }
        public string MeetingNotes { get; set; }
        public int CWLTActionPlanFK { get; set; }
    }
}
