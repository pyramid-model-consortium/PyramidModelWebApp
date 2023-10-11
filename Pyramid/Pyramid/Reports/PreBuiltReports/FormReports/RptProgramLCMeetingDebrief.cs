using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;
using System.Collections.Generic;
using DevExpress.XtraReports.UI;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptProgramLCMeetingDebrief : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptProgramLCMeetingDebrief()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptProgramLCMeetingDebrief_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.ProgramLCMeetingDebrief currentMeetingDebrief;
            List<Models.PLTMember> activeTeamMembers;
            List<Models.ProgramLCMeetingDebriefSession> currentSessions;
            List<Models.ProgramLCMeetingDebriefSessionAttendee> currentSessionAttendees;

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentMeetingDebrief = context.ProgramLCMeetingDebrief
                                            .Include(d => d.Program)
                                            .Include(d => d.Program.Cohort)
                                            .Include(d => d.ProgramLCMeetingDebriefSession)
                                            .AsNoTracking()
                                            .Where(d => d.ProgramLCMeetingDebriefPK == formPK)
                                            .FirstOrDefault();

                if (currentMeetingDebrief != null)
                {
                    activeTeamMembers = context.PLTMember.AsNoTracking()
                                                .Include(tm => tm.Program)
                                                .Where(tm => tm.ProgramFK == currentMeetingDebrief.ProgramFK &&
                                                             tm.StartDate.Year <= currentMeetingDebrief.DebriefYear &&
                                                             (tm.LeaveDate.HasValue == false ||
                                                                tm.LeaveDate.Value.Year >= currentMeetingDebrief.DebriefYear))
                                                .ToList();
                    currentSessions = currentMeetingDebrief.ProgramLCMeetingDebriefSession.ToList();
                    currentSessionAttendees = context.ProgramLCMeetingDebriefSessionAttendee
                                                    .Include(a => a.ProgramLCMeetingDebriefSession)
                                                    .Include(a => a.PLTMember)
                                                    .AsNoTracking()
                                                    .Where(a => a.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebriefFK == currentMeetingDebrief.ProgramLCMeetingDebriefPK)
                                                    .ToList();
                }
                else
                {
                    activeTeamMembers = new List<PLTMember>();
                    currentSessions = new List<ProgramLCMeetingDebriefSession>();
                    currentSessionAttendees = new List<ProgramLCMeetingDebriefSessionAttendee>();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            PyramidUser leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentMeetingDebrief.LeadershipCoachUsername);
            lblDebriefLeadershipCoach.Text = (leadershipCoachUser == null ? "Error!" : string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName));
            lblDebriefProgram.Text = currentMeetingDebrief.Program.ProgramName;
            lblDebriefCohort.Text = currentMeetingDebrief.Program.Cohort.CohortName;
            lblDebriefLaunchDate.Text = currentMeetingDebrief.Program.ProgramStartDate.ToString("MM/dd/yyyy");
            lblDebriefYear.Text = currentMeetingDebrief.DebriefYear.ToString();
            lblDebriefAddress.Text = currentMeetingDebrief.LocationAddress;
            lblDebriefEmail.Text = currentMeetingDebrief.PrimaryContactEmail;
            lblDebriefPhone.Text = (string.IsNullOrWhiteSpace(currentMeetingDebrief.PrimaryContactPhone) ? "" : Code.Utilities.FormatPhoneNumber(currentMeetingDebrief.PrimaryContactPhone, "US"));

            //------ Leadership Team Information ------
            //Set the detail report source and sorting
            LeadershipTeamDetailReport.DataSource = activeTeamMembers;
            LeadershipTeamDetail.SortFields.Add(new GroupField("IDNumber", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblTeamMemberID.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IDNumber"));
            lblTeamMemberFirstName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FirstName"));
            lblTeamMemberLastName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LastName"));
            lblTeamMemberEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));

            //------ Session Debrief Information ------
            //Set the detail report source and sorting
            var sessionDebriefInformation = currentSessions.Select(s => new
            {
                s.ProgramLCMeetingDebriefSessionPK,
                Attendees = string.Join(", ", currentSessionAttendees
                                                .Where(a => a.ProgramLCMeetingDebriefSessionFK == s.ProgramLCMeetingDebriefSessionPK)
                                                .Select(a => string.Format("({0}) {1} {2}", a.PLTMember.IDNumber, a.PLTMember.FirstName, a.PLTMember.LastName))
                                                .ToList()),
                s.SessionStartDateTime,
                s.SessionEndDateTime,
                s.NextSessionStartDateTime,
                s.NextSessionEndDateTime,
                s.ReviewedActionPlan,
                s.ReviewedBOQ,
                s.ReviewedTPITOS,
                s.ReviewedTPOT,
                s.ReviewedOtherItem,
                s.ReviewedOtherItemSpecify,
                s.SessionSummary,
                s.SessionNextSteps
            }).ToList();
            SessionDebriefDetailReport.DataSource = sessionDebriefInformation;
            SessionDebriefDetail.SortFields.Add(new GroupField("SessionStartDateTime", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblSessionAttendees.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Attendees"));
            lblSessionDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionStartDateTime"));
            lblNextSessionDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NextSessionStartDateTime"));
            chkReviewedActionPlan.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedActionPlan"));
            chkReviewedBOQ.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedBOQ"));
            chkReviewedTPITOS.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedTPITOS"));
            chkReviewedTPOT.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedTPOT"));
            chkReviewedOther.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedOtherItem"));
            lblSessionReviewedOtherItemSpecify.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ReviewedOtherItemSpecify"));
            lblSessionNextSteps.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionNextSteps"));
            lblSessionSummary.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionSummary"));
        }
    }
}
