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
    
    public partial class CoachingCircleLCMeetingScheduleChanged
    {
        public int CoachingCircleLCMeetingScheduleChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int CoachingCircleLCMeetingSchedulePK { get; set; }
        public string CoachingCircleName { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool MeetingInJan { get; set; }
        public bool MeetingInFeb { get; set; }
        public bool MeetingInMar { get; set; }
        public bool MeetingInApr { get; set; }
        public bool MeetingInMay { get; set; }
        public bool MeetingInJun { get; set; }
        public bool MeetingInJul { get; set; }
        public bool MeetingInAug { get; set; }
        public bool MeetingInSep { get; set; }
        public bool MeetingInOct { get; set; }
        public bool MeetingInNov { get; set; }
        public bool MeetingInDec { get; set; }
        public int MeetingYear { get; set; }
        public string TargetAudience { get; set; }
        public int TotalMeetings { get; set; }
        public string LeadershipCoachUsername { get; set; }
        public int StateFK { get; set; }
    }
}
