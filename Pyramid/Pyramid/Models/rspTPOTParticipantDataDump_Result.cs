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
    
    public partial class rspTPOTParticipantDataDump_Result
    {
        public int TPOTParticipantPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int ParticipantTypeCodeFK { get; set; }
        public int ProgramEmployeeFK { get; set; }
        public int TPOTFK { get; set; }
        public string ParticipantType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ParticipantEmployeeKey { get; set; }
        public string ParticipantID { get; set; }
        public string ProgramName { get; set; }
        public int ProgramPK { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
        public int TPOTPK { get; set; }
        public System.DateTime ObservationStartDateTime { get; set; }
    }
}
