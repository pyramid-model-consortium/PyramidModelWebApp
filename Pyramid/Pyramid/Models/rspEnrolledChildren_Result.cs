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
    
    public partial class rspEnrolledChildren_Result
    {
        public int ChildProgramPK { get; set; }
        public string ProgramSpecificID { get; set; }
        public int ChildPK { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public System.DateTime BirthDate { get; set; }
        public System.DateTime EnrollmentDate { get; set; }
        public Nullable<System.DateTime> DischargeDate { get; set; }
        public bool HasIEP { get; set; }
        public int IEPInt { get; set; }
        public bool IsDLL { get; set; }
        public int DLLInt { get; set; }
        public int RaceCodeFK { get; set; }
        public string Race { get; set; }
        public int EthnicityCodeFK { get; set; }
        public string Ethnicity { get; set; }
        public int GenderCodeFK { get; set; }
        public string Gender { get; set; }
    }
}
