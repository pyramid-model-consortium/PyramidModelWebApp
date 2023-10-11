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
    public partial class RptProgramActionPlan : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptProgramActionPlan()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptProgramActionPlan_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.ProgramActionPlan currentActionPlan;
            List<Models.PLTMember> activePLTMembers = new List<PLTMember>();
            List<Models.ProgramActionPlanMeeting> currentMeetings = new List<ProgramActionPlanMeeting>();
            List<Models.ProgramLCMeetingSchedule> currentLCMeetingSchedules = new List<ProgramLCMeetingSchedule>();
            List<Models.ProgramActionPlanGroundRule> currentGroundRules = new List<ProgramActionPlanGroundRule>();
            int numActiveStaff = 0, numStaffHired = 0, numStaffTerminated = 0;
            List<CodeBOQCriticalElement> allBOQCriticalElements = new List<CodeBOQCriticalElement>();
            Models.BenchmarkOfQuality mostRecentBOQ = new BenchmarkOfQuality();
            List<spGetBOQIndicatorValues_Result> mostRecentBOQIndicators = new List<spGetBOQIndicatorValues_Result>();
            List<spGetBOQIndicatorValues_Result> mostRecentBOQIndicatorsThatNeedImprovement = new List<spGetBOQIndicatorValues_Result>();
            List<Models.ProgramActionPlanActionStep> currentActionSteps = new List<ProgramActionPlanActionStep>();

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.ProgramActionPlan
                                            .Include(ap => ap.Program)
                                            .Include(ap => ap.Program.Cohort)
                                            .AsNoTracking()
                                            .Where(ap => ap.ProgramActionPlanPK == formPK)
                                            .FirstOrDefault();

                if (currentActionPlan != null)
                {
                    //Get the leadership coaches
                    List<PyramidUser> allLeadershipCoaches = PyramidUser.GetProgramLeadershipCoachUserRecords(new List<int>() { currentActionPlan.ProgramFK });

                    activePLTMembers = context.PLTMember.AsNoTracking()
                                                .Include(cm => cm.Program)
                                                .Where(cm => cm.ProgramFK == currentActionPlan.ProgramFK &&
                                                       cm.StartDate <= currentActionPlan.ActionPlanEndDate &&
                                                       (cm.LeaveDate.HasValue == false ||
                                                            cm.LeaveDate >= currentActionPlan.ActionPlanStartDate))
                                                .ToList();

                    currentMeetings = context.ProgramActionPlanMeeting
                                                .AsNoTracking()
                                                .Where(m => m.ProgramActionPlanFK == formPK)
                                                .ToList();

                    //Get the schedules for the program and year combination
                    currentLCMeetingSchedules = context.ProgramLCMeetingSchedule.AsNoTracking()
                                                .Include(p => p.Program)
                                                .Where(p => p.ProgramFK == currentActionPlan.ProgramFK &&
                                                            (p.MeetingYear == currentActionPlan.ActionPlanStartDate.Year ||
                                                                p.MeetingYear == currentActionPlan.ActionPlanEndDate.Year))
                                                .ToList();

                    //Set the full leadership coach name
                    foreach (ProgramLCMeetingSchedule schedule in currentLCMeetingSchedules)
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
                            //This is necessary now that the leadership coach roles are program-specific and can be removed
                            leadershipCoachUserRecord = PyramidUser.GetUserRecordByUsername(schedule.LeadershipCoachUsername);

                            //Set the label text (include the username for searching
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                    }

                    currentGroundRules = context.ProgramActionPlanGroundRule
                                                .AsNoTracking()
                                                .Where(gr => gr.ProgramActionPlanFK == formPK)
                                                .ToList();

                    //Get the counts
                    //Active employees
                    numActiveStaff = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentActionPlan.ProgramFK &&
                                                                pe.HireDate <= currentActionPlan.ActionPlanEndDate &&
                                                                (pe.TermDate.HasValue == false || pe.TermDate.Value >= currentActionPlan.ActionPlanStartDate))
                                                    .Count();

                    //Hired in the year
                    numStaffHired = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentActionPlan.ProgramFK &&
                                                                pe.HireDate >= currentActionPlan.ActionPlanStartDate &&
                                                                pe.HireDate <= currentActionPlan.ActionPlanEndDate)
                                                    .Count();

                    //Terminated in the year
                    numStaffTerminated = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentActionPlan.ProgramFK &&
                                                                pe.TermDate.HasValue &&
                                                                pe.TermDate.Value >= currentActionPlan.ActionPlanStartDate &&
                                                                pe.TermDate.Value <= currentActionPlan.ActionPlanEndDate)
                                                    .Count();

                    allBOQCriticalElements = context.CodeBOQCriticalElement.AsNoTracking()
                                                        .Where(ce => ce.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQ)
                                                        .OrderBy(ce => ce.OrderBy)
                                                        .ToList();

                    //Get the action plan start date minus 6 months for the calculation below
                    DateTime startOfTimeframe = currentActionPlan.ActionPlanStartDate.AddMonths(-6);

                    //Get the most recent BOQ
                    mostRecentBOQ = context.BenchmarkOfQuality.AsNoTracking()
                                                    .Where(boq => boq.ProgramFK == currentActionPlan.ProgramFK &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= currentActionPlan.ActionPlanEndDate)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();

                    //Make sure there is a most recent BOQ
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityPK > 0)
                    {
                        //Get the BOQ indicator information
                        mostRecentBOQIndicators = context.spGetBOQIndicatorValues(mostRecentBOQ.BenchmarkOfQualityPK).ToList();

                        //Get the indicators that need improvement
                        mostRecentBOQIndicatorsThatNeedImprovement = mostRecentBOQIndicators.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQIndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();
                    }

                    currentActionSteps = context.ProgramActionPlanActionStep
                                                .Include(s => s.CodeBOQIndicator)
                                                .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                .Include(s => s.ProgramActionPlanActionStepStatus)
                                                .Include(s => s.ProgramActionPlanActionStepStatus.Select(ss => ss.CodeActionPlanActionStepStatus))
                                                .AsNoTracking()
                                                .Where(s => s.ProgramActionPlanFK == formPK)
                                                .ToList();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            PyramidUser leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentActionPlan.LeadershipCoachUsername);
            lblBIIsLeadershipCoachInvolved.Text = (currentActionPlan.IsLeadershipCoachInvolved ? "Yes" : "No");
            lblBILeadershipCoach.Text = (leadershipCoachUser == null ? "" : string.Format("{0} {1}", leadershipCoachUser.FirstName, leadershipCoachUser.LastName));
            lblBILeadershipCoachEmail.Text = (leadershipCoachUser == null ? "" : leadershipCoachUser.Email);
            lblBIProgram.Text = currentActionPlan.Program.ProgramName;
            lblBIProgramStartDate.Text = currentActionPlan.Program.ProgramStartDate.ToString("MM/dd/yyyy");
            lblBIProgramCohort.Text = currentActionPlan.Program.Cohort.CohortName;
            lblBIActionPlanStartDate.Text = currentActionPlan.ActionPlanStartDate.ToString("MM/dd/yyyy");
            lblBIActionPlanEndDate.Text = currentActionPlan.ActionPlanEndDate.ToString("MM/dd/yyyy");
            lblBIMissionStatement.Text = currentActionPlan.MissionStatement;
            lblBIAdditionalNotes.Text = currentActionPlan.AdditionalNotes;

            //------ Active Leadership Team Members ------
            //Set the detail report source and sorting
            LeadershipTeamDetailReport.DataSource = activePLTMembers;
            LeadershipTeamDetail.SortFields.Add(new GroupField("IDNumber", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("FirstName", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("LastName", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblLTTeamMember.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Concat('(', [IDNumber], ') ', [FirstName], ' ', [LastName])"));
            lblLTStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblLTEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));
            lblLTLeaveDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeaveDate"));
            lblLTProgram.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Program.ProgramName]"));

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
            lblLCSProgram.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.ProgramName"));
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

            //------ Program Staff Information ------
            lblSINumActiveStaff.Text = numActiveStaff.ToString();
            lblSINumStaffHired.Text = numStaffHired.ToString();
            lblSINumStaffTerminated.Text = numStaffTerminated.ToString();

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
            
            if(mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityPK > 0)
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
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.PARTIALLY_IN_PLACE, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorNum.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.PARTIALLY_IN_PLACE, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.PARTIALLY_IN_PLACE, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.PARTIALLY_IN_PLACE, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
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
            ActionStepStatusDetailReport.DataMember = "ProgramActionPlanActionStepStatus";
            ActionStepStatusDetail.SortFields.Add(new GroupField("StatusDate", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblASSStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CodeActionPlanActionStepStatus.Description]"));
            lblASSStatusDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[StatusDate]"));
        }
    }
}
