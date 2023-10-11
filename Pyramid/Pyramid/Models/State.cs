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
    
    public partial class State
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public State()
        {
            this.BenchmarkOfQualitySLT = new HashSet<BenchmarkOfQualitySLT>();
            this.CoachingCircleLCMeetingDebrief = new HashSet<CoachingCircleLCMeetingDebrief>();
            this.CoachingCircleLCMeetingSchedule = new HashSet<CoachingCircleLCMeetingSchedule>();
            this.CodeTrainingAccess = new HashSet<CodeTrainingAccess>();
            this.Cohort = new HashSet<Cohort>();
            this.ConfidentialityAgreement = new HashSet<ConfidentialityAgreement>();
            this.CWLTAgencyType = new HashSet<CWLTAgencyType>();
            this.FormDueDate = new HashSet<FormDueDate>();
            this.Hub = new HashSet<Hub>();
            this.NewsEntry = new HashSet<NewsEntry>();
            this.Program = new HashSet<Program>();
            this.SLTActionPlan = new HashSet<SLTActionPlan>();
            this.SLTAgency = new HashSet<SLTAgency>();
            this.SLTMember = new HashSet<SLTMember>();
            this.SLTWorkGroup = new HashSet<SLTWorkGroup>();
            this.StateSettings = new HashSet<StateSettings>();
            this.UserFileUpload = new HashSet<UserFileUpload>();
            this.MasterCadreTrainingDebrief = new HashSet<MasterCadreTrainingDebrief>();
            this.MasterCadreTrainingTrackerItem = new HashSet<MasterCadreTrainingTrackerItem>();
        }
    
        public int StatePK { get; set; }
        public string Abbreviation { get; set; }
        public string Catchphrase { get; set; }
        public Nullable<System.DateTime> ConfidentialityChangeDate { get; set; }
        public bool ConfidentialityEnabled { get; set; }
        public string ConfidentialityFilename { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Disclaimer { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int HomePageLogoOption { get; set; }
        public bool LockEndedPrograms { get; set; }
        public string LogoFilename { get; set; }
        public int MaxNumberOfPrograms { get; set; }
        public string Name { get; set; }
        public bool ShareDataNationally { get; set; }
        public string ThumbnailLogoFilename { get; set; }
        public bool UtilizingPIDS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BenchmarkOfQualitySLT> BenchmarkOfQualitySLT { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CoachingCircleLCMeetingDebrief> CoachingCircleLCMeetingDebrief { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CoachingCircleLCMeetingSchedule> CoachingCircleLCMeetingSchedule { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CodeTrainingAccess> CodeTrainingAccess { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cohort> Cohort { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ConfidentialityAgreement> ConfidentialityAgreement { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CWLTAgencyType> CWLTAgencyType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FormDueDate> FormDueDate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hub> Hub { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NewsEntry> NewsEntry { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Program> Program { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SLTActionPlan> SLTActionPlan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SLTAgency> SLTAgency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SLTMember> SLTMember { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SLTWorkGroup> SLTWorkGroup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StateSettings> StateSettings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserFileUpload> UserFileUpload { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MasterCadreTrainingDebrief> MasterCadreTrainingDebrief { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MasterCadreTrainingTrackerItem> MasterCadreTrainingTrackerItem { get; set; }
    }
}
