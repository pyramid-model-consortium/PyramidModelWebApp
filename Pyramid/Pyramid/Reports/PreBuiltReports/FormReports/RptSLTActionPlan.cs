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
    public partial class RptSLTActionPlan : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptSLTActionPlan()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptSLTActionPlan_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.SLTActionPlan currentActionPlan;
            List<Models.SLTMember> activeSLTMembers = new List<SLTMember>();
            List<Models.SLTActionPlanMeeting> currentMeetings = new List<SLTActionPlanMeeting>();
            List<Models.SLTActionPlanGroundRule> currentGroundRules = new List<SLTActionPlanGroundRule>();
            int numActivePrograms = 0;
            List<CodeBOQCriticalElement> allBOQCriticalElements = new List<CodeBOQCriticalElement>();
            Models.BenchmarkOfQualitySLT mostRecentBOQ = new BenchmarkOfQualitySLT();
            List<spGetBOQSLTIndicatorValues_Result> mostRecentBOQIndicators = new List<spGetBOQSLTIndicatorValues_Result>();
            List<spGetBOQSLTIndicatorValues_Result> mostRecentBOQIndicatorsThatNeedImprovement = new List<spGetBOQSLTIndicatorValues_Result>();
            List<Models.SLTActionPlanActionStep> currentActionSteps = new List<SLTActionPlanActionStep>();

            //Get the PK
            int formPK = Convert.ToInt32(ParamFormPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.SLTActionPlan
                                            .Include(ap => ap.State)
                                            .Include(ap => ap.SLTMember)
                                            .Include(ap => ap.SLTWorkGroup)
                                            .AsNoTracking()
                                            .Where(ap => ap.SLTActionPlanPK == formPK)
                                            .FirstOrDefault();

                if (currentActionPlan != null)
                {
                    activeSLTMembers = context.SLTMember.AsNoTracking()
                                                .Include(cm => cm.State)
                                                .Where(cm => cm.StateFK == currentActionPlan.StateFK &&
                                                       cm.StartDate <= currentActionPlan.ActionPlanEndDate &&
                                                       (cm.LeaveDate.HasValue == false ||
                                                            cm.LeaveDate >= currentActionPlan.ActionPlanStartDate))
                                                .ToList();

                    currentMeetings = context.SLTActionPlanMeeting
                                                .AsNoTracking()
                                                .Where(m => m.SLTActionPlanFK == formPK)
                                                .ToList();

                    currentGroundRules = context.SLTActionPlanGroundRule
                                                .AsNoTracking()
                                                .Where(gr => gr.SLTActionPlanFK == formPK)
                                                .ToList();

                    //Get the counts
                    //Active programs
                    numActivePrograms = context.Program.AsNoTracking()
                                                    .Where(p => p.StateFK == currentActionPlan.StateFK &&
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
                    mostRecentBOQ = context.BenchmarkOfQualitySLT.AsNoTracking()
                                                    .Where(boq => boq.StateFK == currentActionPlan.StateFK &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= currentActionPlan.ActionPlanEndDate)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();

                    //Make sure there is a most recent BOQ
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualitySLTPK > 0)
                    {
                        //Get the BOQ indicator information
                        mostRecentBOQIndicators = context.spGetBOQSLTIndicatorValues(mostRecentBOQ.BenchmarkOfQualitySLTPK).ToList();

                        //Get the indicators that need improvement
                        mostRecentBOQIndicatorsThatNeedImprovement = mostRecentBOQIndicators.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();
                    }

                    currentActionSteps = context.SLTActionPlanActionStep
                                                .Include(s => s.CodeBOQIndicator)
                                                .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                .Include(s => s.SLTActionPlanActionStepStatus)
                                                .Include(s => s.SLTActionPlanActionStepStatus.Select(ss => ss.CodeActionPlanActionStepStatus))
                                                .AsNoTracking()
                                                .Where(s => s.SLTActionPlanFK == formPK)
                                                .ToList();
                }
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblBIState.Text = currentActionPlan.State.Name;
            lblBIActionPlanStartDate.Text = currentActionPlan.ActionPlanStartDate.ToString("MM/dd/yyyy");
            lblBIActionPlanEndDate.Text = currentActionPlan.ActionPlanEndDate.ToString("MM/dd/yyyy");
            lblBIWorkGroup.Text = currentActionPlan.SLTWorkGroup.WorkGroupName;
            lblBIDateUpdated.Text = (currentActionPlan.EditDate.HasValue ? currentActionPlan.EditDate.Value.ToString("MM/dd/yyyy") : currentActionPlan.CreateDate.ToString("MM/dd/yyyy"));
            lblBICoordinator.Text = string.Format("({0}) {1} {2}", currentActionPlan.SLTMember.IDNumber, currentActionPlan.SLTMember.FirstName, currentActionPlan.SLTMember.LastName);
            lblBICoordinatorEmail.Text = currentActionPlan.SLTMember.EmailAddress;
            lblBIMissionStatement.Text = currentActionPlan.MissionStatement;
            lblBIAdditionalNotes.Text = currentActionPlan.AdditionalNotes;

            //------ Active Leadership Team Members ------
            //Set the detail report source and sorting
            LeadershipTeamDetailReport.DataSource = activeSLTMembers;
            LeadershipTeamDetail.SortFields.Add(new GroupField("IDNumber", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("FirstName", XRColumnSortOrder.Ascending));
            LeadershipTeamDetail.SortFields.Add(new GroupField("LastName", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblLTTeamMember.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Concat('(', [IDNumber], ') ', [FirstName], ' ', [LastName])"));
            lblLTStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblLTEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EmailAddress"));
            lblLTLeaveDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeaveDate"));
            lblLTState.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[State.Name]"));

            //------ All Meeting Dates ------
            //Set the detail report source and sorting
            MeetingDatesDetailReport.DataSource = currentMeetings;
            MeetingDatesDetail.SortFields.Add(new GroupField("MeetingDate", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblMDMeetingDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MeetingDate"));
            lblMDNotes.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "MeetingNotes"));

            //------ Ground Rules ------
            //Set the detail report source and sorting
            GroundRulesDetailReport.DataSource = currentGroundRules;
            GroundRulesDetail.SortFields.Add(new GroupField("GroundRuleNumber", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblGRGroundRuleDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroundRuleDescription"));
            lblGRGroundRuleNumber.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroundRuleNumber"));

            //------ State Information ------
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

            if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualitySLTPK > 0)
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
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.EMERGING_NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorNum.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.EMERGING_NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.EMERGING_NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
                StandardStyle.Name)));
            lblBIIndicatorStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "StyleName", string.Format("Iif([IndicatorValue] == {0}, '{1}', [IndicatorValue] == {2}, '{3}', '{4}')",
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.EMERGING_NEEDS_IMPROVEMENT, WarningStyle.Name,
                (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.NOT_IN_PLACE, DangerStyle.Name,
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
            ActionStepStatusDetailReport.DataMember = "SLTActionPlanActionStepStatus";
            ActionStepStatusDetail.SortFields.Add(new GroupField("StatusDate", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblASSStatus.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CodeActionPlanActionStepStatus.Description]"));
            lblASSStatusDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[StatusDate]"));
        }
    }
}