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
    
    public partial class MasterCadreTrainingTrackerItemChanged
    {
        public int MasterCadreTrainingTrackerItemChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
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
        public int MasterCadreActivityCodeFK { get; set; }
        public int MasterCadreFundingSourceCodeFK { get; set; }
        public string MasterCadreMemberUsername { get; set; }
        public int MeetingFormatCodeFK { get; set; }
        public int StateFK { get; set; }
    }
}