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
    
    public partial class rspCoachingLog_Result
    {
        public int CoachingLogPK { get; set; }
        public System.DateTime LogDate { get; set; }
        public int DurationMinutes { get; set; }
        public bool FUEmail { get; set; }
        public bool FUInPerson { get; set; }
        public bool FUNone { get; set; }
        public bool FUPhone { get; set; }
        public bool MEETDemonstration { get; set; }
        public bool MEETEnvironment { get; set; }
        public bool MEETGoalSetting { get; set; }
        public bool MEETGraphic { get; set; }
        public bool MEETMaterial { get; set; }
        public bool MEETOther { get; set; }
        public string MEETOtherSpecify { get; set; }
        public bool MEETPerformance { get; set; }
        public bool MEETProblemSolving { get; set; }
        public bool MEETReflectiveConversation { get; set; }
        public bool MEETRoleplay { get; set; }
        public bool MEETVideo { get; set; }
        public string Narrative { get; set; }
        public bool OBSConductTPITOS { get; set; }
        public bool OBSConductTPOT { get; set; }
        public bool OBSEnvironment { get; set; }
        public bool OBSModeling { get; set; }
        public bool OBSObserving { get; set; }
        public bool OBSOther { get; set; }
        public bool OBSOtherHelp { get; set; }
        public string OBSOtherSpecify { get; set; }
        public bool OBSProblemSolving { get; set; }
        public bool OBSReflectiveConversation { get; set; }
        public bool OBSSideBySide { get; set; }
        public bool OBSVerbalSupport { get; set; }
        public string CoachID { get; set; }
        public string CoachFirstName { get; set; }
        public string CoachLastName { get; set; }
        public string ProgramName { get; set; }
        public string CoacheeNames { get; set; }
    }
}