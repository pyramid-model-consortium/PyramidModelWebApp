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
    
    public partial class TPITOSRedFlagsChanged
    {
        public int TPITOSRedFlagsChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int TPITOSRedFlagsPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int TPITOSFK { get; set; }
        public int RedFlagCodeFK { get; set; }
    }
}
