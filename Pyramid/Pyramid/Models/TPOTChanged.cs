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
    
    public partial class TPOTChanged
    {
        public int TPOTChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int TPOTPK { get; set; }
        public Nullable<int> AdditionalStrategiesNumUsed { get; set; }
        public Nullable<int> ChallengingBehaviorsNumObserved { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool IsComplete { get; set; }
        public Nullable<int> Item1NumNo { get; set; }
        public Nullable<int> Item1NumYes { get; set; }
        public Nullable<int> Item2NumNo { get; set; }
        public Nullable<int> Item2NumYes { get; set; }
        public Nullable<int> Item3NumNo { get; set; }
        public Nullable<int> Item3NumYes { get; set; }
        public Nullable<int> Item4NumNo { get; set; }
        public Nullable<int> Item4NumYes { get; set; }
        public Nullable<int> Item5NumNo { get; set; }
        public Nullable<int> Item5NumYes { get; set; }
        public Nullable<int> Item6NumNo { get; set; }
        public Nullable<int> Item6NumYes { get; set; }
        public Nullable<int> Item7NumNo { get; set; }
        public Nullable<int> Item7NumYes { get; set; }
        public Nullable<int> Item8NumNo { get; set; }
        public Nullable<int> Item8NumYes { get; set; }
        public Nullable<int> Item9NumNo { get; set; }
        public Nullable<int> Item9NumYes { get; set; }
        public Nullable<int> Item10NumNo { get; set; }
        public Nullable<int> Item10NumYes { get; set; }
        public Nullable<int> Item11NumNo { get; set; }
        public Nullable<int> Item11NumYes { get; set; }
        public Nullable<int> Item12NumNo { get; set; }
        public Nullable<int> Item12NumYes { get; set; }
        public Nullable<int> Item13NumNo { get; set; }
        public Nullable<int> Item13NumYes { get; set; }
        public Nullable<int> Item14NumNo { get; set; }
        public Nullable<int> Item14NumYes { get; set; }
        public string Notes { get; set; }
        public int NumAdultsBegin { get; set; }
        public int NumAdultsEnd { get; set; }
        public int NumAdultsEntered { get; set; }
        public int NumKidsBegin { get; set; }
        public int NumKidsEnd { get; set; }
        public System.DateTime ObservationEndDateTime { get; set; }
        public System.DateTime ObservationStartDateTime { get; set; }
        public Nullable<int> RedFlagsNumNo { get; set; }
        public Nullable<int> RedFlagsNumYes { get; set; }
        public int ClassroomFK { get; set; }
        public Nullable<int> EssentialStrategiesUsedCodeFK { get; set; }
        public int ObserverFK { get; set; }
    }
}
