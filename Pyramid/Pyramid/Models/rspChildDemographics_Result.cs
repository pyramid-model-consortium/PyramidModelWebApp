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
    
    public partial class rspChildDemographics_Result
    {
        public int ChildProgramPK { get; set; }
        public string ProgramSpecificID { get; set; }
        public int ChildPK { get; set; }
        public string ChildName { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string Ethnicity { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public System.DateTime EnrollmentDate { get; set; }
        public string IEPStatus { get; set; }
        public string DLLStatus { get; set; }
        public Nullable<System.DateTime> DischargeDate { get; set; }
        public string DischargeReason { get; set; }
        public string ProgramName { get; set; }
    }
}
