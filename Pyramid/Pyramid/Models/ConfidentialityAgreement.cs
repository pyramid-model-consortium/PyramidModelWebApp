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
    
    public partial class ConfidentialityAgreement
    {
        public int ConfidentialityAgreementPK { get; set; }
        public System.DateTime AgreementDate { get; set; }
        public string Username { get; set; }
        public int StateFK { get; set; }
    
        public virtual State State { get; set; }
    }
}
