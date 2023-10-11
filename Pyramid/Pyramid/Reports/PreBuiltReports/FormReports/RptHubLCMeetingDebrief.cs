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
    public partial class RptHubLCMeetingDebrief : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptHubLCMeetingDebrief()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptHubLCMeetingDebrief_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.HubLCMeetingDebrief currentMeetingDebrief;
            List<Models.CWLTMember> activeTeamMembers;
            List<Models.HubLCMeetingDebriefSession> currentSessions;
            List<Models.HubLCMeetingDebriefSessionAttendee> currentSessionAttendees;
            List<Models.CodeTeamPosition> codeTeamPositions;

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentMeetingDebrief = context.HubLCMeetingDebrief
                                            .Include(d => d.Hub)
                                            .Include(d => d.Hub.Program)
                                            .Include(d => d.HubLCMeetingDebriefSession)
                                            .AsNoTracking()
                                            .Where(d => d.HubLCMeetingDebriefPK == formPK)
                                            .FirstOrDefault();

                //Get the code team position rows
                codeTeamPositions = context.CodeTeamPosition.AsNoTracking().ToList();

                if (currentMeetingDebrief != null)
                {
                    activeTeamMembers = context.CWLTMember.AsNoTracking()
                                                .Include(tm => tm.Hub)
                                                .Where(tm => tm.HubFK == currentMeetingDebrief.HubFK &&
                                                             tm.StartDate.Year <= currentMeetingDebrief.DebriefYear &&
                                                             (tm.LeaveDate.HasValue == false ||
                                                                tm.LeaveDate.Value.Year >= currentMeetingDebrief.DebriefYear))
                                                .ToList();
                    currentSessions = currentMeetingDebrief.HubLCMeetingDebriefSession.ToList();
                    currentSessionAttendees = context.HubLCMeetingDebriefSessionAttendee
                                                    .Include(a => a.HubLCMeetingDebriefSession)
                                                    .Include(a => a.CWLTMember)
                                                    .AsNoTracking()
                                                    .Where(a => a.HubLCMeetingDebriefSession.HubLCMeetingDebriefFK == currentMeetingDebrief.HubLCMeetingDebriefPK)
                                                    .ToList();
                }
                else
                {
                    activeTeamMembers = new List<CWLTMember>();
                    currentSessions = new List<HubLCMeetingDebriefSession>();
                    currentSessionAttendees = new List<HubLCMeetingDebriefSessionAttendee>();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            PyramidUser leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentMeetingDebrief.LeadershipCoachUsername);
            lblDebriefLeadershipCoach.Text = (leadershipCoachUser == null ? "Error!" : string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName));
            lblDebriefHub.Text = currentMeetingDebrief.Hub.Name;
            lblDebriefHubNumPrograms.Text = (currentMeetingDebrief.Hub.Program == null ? "Error!" : currentMeetingDebrief.Hub.Program.Count.ToString());
            lblDebriefLeadOrganization.Text = currentMeetingDebrief.LeadOrganization;
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
                s.HubLCMeetingDebriefSessionPK,
                Attendees = string.Join(", ", currentSessionAttendees
                                                .Where(a => a.HubLCMeetingDebriefSessionFK == s.HubLCMeetingDebriefSessionPK)
                                                .Select(a => string.Format("({0}) {1} {2}", a.CWLTMember.IDNumber, a.CWLTMember.FirstName, a.CWLTMember.LastName))
                                                .ToList()),
                s.SessionStartDateTime,
                s.SessionEndDateTime,
                s.NextSessionStartDateTime,
                s.NextSessionEndDateTime,
                s.ReviewedActionPlan,
                s.ReviewedBOQ,
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
            chkReviewedOther.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "ReviewedOtherItem"));
            lblSessionReviewedOtherItemSpecify.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ReviewedOtherItemSpecify"));
            lblSessionNextSteps.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionNextSteps"));
            lblSessionSummary.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionSummary"));
        }
    }
}
