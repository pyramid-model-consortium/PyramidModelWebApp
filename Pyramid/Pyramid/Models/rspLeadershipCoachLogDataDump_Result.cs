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
    
    public partial class rspLeadershipCoachLogDataDump_Result
    {
        public int LeadershipCoachLogPK { get; set; }
        public string ActNarrative { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<int> CyclePhase { get; set; }
        public Nullable<System.DateTime> DateCompleted { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string HighlightsNarrative { get; set; }
        public bool IsComplete { get; set; }
        public Nullable<bool> IsMonthly { get; set; }
        public Nullable<int> NumberOfAttemptedEngagements { get; set; }
        public Nullable<int> NumberOfEngagements { get; set; }
        public string OtherDomainTwoSpecify { get; set; }
        public string OtherEngagementSpecify { get; set; }
        public string OtherSiteResourcesSpecify { get; set; }
        public string OtherTopicsDiscussedSpecify { get; set; }
        public string OtherTrainingsCoveredSpecify { get; set; }
        public Nullable<int> TargetedTrainingHours { get; set; }
        public Nullable<int> TargetedTrainingMinutes { get; set; }
        public string ThinkNarrative { get; set; }
        public Nullable<int> TotalDurationHours { get; set; }
        public Nullable<int> TotalDurationMinutes { get; set; }
        public string GoalCompletionLikelihood { get; set; }
        public string LeadershipCoachUsername { get; set; }
        public int ProgramFK { get; set; }
        public string ProgramName { get; set; }
        public string TimelyProgressionLikelihood { get; set; }
        public string IDNumber { get; set; }
        public string Domain1 { get; set; }
        public string Domain2 { get; set; }
        public string Domain3 { get; set; }
        public string Domain4 { get; set; }
        public string Domain5 { get; set; }
        public string Domain6 { get; set; }
        public string IdentifiedProgramBarriers { get; set; }
        public string IdentifiedProgramStrengths { get; set; }
        public string SiteResources { get; set; }
        public string TrainingsCovered { get; set; }
        public string TopicsDiscussed { get; set; }
        public string Types { get; set; }
        public string Coaches { get; set; }
        public string TeamMembers { get; set; }
    }
}
