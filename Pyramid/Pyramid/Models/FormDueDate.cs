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
    
    public partial class FormDueDate
    {
        public int FormDueDatePK { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int DueEndWindow { get; set; }
        public System.DateTime DueRecommendedDate { get; set; }
        public int DueStartWindow { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string HelpText { get; set; }
        public int CodeFormFK { get; set; }
        public int StateFK { get; set; }
    
        public virtual CodeForm CodeForm { get; set; }
        public virtual State State { get; set; }
    }
}
