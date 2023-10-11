using System;
using System.Data;
using System.Drawing;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBIRSummary : Pyramid.Reports.PreBuiltReports.MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspBIRAllInfo_Result> allBIRInfo;
        List<rspEnrolledChildren_Result> allEnrolledChildren;
        List<DateTime> allYearMonthsInRange;
        List<int> allHoursInDay;
        int totalBIRs;
        int totalChildren;
        int totalChildrenWithBIRs;

        public RptBIRSummary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptBIRSummary_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
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

            //Create the hour list
            allHoursInDay = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                allHoursInDay.Add(i);
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
            FillMonthlyChartsAndTables();
            FillChildrenWithBIRsChartsAndTables();
        }

        #region Monthly Information

        /// <summary>
        /// This method fills the summary chart with data
        /// </summary>
        private void FillMonthlyChartsAndTables()
        {
            //Get the info for the monthly charts grouped by year-month
            var groupedMonthlyInfo = allBIRInfo.GroupBy(abi => new { abi.IncidentDatetime.Month, abi.IncidentDatetime.Year })
                                                .Select(g => new {
                                                    IncidentMonth = g.Key.Month,
                                                    IncidentYear = g.Key.Year,
                                                    YearMonthDateTime = new DateTime(g.Key.Year, g.Key.Month, 1),
                                                    ChildrenWithBIRs = g.Select(abi => abi.ChildFK).Distinct().Count(),
                                                    TotalIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                    AverageIncidentsByChild = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / g.Select(abi => abi.ChildFK).Distinct().Count(),
                                                    AverageIncidentsByMonth = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / DateTime.DaysInMonth(g.Key.Year, g.Key.Month),
                                                    PercentOfTotalBIRs = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs,
                                                    PercentOfChildrenWithBIRs = (double)g.Select(abi => abi.ChildFK).Distinct().Count() / allEnrolledChildren.Where(aec => aec.EnrollmentDate <= new DateTime(g.Key.Year, g.Key.Month, 1).AddMonths(1).AddSeconds(-1) && (aec.DischargeDate.HasValue == false || aec.DischargeDate > new DateTime(g.Key.Year, g.Key.Month, 1))).Count()
                                                }).ToList();

            //Get all the monthly info, even if no incidents happened in a month
            var finalMonthlyInfo = allYearMonthsInRange.GroupJoin(groupedMonthlyInfo, ym => ym,
                                                                    gmi => gmi.YearMonthDateTime,
                                                                    (ym, abi) => new
                                                                    {
                                                                        YearMonthDateTime = ym,
                                                                        ChildrenWithBIRs = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().ChildrenWithBIRs),
                                                                        TotalIncidents = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().TotalIncidents),
                                                                        AverageIncidentsByChild = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().AverageIncidentsByChild),
                                                                        AverageIncidentsByMonth = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().AverageIncidentsByMonth),
                                                                        PercentOfTotalBIRs = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().PercentOfTotalBIRs),
                                                                        PercentOfChildrenWithBIRs = (abi.FirstOrDefault() == null ? 0 : abi.FirstOrDefault().PercentOfChildrenWithBIRs),
                                                                    }).ToList();

            //--------------------- Summary Chart Start -----------------------
            //Set the chart data source
            NumBIRsChart.DataSource = finalMonthlyInfo;
            NumChildrenWithBIRChart.DataSource = finalMonthlyInfo;

            //Set the # of BIR chart series
            NumBIRsChart.Series[0].View.Color = Utilities.DevExChartColors.Blue1;
            NumBIRsChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            NumBIRsChart.Series[0].ValueScaleType = ScaleType.Numerical;
            NumBIRsChart.Series[0].ValueDataMembers.AddRange(new string[] { "TotalIncidents" });

            //Set the # of Children with a BIR chart series
            NumChildrenWithBIRChart.Series[0].View.Color = Utilities.DevExChartColors.Blue1;
            NumChildrenWithBIRChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            NumChildrenWithBIRChart.Series[0].ValueScaleType = ScaleType.Numerical;
            NumChildrenWithBIRChart.Series[0].ValueDataMembers.AddRange(new string[] { "ChildrenWithBIRs" });

            //--------------------- Summary Chart End -----------------------

            //--------------------- Average Incident Chart Start -----------------------
            //Set the chart data source
            AverageNumBIRsChart.DataSource = finalMonthlyInfo;
            AverageNumChildrenWithBIRsChart.DataSource = finalMonthlyInfo;

            //Set the chart series
            AverageNumBIRsChart.Series[0].View.Color = Utilities.DevExChartColors.Red1;
            AverageNumBIRsChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            AverageNumBIRsChart.Series[0].ValueScaleType = ScaleType.Numerical;
            AverageNumBIRsChart.Series[0].ValueDataMembers.AddRange(new string[] { "AverageIncidentsByMonth" });

            //Set the Average # of BIRs per Child with a BIR chart series
            AverageNumChildrenWithBIRsChart.Series[0].View.Color = Utilities.DevExChartColors.Red1;
            AverageNumChildrenWithBIRsChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            AverageNumChildrenWithBIRsChart.Series[0].ValueScaleType = ScaleType.Numerical;
            AverageNumChildrenWithBIRsChart.Series[0].ValueDataMembers.AddRange(new string[] { "AverageIncidentsByChild" });

            //--------------------- Average Incident Chart End -----------------------

            //--------------------- Percentages Chart Start -----------------------
            //Set the chart data source
            TotalBIRsPercentageChart.DataSource = finalMonthlyInfo;
            ChildrenWithBIRsPercentageChart.DataSource = finalMonthlyInfo;

            //Set the % of Total BIRs chart series
            TotalBIRsPercentageChart.Series[0].View.Color = Utilities.DevExChartColors.Purple1;
            TotalBIRsPercentageChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            TotalBIRsPercentageChart.Series[0].ValueScaleType = ScaleType.Numerical;
            TotalBIRsPercentageChart.Series[0].ValueDataMembers.AddRange(new string[] { "PercentOfTotalBIRs" });

            //Set the % of Children with a BIR chart series
            ChildrenWithBIRsPercentageChart.Series[0].View.Color = Utilities.DevExChartColors.Purple1;
            ChildrenWithBIRsPercentageChart.Series[0].ArgumentDataMember = "YearMonthDateTime";
            ChildrenWithBIRsPercentageChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ChildrenWithBIRsPercentageChart.Series[0].ValueDataMembers.AddRange(new string[] { "PercentOfChildrenWithBIRs" });

            //--------------------- Percentages Chart End -----------------------

            //--------------------- Monthly Info Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.MonthlyDetailReport.DataSource = finalMonthlyInfo;
            this.MonthlyDetail.SortFields.Add(new GroupField("YearMonthDateTime", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblMonthlyYearMonth.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "YearMonthDateTime"));
            lblMonthlyNumBIRs.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "TotalIncidents"));
            lblMonthlyPercentTotalBIRs.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotalBIRs"));
            lblMonthlyNumChildrenWithBIRs.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildrenWithBIRs"));
            lblMonthlyAverageNumBIRsByChild.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "AverageIncidentsByChild"));
            lblMonthlyPercentChildrenWithBIR.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfChildrenWithBIRs"));
            lblMonthlyAverageNumIncidents.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "AverageIncidentsByMonth"));

            //Set the group footer label expressions
            lblMonthlyNumBIRsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([TotalIncidents])"));
            lblMonthlyNumBIRsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblMonthlyPercentTotalBIRsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfTotalBIRs])"));
            lblMonthlyPercentTotalBIRsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblMonthlyNumChildrenWithBIRsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([ChildrenWithBIRs])"));
            lblMonthlyNumChildrenWithBIRsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblMonthlyAverageNumBIRsByChildAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([AverageIncidentsByChild])"));
            lblMonthlyAverageNumBIRsByChildAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblMonthlyPercentChildrenWithBIRAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfChildrenWithBIRs])"));
            lblMonthlyPercentChildrenWithBIRAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblMonthlyAverageNumIncidentsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([AverageIncidentsByMonth])"));
            lblMonthlyAverageNumIncidentsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Monthly Info Table End -----------------------
        }

        #endregion

        #region Total Children with BIRs

        /// <summary>
        /// This method fills the children with BIRs charts and tables
        /// </summary>
        private void FillChildrenWithBIRsChartsAndTables()
        {
            //Create a list of BIR groups
            var incidentGroups = new[] {
                new { GroupID = 0, GroupName = "0", StartRange = 0, EndRange = 1 },
                new { GroupID = 1, GroupName = "1", StartRange = 1, EndRange = 2 },
                new { GroupID = 2, GroupName = "2-5", StartRange = 2, EndRange = 6 },
                new { GroupID = 3, GroupName = "6-10", StartRange = 6, EndRange = 11 },
                new { GroupID = 4, GroupName = "11-15", StartRange = 11, EndRange = 16 },
                new { GroupID = 5, GroupName = "16-20", StartRange = 16, EndRange = 21 },
                new { GroupID = 6, GroupName = "21+", StartRange = 21, EndRange = int.MaxValue }
            }.ToList();

            //Group the BIRs by child and get the number of incidents
            var groupedBIRsByChild = allBIRInfo.GroupBy(abi => abi.ChildFK)
                                                .Select(g => new
                                                {
                                                    ChildFK = g.Key,
                                                    NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count()
                                                }).ToList();

            //Left join all enrolled children on the grouped BIRs, 
            //then group by group ID and get the number of children in the group
            var groupedBIRsByChildIncidents = allEnrolledChildren.GroupJoin(groupedBIRsByChild, aec => aec.ChildPK,
                                                                    gbc => gbc.ChildFK,
                                                                    (aec, gbc) => new
                                                                    {
                                                                        aec.ChildPK,
                                                                        NumIncidents = (gbc.FirstOrDefault() == null ? 0 : gbc.FirstOrDefault().NumIncidents)
                                                                    })
                                                                    .Select(g => new
                                                                    {
                                                                        g.ChildPK,
                                                                        g.NumIncidents,
                                                                        IncidentGroup = incidentGroups.Where(ig => g.NumIncidents >= ig.StartRange &&
                                                                                                            g.NumIncidents < ig.EndRange)
                                                                                                .FirstOrDefault()
                                                                    })
                                                                    .GroupBy(g => g.IncidentGroup.GroupID)
                                                                    .Select(g => new
                                                                    {
                                                                        GroupID = g.Key,
                                                                        NumChildren = g.Select(c => c.ChildPK).Count()
                                                                    })
                                                                    .ToList();

            //Left join the BIR groups on the previous LINQ query
            var groupedBIRsByNumGroup = incidentGroups.GroupJoin(groupedBIRsByChildIncidents,
                                                                ig => ig.GroupID,
                                                                gbc => gbc.GroupID,
                                                                (ig, gbc) => new
                                                                {
                                                                    ig.GroupID,
                                                                    ig.GroupName,
                                                                    NumChildren = gbc.FirstOrDefault() == null ? 0 : gbc.FirstOrDefault().NumChildren,
                                                                    PercentChildren = (double)(gbc.FirstOrDefault() == null ? 0 : gbc.FirstOrDefault().NumChildren) / totalChildren
                                                                })
                                                        .ToList();

            //--------------------- Total Number of Children by BIR Count Chart Start -----------------------

            //Set the chart data source
            ChildrenWithBIRNumberChart.DataSource = groupedBIRsByNumGroup;

            //Set the Total Number of Children chart series
            ChildrenWithBIRNumberChart.Series[0].View.Color = Utilities.DevExChartColors.Green1;
            ChildrenWithBIRNumberChart.Series[0].ArgumentScaleType = ScaleType.Auto;
            ChildrenWithBIRNumberChart.Series[0].ArgumentDataMember = "GroupName";
            ChildrenWithBIRNumberChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ChildrenWithBIRNumberChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumChildren" });

            //--------------------- Total Number of Children by BIR Count Chart End -----------------------

            //--------------------- Percent of all Enrolled Children by BIR Count Chart Start -----------------------

            //Set the chart data source
            ChildrenWithBIRsPercentChart.DataSource = groupedBIRsByNumGroup;

            //Set the Percent of Enrolled Children chart series
            ChildrenWithBIRsPercentChart.Series[0].View.Color = Utilities.DevExChartColors.Green1;
            ChildrenWithBIRsPercentChart.Series[0].ArgumentScaleType = ScaleType.Auto;
            ChildrenWithBIRsPercentChart.Series[0].ArgumentDataMember = "GroupName";
            ChildrenWithBIRsPercentChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ChildrenWithBIRsPercentChart.Series[0].ValueDataMembers.AddRange(new string[] { "PercentChildren" });

            //--------------------- Percent of all Enrolled Children by BIR Count Chart End -----------------------


            //--------------------- Children by BIR Count Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.ChildrenWithBIRsDetailReport.DataSource = groupedBIRsByNumGroup;

            //Set the detail band label expressions
            lblChildrenWithBIRsBIRGroup.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroupName"));
            lblChildrenWithBIRsNumChildren.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumChildren"));
            lblChildrenWithBIRsPercentChildren.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentChildren"));

            //Set the group footer label expressions
            lblChildrenWithBIRsNumChildrenAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([NumChildren])"));
            lblChildrenWithBIRsNumChildrenAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblChildrenWithBIRsPercentChildrenAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentChildren])"));
            lblChildrenWithBIRsPercentChildrenAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Children by BIR Count Table End -----------------------
        }

        #endregion
    }
}
