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
    
    public partial class rspMasterCadreDataDump_TrainingDebriefs_Result
    {
        public int MasterCadreTrainingDebriefPK { get; set; }
        public string AspireEventNum { get; set; }
        public string AssistanceNeeded { get; set; }
        public string CoachingInterest { get; set; }
        public string CourseIDNum { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public System.DateTime DateCompleted { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string MeetingLocation { get; set; }
        public int NumAttendees { get; set; }
        public int NumEvalsReceived { get; set; }
        public string Reflection { get; set; }
        public Nullable<bool> WasUploadedToAspire { get; set; }
        public string MasterCadreMemberUsername { get; set; }
        public int CodeMeetingFormatPK { get; set; }
        public string MeetingFormatText { get; set; }
        public int CodeMasterCadreActivityPK { get; set; }
        public string ActivityText { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}
