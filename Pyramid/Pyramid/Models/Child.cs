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
    
    public partial class Child
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Child()
        {
            this.ASQSE = new HashSet<ASQSE>();
            this.BehaviorIncident = new HashSet<BehaviorIncident>();
            this.ChildClassroom = new HashSet<ChildClassroom>();
            this.ChildNote = new HashSet<ChildNote>();
            this.ChildProgram = new HashSet<ChildProgram>();
            this.ChildStatus = new HashSet<ChildStatus>();
            this.OtherSEScreen = new HashSet<OtherSEScreen>();
        }
    
        public int ChildPK { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string EthnicitySpecify { get; set; }
        public string FirstName { get; set; }
        public string GenderSpecify { get; set; }
        public string LastName { get; set; }
        public string RaceSpecify { get; set; }
        public int EthnicityCodeFK { get; set; }
        public int GenderCodeFK { get; set; }
        public int RaceCodeFK { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ASQSE> ASQSE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BehaviorIncident> BehaviorIncident { get; set; }
        public virtual CodeEthnicity CodeEthnicity { get; set; }
        public virtual CodeGender CodeGender { get; set; }
        public virtual CodeRace CodeRace { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChildClassroom> ChildClassroom { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChildNote> ChildNote { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChildProgram> ChildProgram { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChildStatus> ChildStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OtherSEScreen> OtherSEScreen { get; set; }
    }
}
