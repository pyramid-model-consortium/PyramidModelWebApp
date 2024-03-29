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
    
    public partial class HubLCMeetingDebrief
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HubLCMeetingDebrief()
        {
            this.HubLCMeetingDebriefSession = new HashSet<HubLCMeetingDebriefSession>();
        }
    
        public int HubLCMeetingDebriefPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int DebriefYear { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string LeadOrganization { get; set; }
        public string LocationAddress { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string PrimaryContactPhone { get; set; }
        public string LeadershipCoachUsername { get; set; }
        public int HubFK { get; set; }
    
        public virtual Hub Hub { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HubLCMeetingDebriefSession> HubLCMeetingDebriefSession { get; set; }
    }
}
