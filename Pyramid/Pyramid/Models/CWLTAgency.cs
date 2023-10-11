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
    
    public partial class CWLTAgency
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CWLTAgency()
        {
            this.CWLTMemberAgencyAssignment = new HashSet<CWLTMemberAgencyAssignment>();
        }
    
        public int CWLTAgencyPK { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressStreet { get; set; }
        public string AddressZIPCode { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public int CWLTAgencyTypeFK { get; set; }
        public int HubFK { get; set; }
    
        public virtual CWLTAgencyType CWLTAgencyType { get; set; }
        public virtual Hub Hub { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CWLTMemberAgencyAssignment> CWLTMemberAgencyAssignment { get; set; }
    }
}