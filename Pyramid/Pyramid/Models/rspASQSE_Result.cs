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
    
    public partial class rspASQSE_Result
    {
        public int ASQSEPK { get; set; }
        public System.DateTime ASQSEDate { get; set; }
        public bool HasDemographicInfoSheet { get; set; }
        public bool HasPhysicianInfoLetter { get; set; }
        public int TotalScore { get; set; }
        public int ASQSEVersion { get; set; }
        public int CutoffScore { get; set; }
        public int MaxScore { get; set; }
        public int MonitoringScoreStart { get; set; }
        public int MonitoringScoreEnd { get; set; }
        public Nullable<int> IntervalMonth { get; set; }
        public string IntervalDescription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string ProgramSpecificID { get; set; }
        public string ProgramName { get; set; }
        public string ClassroomName { get; set; }
    }
}
