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
    
    public partial class CodeActionPlanActionStepStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CodeActionPlanActionStepStatus()
        {
            this.CWLTActionPlanActionStepStatus = new HashSet<CWLTActionPlanActionStepStatus>();
            this.ProgramActionPlanActionStepStatus = new HashSet<ProgramActionPlanActionStepStatus>();
            this.ProgramActionPlanFCCActionStepStatus = new HashSet<ProgramActionPlanFCCActionStepStatus>();
            this.SLTActionPlanActionStepStatus = new HashSet<SLTActionPlanActionStepStatus>();
        }
    
        public int CodeActionPlanActionStepStatusPK { get; set; }
        public string Abbreviation { get; set; }
        public string Description { get; set; }
        public int OrderBy { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CWLTActionPlanActionStepStatus> CWLTActionPlanActionStepStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramActionPlanActionStepStatus> ProgramActionPlanActionStepStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramActionPlanFCCActionStepStatus> ProgramActionPlanFCCActionStepStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SLTActionPlanActionStepStatus> SLTActionPlanActionStepStatus { get; set; }
    }
}
