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
    
    public partial class rspMasterCadreDataDump_ActivityTrackers_Result
    {
        public int MasterCadreTrainingTrackerItemPK { get; set; }
        public string AspireEventNum { get; set; }
        public string CourseIDNum { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public bool DidEventOccur { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool IsOpenToPublic { get; set; }
        public string MeetingLocation { get; set; }
        public decimal NumHours { get; set; }
        public decimal ParticipantFee { get; set; }
        public string TargetAudience { get; set; }
        public string MasterCadreMemberUsername { get; set; }
        public int CodeMasterCadreFundingSourcePK { get; set; }
        public string FundingSourceText { get; set; }
        public int CodeMeetingFormatPK { get; set; }
        public string MeetingFormatText { get; set; }
        public int CodeMasterCadreActivityPK { get; set; }
        public string ActivityText { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}