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
    
    public partial class StateSettings
    {
        public int StateSettingsPK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> DueDatesBeginDate { get; set; }
        public Nullable<int> DueDatesDaysUntilWarning { get; set; }
        public bool DueDatesEnabled { get; set; }
        public Nullable<decimal> DueDatesMonthsStart { get; set; }
        public Nullable<decimal> DueDatesMonthsEnd { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public int StateFK { get; set; }
    
        public virtual State State { get; set; }
    }
}
