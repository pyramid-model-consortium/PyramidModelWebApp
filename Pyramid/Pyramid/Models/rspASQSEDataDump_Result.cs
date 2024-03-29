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
    
    public partial class rspASQSEDataDump_Result
    {
        public int ASQSEPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime FormDate { get; set; }
        public bool HasDemographicInfoSheet { get; set; }
        public bool HasPhysicianInfoLetter { get; set; }
        public int TotalScore { get; set; }
        public int ChildFK { get; set; }
        public int IntervalCodeFK { get; set; }
        public int ProgramFK { get; set; }
        public int Version { get; set; }
        public int CutoffScore { get; set; }
        public int MaxScore { get; set; }
        public int MonitoringScoreStart { get; set; }
        public int MonitoringScoreEnd { get; set; }
        public int ProgramPK { get; set; }
        public string ProgramName { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
        public string IntervalDescription { get; set; }
        public Nullable<int> IntervalMonth { get; set; }
        public int ChildProgramPK { get; set; }
        public string ChildIDNumber { get; set; }
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
    }
}
