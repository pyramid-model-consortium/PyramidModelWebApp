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
    
    public partial class Training
    {
        public int TrainingPK { get; set; }
        public Nullable<int> AspireEventAttendeeID { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public Nullable<System.DateTime> ExpirationDate { get; set; }
        public bool IsAspireTraining { get; set; }
        public System.DateTime TrainingDate { get; set; }
        public int EmployeeFK { get; set; }
        public int TrainingCodeFK { get; set; }
    
        public virtual CodeTraining CodeTraining { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
