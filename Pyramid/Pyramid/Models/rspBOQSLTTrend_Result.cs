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
    
    public partial class rspBOQSLTTrend_Result
    {
        public int BOQPK { get; set; }
        public System.DateTime FormDate { get; set; }
        public string GroupingValue { get; set; }
        public string GroupingText { get; set; }
        public int StateFK { get; set; }
        public string StateName { get; set; }
        public int SectionOrderBy { get; set; }
        public string SectionAbbr { get; set; }
        public string SectionName { get; set; }
        public string SectionColor { get; set; }
        public int CriticalElementOrderBy { get; set; }
        public string CriticalElementAbbr { get; set; }
        public string CriticalElementName { get; set; }
        public decimal CriticalElementAvg { get; set; }
        public int CriticalElementNumNotInPlace { get; set; }
        public int CriticalElementNumNeedsImprovement { get; set; }
        public int CriticalElementNumInPlace { get; set; }
    }
}