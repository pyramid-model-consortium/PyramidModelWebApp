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
    
    public partial class CoachingCircleLCMeetingDebriefSession
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CoachingCircleLCMeetingDebriefSession()
        {
            this.CoachingCircleLCMeetingDebriefSessionAttendee = new HashSet<CoachingCircleLCMeetingDebriefSessionAttendee>();
        }
    
        public int CoachingCircleLCMeetingDebriefSessionPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime SessionEndDateTime { get; set; }
        public System.DateTime SessionStartDateTime { get; set; }
        public string SessionSummary { get; set; }
        public int CoachingCircleLCMeetingDebriefFK { get; set; }
    
        public virtual CoachingCircleLCMeetingDebrief CoachingCircleLCMeetingDebrief { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CoachingCircleLCMeetingDebriefSessionAttendee> CoachingCircleLCMeetingDebriefSessionAttendee { get; set; }
    }
}
