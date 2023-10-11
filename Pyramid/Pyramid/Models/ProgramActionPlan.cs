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
    
    public partial class ProgramActionPlan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProgramActionPlan()
        {
            this.ProgramActionPlanActionStep = new HashSet<ProgramActionPlanActionStep>();
            this.ProgramActionPlanGroundRule = new HashSet<ProgramActionPlanGroundRule>();
            this.ProgramActionPlanMeeting = new HashSet<ProgramActionPlanMeeting>();
        }
    
        public int ProgramActionPlanPK { get; set; }
        public System.DateTime ActionPlanEndDate { get; set; }
        public System.DateTime ActionPlanStartDate { get; set; }
        public string AdditionalNotes { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool IsLeadershipCoachInvolved { get; set; }
        public string MissionStatement { get; set; }
        public string LeadershipCoachUsername { get; set; }
        public int ProgramFK { get; set; }
        public Nullable<bool> IsFullyReviewed { get; set; }
        public bool IsPrefilled { get; set; }
        public bool ReviewedActionSteps { get; set; }
        public bool ReviewedBasicInfo { get; set; }
        public bool ReviewedGroundRules { get; set; }
    
        public virtual Program Program { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramActionPlanActionStep> ProgramActionPlanActionStep { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramActionPlanGroundRule> ProgramActionPlanGroundRule { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramActionPlanMeeting> ProgramActionPlanMeeting { get; set; }
    }
}
