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
    
    public partial class rspCWLTDataDump_BOQParticipants_Result
    {
        public int BOQCWLTParticipantPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int CWLTMemberPK { get; set; }
        public string ParticipantIDNumber { get; set; }
        public string ParticipantFirstName { get; set; }
        public string ParticipantLastName { get; set; }
        public int BenchmarkOfQualityCWLTPK { get; set; }
        public System.DateTime BOQFormDate { get; set; }
        public int HubPK { get; set; }
        public string HubName { get; set; }
        public int StatePK { get; set; }
        public string StateName { get; set; }
    }
}
