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
    
    public partial class ScoreASQSE
    {
        public int ScoreASQSEPK { get; set; }
        public int Version { get; set; }
        public int MaxScore { get; set; }
        public int CutoffScore { get; set; }
        public int IntervalCodeFK { get; set; }
        public int MonitoringScoreStart { get; set; }
        public int MonitoringScoreEnd { get; set; }
    
        public virtual CodeASQSEInterval CodeASQSEInterval { get; set; }
    }
}