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
    
    public partial class BehaviorIncidentChanged
    {
        public int BehaviorIncidentChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int BehaviorIncidentPK { get; set; }
        public string ActivitySpecify { get; set; }
        public string AdminFollowUpSpecify { get; set; }
        public string BehaviorDescription { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime IncidentDatetime { get; set; }
        public string Notes { get; set; }
        public string OthersInvolvedSpecify { get; set; }
        public string PossibleMotivationSpecify { get; set; }
        public string ProblemBehaviorSpecify { get; set; }
        public string StrategyResponseSpecify { get; set; }
        public int ActivityCodeFK { get; set; }
        public int AdminFollowUpCodeFK { get; set; }
        public int OthersInvolvedCodeFK { get; set; }
        public int PossibleMotivationCodeFK { get; set; }
        public int ProblemBehaviorCodeFK { get; set; }
        public int StrategyResponseCodeFK { get; set; }
        public int ChildFK { get; set; }
        public int ClassroomFK { get; set; }
    }
}
