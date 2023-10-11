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
    public partial class RptCWLTActionPlan : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptCWLTActionPlan()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptCWLTActionPlan_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.CWLTActionPlan currentActionPlan;
            List<Models.CWLTMember> activeCWLTMembers = new List<CWLTMember>();
            List<Models.CWLTActionPlanMeeting> currentMeetings = new List<CWLTActionPlanMeeting>();
            List<Models.HubLCMeetingSchedule> currentLCMeetingSchedules = new List<HubLCMeetingSchedule>();
            List<Models.CWLTActionPlanGroundRule> currentGroundRules = new List<CWLTActionPlanGroundRule>();
            int numActivePrograms = 0;
            List<CodeBOQCriticalElement> allBOQCriticalElements = new List<CodeBOQCriticalElement>();
            Models.BenchmarkOfQualityCWLT mostRecentBOQ = new BenchmarkOfQualityCWLT();
            List<spGetBOQCWLTIndicatorValues_Result> mostRecentBOQIndicators = new List<spGetBOQCWLTIndicatorValues_Result>();
            List<spGetBOQCWLTIndicatorValues_Result> mostRecentBOQIndicatorsThatNeedImprovement = new List<spGetBOQCWLTIndicatorValues_Result>();
            List<Models.CWLTActionPlanActionStep> currentActionSteps = new List<CWLTActionPlanActionStep>();

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.CWLTActionPlan
                                            .Include(ap => ap.Hub)
                                            .Include(ap => ap.CWLTMember)
                                            .AsNoTracking()
                                            .Where(ap => ap.CWLTActionPlanPK == formPK)
                                            .FirstOrDefault();

                if (currentActionPlan != null)
                {
                    //Get the leadership coaches
                    List<PyramidUser> allLeadershipCoaches = PyramidUser.GetHubLeadershipCoachUserRecords(new List<int>() { currentActionPlan.HubFK });

                    activeCWLTMembers = context.CWLTMember.AsNoTracking()
                                                .Include(cm => cm.Hub)
                                                .Where(cm => cm.HubFK == currentActionPlan.HubFK &&
                                                       cm.StartDate <= currentActionPlan.ActionPlanEndDate &&
                                                       (cm.LeaveDate.HasValue == false ||
                                                            cm.LeaveDate >= currentActionPlan.ActionPlanStartDate))
                                                .ToList();

                    currentMeetings = context.CWLTActionPlanMeeting
                                                .AsNoTracking()
                                                .Where(m => m.CWLTActionPlanFK == formPK)
                                                .ToList();

                    //Get the schedules for the hub and year combination
                    currentLCMeetingSchedules = context.HubLCMeetingSchedule.AsNoTracking()
                                                .Include(p => p.Hub)
                                                .Where(p => p.HubFK == currentActionPlan.HubFK &&
                                                            (p.MeetingYear == currentActionPlan.ActionPlanStartDate.Year ||
                                                                p.MeetingYear == currentActionPlan.ActionPlanEndDate.Year))
                                                .ToList();

                    //Set the full leadership coach name
                    foreach (HubLCMeetingSchedule schedule in currentLCMeetingSchedules)
                    {
                        //Get the leadership coach user record
                        PyramidUser leadershipCoachUserRecord = allLeadershipCoaches.Where(c => c.UserName == schedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUserRecord != null)
                        {
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                        else
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are hub-specific and can be removed
                            leadershipCoachUserRecord = PyramidUser.GetUserRecordByUsername(schedule.LeadershipCoachUsername);

                            //Set the label text (include the username for searching
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                    }

                    currentGroundRules = context.CWLTActionPlanGroundRule
                                                .AsNoTracking()
                                                .Where(gr => gr.CWLTActionPlanFK == formPK)
                                                .ToList();

                    //Get the counts
                    //Active programs
                    numActivePrograms = context.Program.AsNoTracking()
                                                    .Where(p => p.HubFK == currentActionPlan.HubFK &&
                                                                p.ProgramStartDate <= currentActionPlan.ActionPlanEndDate &&
                                                                (p.ProgramEndDate.HasValue == false || p.ProgramEndDate.Value >= currentActionPlan.ActionPlanStartDate))
                                                    .Count();

                    allBOQCriticalElements = context.CodeBOQCriticalElement.AsNoTracking()
                                                        .Where(ce => ce.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQ)
                                                        .OrderBy(ce => ce.OrderBy)
                                                        .ToList();

                    //Get the action plan start date minus 6 months for the calculation below
                    DateTime startOfTimeframe = currentActionPlan.ActionPlanStartDate.AddMonths(-6);

                    //Get the most recent BOQ
                    mostRecentBOQ = context.BenchmarkOfQualityCWLT.AsNoTracking()
                                                    .Where(boq => boq.HubFK == currentActionPlan.HubFK &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= currentActionPlan.ActionPlanEndDate)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();

                    //Make sure there is a most recent BOQ
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityCWLTPK > 0)
                    {
                        //Get the BOQ indicator information
                        mostRecentBOQIndicators = context.spGetBOQCWLTIndicatorValues(mostRecentBOQ.BenchmarkOfQualityCWLTPK).ToList();

                        //Get the indicators that need improvement
                        mostRecentBOQIndicatorsThatNeedImprovement = mostRecentBOQIndicators.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();
                    }

                    currentActionSteps = context.CWLTActionPlanActionStep
                                                .Include(s => s.CodeBOQIndicator)
                                                .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                .Include(s => s.CWLTActionPlanActionStepStatus)
                                                .Include(s => s.CWLTActionPlanActionStepStatus.Select(ss => ss.CodeActionPlanActionStepStatus))
                                                .AsNoTracking()
                                                .Where(s => s.CWLTActionPlanFK == formPK)
                                                .ToList();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            PyramidUser leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentActionPlan.LeadershipCoachUsername);
            lblBIIsLeadershipCoachInvolved.Text = (currentActionPlan.IsLeadershipCoachInvolved ? "Yes" : "No");
            lblBILeadershipCoach.Text = (leadershipCoachUser == null ? "" : string.Format("{0} {1}", leadershipCoachUser.FirstName, leadershipCoachUser.LastName));
            lblBILeadershipCoachEmail.Text = (leadershipCoachUser == null ? "" : leadershipCoachUser.Email);
            lblBIHub.Text = currentActionPlan.Hub.Name;
            lblBIActionPlanStartDate.Text = currentActionPlan.ActionPlanStartDate.ToString("MM/dd/yyyy");
            lblBIActionPlanEndDate.Text = currentActionPlan.ActionPlanEndDate.ToString("MM/dd/yyyy");
            lblBICoordinator.Text = string.Format("({0}) {1} {2}", currentActionPlan.CWLTMember.IDNumber, currentActionPlan.CWLTMember.FirstName, currentActionPlan.CWLTMember.LastName);
            lblBICoordinatorEmail.Text = currentActionPlan.CWLTMember.EmailAddress;
            lblBIMissionStatement.Text = currentActionPlan.MissionStatement;
            lblBIAdditionalNotes.Text = currentActionPlan.AdditionalNotes;

            //------ Active Leadership Team Members ------
            //Set the detail report source and sorting
            LeadershipTeamDetailReport.DataSource = activeCWLTMembers;
            LeadershipTeamDetail.SortFields.Add(new GroupField("IDNumber", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("FirstName", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("LastName", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblLTTeamMember.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Concat('(', [IDNumber], ') ', [FirstName], ' ', [LastName])"));
            lblLTStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblLTEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));
            lblLTLeaveDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeaveDate"));
            lblLTHub.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Hub.Name]"));

            //------ All Meeting Dates ------
            //Set the detail report source and sorting
            MeetingDatesDetailReport.DataSource = currentMeetings;
            MeetingDatesDetail.SortFields.Add(new GroupField("MeetingDate", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblMDMeetingDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MeetingDate"));
            lblMDLCAttended.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([LeadershipCoachAttendance] == true, 'Yes', 'No')"));
            lblMDNotes.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MeetingNotes"));

            //------ Meeting Dates Proposed by Leadership Coaches ------
            //Set the detail report source and sorting
            LCSchedulesDetailReport.DataSource = currentLCMeetingSchedules;
            LCSchedulesDetail.SortFields.Add(new GroupField("LeadershipCoachUsername", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblLCSHub.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Hub.Name"));
            lblLCSYear.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MeetingYear"));
            lblLCSLeadershipCoach.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeadershipCoachUsername"));
            lblLCSTotalMeetings.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "TotalMeetings"));
            chkLCSJanMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInJan"));
            chkLCSFebMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInFeb"));
            chkLCSMarMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInMar"));
            chkLCSAprMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInApr"));
            chkLCSMayMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInMay"));
            chkLCSJunMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInJun"));
            chkLCSJulMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInJul"));
            chkLCSAugMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInAug"));
            chkLCSSepMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInSep"));
            chkLCSOctMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInOct"));
            chkLCSNovMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInNov"));
            chkLCSDecMeeting.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "CheckBoxState", "MeetingInDec"));

            //------ Ground Rules ------
            //Set the detail report source and sorting
            GroundRulesDetailReport.DataSource = currentGroundRules;
            GroundRulesDetail.SortFields.Add(new GroupField("GroundRuleNumber", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblGRGroundRuleDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroundRuleDescription"));
            lblGRGroundRuleNumber.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroundRuleNumber"));

            //------ Hub Information ------
            lblHINumberActivePrograms.Text = numActivePrograms.ToString();

            //------ Benchmark of Quality Information ------

            //-- Critical Elements section --
            //Set the detail report source and sorting
            BOQCriticalElementDetailReport.DataSource = allBOQCriticalElements;
            BOQCriticalElementDetail.SortFields.Add(new GroupField("OrderBy", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblBCECriticalElement.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Concat([Abbreviation], ' = ', [Description])"));

            //-- Most Recent BOQ section --
            //Set the detail report source and sorting
            BOQIndicatorsDetailReport.DataSource = mostRecentBOQIndicatorsThatNeedImprovement;
            BOQIndicatorsDetail.SortFields.Add(new GroupField("IndicatorNumber", XRColumnSortOrder.Ascending));

            if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityCWLTPK > 0)
            {
                //Set the most recent BOQ date label
                lblBIMostRecentBOQDate.Text = mostRecentBOQ.FormDate.ToString("MM/dd/yyyy");
            }
            else
            {
                lblBIMostRecentBOQDate.Text = "N/A";
            }

            //Set the detail band label expressions
            lblBICriticalElement.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CriticalElementAbbreviation"));
            lblBIIndicatorNum.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IndicatorNumber"));
            lblBIIndicatorDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IndicatorDescription"));
            lblBIIndicatorStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IndicatorValueDescription"));

            //Set the colors
            lblBICriticalElement.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorNum.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));

            //------ Benchmark of Quality Action Steps ------

            //-- Action Step section --
            //Set the detail report source and sorting
            ActionStepsDetailReport.DataSource = currentActionSteps;
            ActionStepsDetail.SortFields.Add(new GroupField("CodeBOQIndicator.IndicatorNumber", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblASIndicator.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Concat('(', [CodeBOQIndicator.CodeBOQCriticalElement.Abbreviation], ') ', [CodeBOQIndicator.IndicatorNumber])"));
            lblASCriticalElement.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CodeBOQIndicator.CodeBOQCriticalElement.Description]"));
            lblASIndicatorDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CodeBOQIndicator.Description]"));
            lblASProblem.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ProblemIssueTask]"));
            lblASActionStep.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ActionStepActivity]"));
            lblASPersonsResponsible.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[PersonsResponsible]"));
            lblASTargetDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[TargetDate]"));

            //-- Action Step Status section --
            //Set the detail report source and sorting
            ActionStepStatusDetailReport.DataSource = currentActionSteps;
            ActionStepStatusDetailReport.DataMember = "CWLTActionPlanActionStepStatus";
            ActionStepStatusDetail.SortFields.Add(new GroupField("StatusDate", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblASSStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CodeActionPlanActionStepStatus.Description]"));
            lblASSStatusDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[StatusDate]"));
        }
    }
}
