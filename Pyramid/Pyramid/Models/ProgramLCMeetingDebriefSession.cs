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
    
    public partial class ProgramLCMeetingDebriefSession
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProgramLCMeetingDebriefSession()
        {
            this.ProgramLCMeetingDebriefSessionAttendee = new HashSet<ProgramLCMeetingDebriefSessionAttendee>();
        }
    
        public int ProgramLCMeetingDebriefSessionPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime NextSessionEndDateTime { get; set; }
        public System.DateTime NextSessionStartDateTime { get; set; }
        public bool ReviewedActionPlan { get; set; }
        public bool ReviewedBOQ { get; set; }
        public bool ReviewedOtherItem { get; set; }
        public string ReviewedOtherItemSpecify { get; set; }
        public bool ReviewedTPITOS { get; set; }
        public bool ReviewedTPOT { get; set; }
        public System.DateTime SessionEndDateTime { get; set; }
        public string SessionNextSteps { get; set; }
        public System.DateTime SessionStartDateTime { get; set; }
        public string SessionSummary { get; set; }
        public int ProgramLCMeetingDebriefFK { get; set; }
    
        public virtual ProgramLCMeetingDebrief ProgramLCMeetingDebrief { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramLCMeetingDebriefSessionAttendee> ProgramLCMeetingDebriefSessionAttendee { get; set; }
    }
}
