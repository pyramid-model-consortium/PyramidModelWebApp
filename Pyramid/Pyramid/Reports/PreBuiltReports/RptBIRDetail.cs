using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using Pyramid.Code;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBIRDetail : MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspBIRAllInfo_Result> allBIRInfo;
        List<rspEnrolledChildren_Result> allEnrolledChildren;
        List<DateTime> allYearMonthsInRange;
        int totalBIRs;
        int totalChildren;
        int totalChildrenWithBIRs;

        public RptBIRDetail()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptBIRDetail_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the parameters
            DateTime startDate = Convert.ToDateTime(Parameters["ParamStartDate"].Value);
            DateTime endDate = Convert.ToDateTime(Parameters["ParamEndDate"].Value);
            string programFKs = Convert.ToString(Parameters["ParamProgramFKs"].Value);
            string hubFKs = Convert.ToString(Parameters["ParamHubFKs"].Value);
            string cohortFKs = Convert.ToString(Parameters["ParamCohortFKs"].Value);
            string stateFKs = Convert.ToString(Parameters["ParamStateFKs"].Value);
            string childFKs = Convert.ToString(Parameters["ParamChildFKs"].Value);
            string classroomFKs = Convert.ToString(Parameters["ParamClassroomFKs"].Value);
            string raceFKs = Convert.ToString(Parameters["ParamRaceFKs"].Value);
            string ethnicityFKs = Convert.ToString(Parameters["ParamEthnicityFKs"].Value);
            string genderFKs = Convert.ToString(Parameters["ParamGenderFKs"].Value);
            string problemBehaviorFKs = Convert.ToString(Parameters["ParamProblemBehaviorFKs"].Value);
            string activityFKs = Convert.ToString(Parameters["ParamActivityFKs"].Value);
            string othersInvolvedFKs = Convert.ToString(Parameters["ParamOthersInvolvedFKs"].Value);
            string possibleMotivationFKs = Convert.ToString(Parameters["ParamPossibleMotivationFKs"].Value);
            string strategyResponseFKs = Convert.ToString(Parameters["ParamStrategyResponseFKs"].Value);
            string adminFollowUpFKs = Convert.ToString(Parameters["ParamAdminFollowUpFKs"].Value);
            bool? hasIEP = (Parameters["ParamIEP"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamIEP"].Value));
            bool? isDLL = (Parameters["ParamDLL"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamDLL"].Value));

            //Create the year month list
            allYearMonthsInRange = new List<DateTime>();
            DateTime iterator = startDate;
            while (iterator <= endDate)
            {
                //Get the month and year and add it to the list
                allYearMonthsInRange.Add(new DateTime(iterator.Year, iterator.Month, 1));

                //Increment
                iterator = iterator.AddMonths(1);
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the BIR info for the timeframe
                allBIRInfo = context.rspBIRAllInfo(startDate, endDate, childFKs,
                                                    classroomFKs, raceFKs, ethnicityFKs,
                                                    genderFKs, problemBehaviorFKs, activityFKs,
                                                    othersInvolvedFKs, possibleMotivationFKs, strategyResponseFKs,
                                                    adminFollowUpFKs, hasIEP, isDLL, programFKs, hubFKs, cohortFKs, stateFKs).ToList();

                //Get all the enrolled children for the timeframe
                allEnrolledChildren = context.rspEnrolledChildren(startDate, endDate, childFKs,
                                                    classroomFKs, raceFKs, ethnicityFKs,
                                                    genderFKs, hasIEP, isDLL, programFKs, hubFKs, cohortFKs, stateFKs).ToList();
            }

            //Get the totals
            totalBIRs = allBIRInfo.Count;
            totalChildren = allEnrolledChildren.Count;
            totalChildrenWithBIRs = allBIRInfo.Select(abi => abi.ChildFK).Distinct().Count();

            //Set the total labels
            lblTotalBIRs.Text = totalBIRs.ToString();
            lblTotalChildren.Text = totalChildren.ToString();
            lblTotalChildrenWithBIRs.Text = totalChildrenWithBIRs.ToString();

            //Fill the charts and tables
            FillProblemBehaviorChartsAndTables();
            FillActivityChartsAndTables();
            FillOthersInvolvedChartsAndTables();
            FillPossibleMotivationChartsAndTables();
            FillStrategyResponseChartsAndTables();
            FillAdminFollowUpChartsAndTables();
        }

        #region Problem Behavior

        /// <summary>
        /// This method fills the problem behavior charts and tables
        /// </summary>
        private void FillProblemBehaviorChartsAndTables()
        {
            //To hold all the problem behaviors
            List<Utilities.CodeTableInfo> allProblemBehaviors;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the problem behaviors
                allProblemBehaviors = context.CodeProblemBehavior.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeProblemBehaviorPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the problem behavior sub report
            SubReports.SubRptBIRDetailItemSection problemBehaviorReport = new SubReports.SubRptBIRDetailItemSection();
            problemBehaviorReport.FillReport(allYearMonthsInRange, allBIRInfo, allProblemBehaviors, 
                                                "ProblemBehaviorCodeFK", "Problem Behavior",
                                                Utilities.DevExChartColors.Orange2, Utilities.DevExChartColors.Blue2);
            subRptProblemBehavior.ReportSource = problemBehaviorReport;
        }

        #endregion

        #region Activities

        /// <summary>
        /// This method fills the activity charts and tables
        /// </summary>
        private void FillActivityChartsAndTables()
        {
            //To hold all the activities
            List<Utilities.CodeTableInfo> allActivities;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the activities
                allActivities = context.CodeActivity.AsNoTracking()
                                            .OrderBy(ca => ca.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeActivityPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the activity sub report
            SubReports.SubRptBIRDetailItemSection activityReport = new SubReports.SubRptBIRDetailItemSection();
            activityReport.FillReport(allYearMonthsInRange, allBIRInfo, allActivities, 
                                        "ActivityCodeFK", "Activity",
                                        Utilities.DevExChartColors.Orange1, Utilities.DevExChartColors.Blue1);
            subRptActivity.ReportSource = activityReport;
        }

        #endregion

        #region Others Involved

        /// <summary>
        /// This method fills the others involved charts and tables
        /// </summary>
        private void FillOthersInvolvedChartsAndTables()
        {
            //To hold all the others involved
            List<Utilities.CodeTableInfo> allOthersInvolved;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the others involved
                allOthersInvolved = context.CodeOthersInvolved.AsNoTracking()
                                            .OrderBy(coi => coi.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeOthersInvolvedPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the others involved sub report
            SubReports.SubRptBIRDetailItemSection othersInvolvedReport = new SubReports.SubRptBIRDetailItemSection();
            othersInvolvedReport.FillReport(allYearMonthsInRange, allBIRInfo, allOthersInvolved, 
                                                "OthersInvolvedCodeFK", "Others Involved",
                                                Utilities.DevExChartColors.Green1, Utilities.DevExChartColors.Red1);
            subRptOthersInvolved.ReportSource = othersInvolvedReport;
        }

        #endregion

        #region Possible Motivation

        /// <summary>
        /// This method fills the possible motivation charts and tables
        /// </summary>
        private void FillPossibleMotivationChartsAndTables()
        {
            //To hold all the possible motivations
            List<Utilities.CodeTableInfo> allPossibleMotivations;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the possible motivations
                allPossibleMotivations = context.CodePossibleMotivation.AsNoTracking()
                                            .OrderBy(cpm => cpm.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodePossibleMotivationPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the possible motivation sub report
            SubReports.SubRptBIRDetailItemSection possibleMotivationReport = new SubReports.SubRptBIRDetailItemSection();
            possibleMotivationReport.FillReport(allYearMonthsInRange, allBIRInfo, allPossibleMotivations, 
                                                    "PossibleMotivationCodeFK", "Possible Motivation",
                                                    Utilities.DevExChartColors.Blue4, Utilities.DevExChartColors.Orange2);
            subRptPossibleMotivation.ReportSource = possibleMotivationReport;
        }

        #endregion

        #region Strategy Response

        /// <summary>
        /// This method fills the strategy response charts and tables
        /// </summary>
        private void FillStrategyResponseChartsAndTables()
        {
            //To hold all the strategy responses
            List<Utilities.CodeTableInfo> allStrategyResponses;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the strategy responses
                allStrategyResponses = context.CodeStrategyResponse.AsNoTracking()
                                            .OrderBy(csa => csa.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeStrategyResponsePK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the strategy response sub report
            SubReports.SubRptBIRDetailItemSection strategyResponseReport = new SubReports.SubRptBIRDetailItemSection();
            strategyResponseReport.FillReport(allYearMonthsInRange, allBIRInfo, allStrategyResponses, 
                                                "StrategyResponseCodeFK", "Strategy Response",
                                                Utilities.DevExChartColors.Purple1, Utilities.DevExChartColors.Yellow1);
            subRptStrategyResponse.ReportSource = strategyResponseReport;
        }

        #endregion

        #region Admin Follow Up

        /// <summary>
        /// This method fills the admin follow up charts and tables
        /// </summary>
        private void FillAdminFollowUpChartsAndTables()
        {
            //To hold all the admin follow ups
            List<Utilities.CodeTableInfo> allAdminFollowUps;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the admin follow ups
                allAdminFollowUps = context.CodeAdminFollowUp.AsNoTracking()
                                            .OrderBy(cafu => cafu.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeAdminFollowUpPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Fill the activity sub report
            SubReports.SubRptBIRDetailItemSection adminFollowUpReport = new SubReports.SubRptBIRDetailItemSection();
            adminFollowUpReport.FillReport(allYearMonthsInRange, allBIRInfo, allAdminFollowUps, 
                                            "AdminFollowUpCodeFK", "Admin Follow-up",
                                            Utilities.DevExChartColors.Red2, Utilities.DevExChartColors.Green2);
            subRptAdminFollowUp.ReportSource = adminFollowUpReport;
        }

        #endregion
    }
}
