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
    
    public partial class ClassroomChanged
    {
        public int ClassroomChangedPK { get; set; }
        public System.DateTime ChangeDatetime { get; set; }
        public string ChangeType { get; set; }
        public string Deleter { get; set; }
        public int ClassroomPK { get; set; }
        public bool BeingServedSubstitute { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public bool IsInfantToddler { get; set; }
        public bool IsPreschool { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string ProgramSpecificID { get; set; }
        public int ProgramFK { get; set; }
    }
}
