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
    public partial class RptCoachingCircleLCMeetingDebrief : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptCoachingCircleLCMeetingDebrief()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptCoachingCircleLCMeetingDebrief_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.CoachingCircleLCMeetingDebrief currentMeetingDebrief;
            List<Models.CoachingCircleLCMeetingDebriefTeamMember> currentTeamMembers;
            List<Models.CoachingCircleLCMeetingDebriefSession> currentSessions;
            List<Models.CoachingCircleLCMeetingDebriefSessionAttendee> currentSessionAttendees;
            List<Models.CodeTeamPosition> codeTeamPositions;

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentMeetingDebrief = context.CoachingCircleLCMeetingDebrief
                                            .Include(d => d.State)
                                            .Include(d => d.CoachingCircleLCMeetingDebriefTeamMember)
                                            .Include(d => d.CoachingCircleLCMeetingDebriefSession)
                                            .AsNoTracking()
                                            .Where(d => d.CoachingCircleLCMeetingDebriefPK == formPK)
                                            .FirstOrDefault();

                //Get the code team position rows
                codeTeamPositions = context.CodeTeamPosition.AsNoTracking().ToList();

                if (currentMeetingDebrief != null)
                {
                    currentTeamMembers = currentMeetingDebrief.CoachingCircleLCMeetingDebriefTeamMember.ToList();
                    currentSessions = currentMeetingDebrief.CoachingCircleLCMeetingDebriefSession.ToList();
                    currentSessionAttendees = context.CoachingCircleLCMeetingDebriefSessionAttendee
                                                    .Include(a => a.CoachingCircleLCMeetingDebriefSession)
                                                    .Include(a => a.CoachingCircleLCMeetingDebriefTeamMember)
                                                    .AsNoTracking()
                                                    .Where(a => a.CoachingCircleLCMeetingDebriefSession.CoachingCircleLCMeetingDebriefFK == currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK)
                                                    .ToList();
                }
                else
                {
                    currentTeamMembers = new List<CoachingCircleLCMeetingDebriefTeamMember>();
                    currentSessions = new List<CoachingCircleLCMeetingDebriefSession>();
                    currentSessionAttendees = new List<CoachingCircleLCMeetingDebriefSessionAttendee>();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            PyramidUser leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentMeetingDebrief.LeadershipCoachUsername);
            lblDebriefLeadershipCoach.Text = (leadershipCoachUser == null ? "Error!" : string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName));
            lblDebriefState.Text = currentMeetingDebrief.State.Name;
            lblDebriefCoachingCircle.Text = currentMeetingDebrief.CoachingCircleName;
            lblDebriefTargetAudience.Text = currentMeetingDebrief.TargetAudience;
            lblDebriefYear.Text = currentMeetingDebrief.DebriefYear.ToString();

            //------ Leadership Team Information ------
            //Set the detail report source and sorting
            var leadershipTeamInformation = currentTeamMembers.Join(codeTeamPositions, m => m.TeamPositionCodeFK, p => p.CodeTeamPositionPK, (m, p) => new
            {
                Position = p.Description,
                m.FirstName,
                m.LastName,
                m.EmailAddress,
                PhoneNumber = (string.IsNullOrWhiteSpace(m.PhoneNumber) ? null : Code.Utilities.FormatPhoneNumber(m.PhoneNumber, "US"))
            }).ToList();
            LeadershipTeamDetailReport.DataSource = leadershipTeamInformation;
            LeadershipTeamDetail.SortFields.Add(new GroupField("LastName", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblTeamMemberPosition.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Position"));
            lblTeamMemberFirstName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FirstName"));
            lblTeamMemberLastName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LastName"));
            lblTeamMemberEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));
            lblTeamMemberPhone.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PhoneNumber"));

            //------ Session Debrief Information ------
            //Set the detail report source and sorting
            var sessionDebriefInformation = currentSessions.Select(s => new
            {
                s.CoachingCircleLCMeetingDebriefSessionPK,
                Attendees = string.Join(", ", currentSessionAttendees
                                                .Where(a => a.CoachingCircleLCMeetingDebriefSessionFK == s.CoachingCircleLCMeetingDebriefSessionPK)
                                                .Select(a => string.Format("{0} {1}", a.CoachingCircleLCMeetingDebriefTeamMember.FirstName, a.CoachingCircleLCMeetingDebriefTeamMember.LastName))
                                                .ToList()),
                s.SessionStartDateTime,
                s.SessionEndDateTime,
                s.SessionSummary
            }).ToList();
            SessionDebriefDetailReport.DataSource = sessionDebriefInformation;
            SessionDebriefDetail.SortFields.Add(new GroupField("SessionStartDateTime", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblSessionAttendees.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Attendees"));
            lblSessionDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionStartDateTime"));
            lblSessionStartTime.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionStartDateTime"));
            lblSessionEndTime.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionEndDateTime"));
            lblSessionSummary.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SessionSummary"));
        }
    }
}
