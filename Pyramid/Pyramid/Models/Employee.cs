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
    
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            this.ProgramEmployee = new HashSet<ProgramEmployee>();
            this.Training = new HashSet<Training>();
        }
    
        public int EmployeePK { get; set; }
        public string AspireEmail { get; set; }
        public Nullable<int> AspireID { get; set; }
        public bool AspireVerified { get; set; }
        public string Creator { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Editor { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public string EmailAddress { get; set; }
        public string EthnicitySpecify { get; set; }
        public string FirstName { get; set; }
        public string GenderSpecify { get; set; }
        public string LastName { get; set; }
        public string RaceSpecify { get; set; }
        public int EthnicityCodeFK { get; set; }
        public int GenderCodeFK { get; set; }
        public int RaceCodeFK { get; set; }
    
        public virtual CodeEthnicity CodeEthnicity { get; set; }
        public virtual CodeGender CodeGender { get; set; }
        public virtual CodeRace CodeRace { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProgramEmployee> ProgramEmployee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Training> Training { get; set; }
    }
}
