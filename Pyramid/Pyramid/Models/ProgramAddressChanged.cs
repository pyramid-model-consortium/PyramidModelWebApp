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
    
    public partial class ProgramAddressChanged
    {
        public int ProgramAddressChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int ProgramAddressPK { get; set; }
        public string City { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Creator { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string Editor { get; set; }
        public bool IsMailingAddress { get; set; }
        public string LicenseNumber { get; set; }
        public string Notes { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        public string ZIPCode { get; set; }
        public int ProgramFK { get; set; }
    }
}
