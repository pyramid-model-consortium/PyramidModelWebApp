using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;
using System.Collections.Generic;
using DevExpress.XtraReports.UI;
using System.Web;
using Pyramid.Code;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptLeadershipCoachLog : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptLeadershipCoachLog()
        {
            InitializeComponent();
        }

        private void RptLeadershipCoachLog_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Models.LeadershipCoachLog currentLCL;
            List<LCLTeamMemberEngagement> teamMembersEngaged;
            List<LCLInvolvedCoach> involvedCoaches;
            PyramidUser leadershipCoachUserRecord;

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            //Get the user role FK
            int userRoleFK = Convert.ToInt32(ParamUserRoleFK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the object
                currentLCL = context.LeadershipCoachLog
                                            .Include(lcl => lcl.GoalCompletionLikelihood)
                                            .Include(lcl => lcl.TimelyProgressionLikelihood)
                                            .Include(lcl => lcl.LCLResponse)
                                            .Include(lcl => lcl.LCLResponse.Select(lclr => lclr.CodeLCLResponse))
                                            .Include(lcl => lcl.Program)
                                            .Include(lcl => lcl.Program.ProgramType)
                                            .Include(lcl => lcl.Program.ProgramType.Select(pt => pt.CodeProgramType))
                                            .AsNoTracking()
                                            .Where(lcl => lcl.LeadershipCoachLogPK == formPK)
                                            .FirstOrDefault();

                //Get the team members engaged
                teamMembersEngaged = context.LCLTeamMemberEngagement
                                            .Include(ltme => ltme.PLTMember)
                                            .Include(ltme => ltme.PLTMember.PLTMemberRole)
                                            .Include(ltme => ltme.PLTMember.PLTMemberRole.Select(pmr => pmr.CodeTeamPosition))
                                            .AsNoTracking()
                                            .Where(ltme => ltme.LeadershipCoachLogFK == formPK)
                                            .ToList();

                //Get the involved coaches
                involvedCoaches = context.LCLInvolvedCoach
                                         .Include(lic => lic.ProgramEmployee)
                                         .Include(lic => lic.ProgramEmployee.Employee)
                                         .AsNoTracking()
                                         .Where(lic => lic.LeadershipCoachLogFK == formPK)
                                         .ToList();
            }

            leadershipCoachUserRecord = Models.PyramidUser.GetUserRecordByUsername(currentLCL.LeadershipCoachUsername);

            if (currentLCL != null)
            {
                lblBIProgram.Text = currentLCL.Program.ProgramName;
                lblBIProgramIDNumber.Text = (string.IsNullOrWhiteSpace(currentLCL.Program.IDNumber) ? "No ID Number Registered" : currentLCL.Program.IDNumber);
                lblBIProgramTypes.Text = string.Join(", ", currentLCL.Program.ProgramType.OrderBy(pt => pt.CodeProgramType.Description).Select(pt => pt.CodeProgramType.Description));
                lblBILeadershipCoach.Text = (leadershipCoachUserRecord != null ? string.Format("{0} {1}", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName) : "" );

                List<string> involvedCoachStringList;

                //Set the visibility of coach names
                //As per Summer Edwards on 03/27/2023, allow the Leadership Coach role to see the names
                if (ParamViewPrivateEmployeeInfo.Value != null && (Convert.ToBoolean(ParamViewPrivateEmployeeInfo.Value) || userRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH))
                {
                    involvedCoachStringList = involvedCoaches.Select(ic => string.Format("({0}) {1} {2}", ic.ProgramEmployee.ProgramSpecificID, ic.ProgramEmployee.Employee.FirstName, ic.ProgramEmployee.Employee.LastName)).OrderBy(ic => ic).ToList();
                }
                else
                {
                    involvedCoachStringList = involvedCoaches.Select(ic => ic.ProgramEmployee.ProgramSpecificID).OrderBy(ic => ic).ToList();
                }
                
                lblBIInvolvedCoaches.Text = string.Join(", ", involvedCoachStringList);

                if (currentLCL.IsMonthly.HasValue)
                {
                    lblBILogType.Text = (currentLCL.IsMonthly.Value ? "Cumulative record of engagements over one month period" : "Single engagement/encounter");
                    lblBIDateCompletedLabel.Text = (currentLCL.IsMonthly.Value ? "Month of log" : "Date Completed");
                    lblBIDateCompleted.Text = (currentLCL.DateCompleted.HasValue ? (currentLCL.IsMonthly.Value ? currentLCL.DateCompleted.Value.ToString("MM/yyyy") : currentLCL.DateCompleted.Value.ToString("MM/dd/yyyy")) : "");
                    lblTypeSpecific1Label.Text = (currentLCL.IsMonthly.Value ? "Number of communication/engagements" : "Total Duration (Hours)");
                    lblTypeSpecific2Label.Text = (currentLCL.IsMonthly.HasValue ? (currentLCL.IsMonthly.Value ? "Number of attempted communication/engagements" : "Total Duration (Minutes)") : "");
                    
                    if (currentLCL.IsMonthly.Value && currentLCL.NumberOfEngagements.HasValue)
                    {
                        lblTypeSpecific1.Text = currentLCL.NumberOfEngagements.Value.ToString();
                    }
                    else if (currentLCL.IsMonthly.Value == false && currentLCL.TotalDurationHours.HasValue)
                    {
                        lblTypeSpecific1.Text = currentLCL.TotalDurationHours.Value.ToString();
                    }
                    
                    if (currentLCL.IsMonthly.Value && currentLCL.NumberOfAttemptedEngagements.HasValue)
                    {
                        lblTypeSpecific2.Text = currentLCL.NumberOfAttemptedEngagements.Value.ToString();
                    }
                    else if (currentLCL.IsMonthly.Value == false && currentLCL.TotalDurationMinutes.HasValue)
                    {
                        lblTypeSpecific2.Text = currentLCL.TotalDurationMinutes.Value.ToString();
                    }
                }

                lblBICyclePhase.Text = (currentLCL.CyclePhase.HasValue ? (currentLCL.CyclePhase.Value == 99 ? "Not applicable" : currentLCL.CyclePhase.Value.ToString()) : "");
                lblBITimelyProgress.Text = (currentLCL.TimelyProgressionLikelihood != null ? currentLCL.TimelyProgressionLikelihood.Description : "");
                lblBIGoalCompletion.Text = (currentLCL.GoalCompletionLikelihood != null ? currentLCL.GoalCompletionLikelihood.Description : "");
                
                //Team member engagement
                RptLeadershipCoachLog_TeamEngagement engagementSubReport = new RptLeadershipCoachLog_TeamEngagement();
                var engagementReportDataSource = teamMembersEngaged.Select(tme => new {
                    tme.PLTMember.PLTMemberPK,
                    MemberIDAndName = string.Format("({0}) {1} {2}", tme.PLTMember.IDNumber, tme.PLTMember.FirstName, tme.PLTMember.LastName),
                    tme.PLTMember.EmailAddress,
                    Roles = string.Join(", ", tme.PLTMember.PLTMemberRole.Select(r => r.CodeTeamPosition.Description).ToList()),
                    StartDate = tme.PLTMember.StartDate
                })
                .OrderBy(tme => tme.MemberIDAndName)
                .ToList();

                engagementSubReport.DataSource = engagementReportDataSource;
                engagementSubReport.Detail.SortFields.Add(new GroupField("MemberIDAndName", XRColumnSortOrder.Ascending));
                engagementSubReport.lblTMEIDAndName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MemberIDAndName"));
                engagementSubReport.lblTMEEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));
                engagementSubReport.lblTMERoles.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Roles"));
                engagementSubReport.lblTMEStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));

                subRptTeamEngagement.ReportSource = engagementSubReport;

                lblBIOtherEngagementSpecify.Text = currentLCL.OtherEngagementSpecify;
                List<LCLResponse> currentResponseObjects = currentLCL.LCLResponse
                                                .OrderBy(r => r.CodeLCLResponse.Group)
                                                .ThenBy(r => r.CodeLCLResponse.OrderBy)
                                                .ToList();
                lblBIDomain1.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_1);
                lblBIDomain2.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_2);
                lblBIDomain2Specify.Text = currentLCL.OtherDomainTwoSpecify;
                lblBIDomain3.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_3);
                lblBIDomain4.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_4);
                lblBIDomain5.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_5);
                lblBIDomain6.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.DOMAIN_6);

                lblBIProgramBarriers.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_BARRIERS);
                lblBIProgramStrengths.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_STRENGTHS);
                lblBISiteResources.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.SITE_RESOURCES);
                lblBISiteResourcesSpecify.Text = currentLCL.OtherSiteResourcesSpecify;

                lblBITopicsDiscussed.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.TOPICS_DISCUSSED);
                lblBITopicsDiscussedSpecify.Text = currentLCL.OtherTopicsDiscussedSpecify;
                lblBITargetedTrainingHours.Text = currentLCL.TargetedTrainingHours.ToString();
                lblBITargetedTrainingMinutes.Text = currentLCL.TargetedTrainingMinutes.ToString();
                lblBITrainingsCovered.Text = GetSelectedLCLResponseString(currentResponseObjects, CodeLCLResponse.ResponseGroups.TRAININGS_COVERED);
                lblBITrainingsCoveredSpecify.Text = currentLCL.OtherTrainingsCoveredSpecify;

                lblBIThinkNarrative.Text = currentLCL.ThinkNarrative;
                lblBIActNarrative.Text = currentLCL.ActNarrative;
                lblBIHighlightsNarrative.Text = currentLCL.HighlightsNarrative;
            }
        }
        
        private string GetSelectedLCLResponseString(List<Models.LCLResponse> selectedResponseObjects, string currentResponseGroup)
        {
            //The value to return
            string valueToReturn = "";

            //Get the items in the group that were selected
            List<LCLResponse> groupItemsSelected = selectedResponseObjects.Where(r => r.CodeLCLResponse.Group == currentResponseGroup).ToList();

            //Determine if any items were selected
            if (groupItemsSelected != null && groupItemsSelected.Count > 0)
            {
                valueToReturn = string.Join(", ", groupItemsSelected.Select(i => i.CodeLCLResponse.Description).ToList());
            }

            //Return the value
            return valueToReturn;
        }
    }
}
