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
    
    public partial class BOQCWLTParticipant
    {
        public int BOQCWLTParticipantPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int BenchmarksOfQualityCWLTFK { get; set; }
        public int CWLTMemberFK { get; set; }
    
        public virtual BenchmarkOfQualityCWLT BenchmarkOfQualityCWLT { get; set; }
        public virtual CWLTMember CWLTMember { get; set; }
    }
}
