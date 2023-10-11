using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;
using DevExpress.XtraReports.UI.CrossTab;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBIROverallSummary : Pyramid.Reports.PreBuiltReports.MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspBIRAllInfo_Result> allBIRInfo;
        List<rspEnrolledChildren_Result> allEnrolledChildren;
        List<DateTime> allYearMonthsInRange;
        List<int> allHoursInDay;
        List<int> intProgramFKs, intHubFKs, intCohortFKs, intStateFKs, intClassroomFKs;
        int totalBIRs;
        int totalChildren;
        int totalChildrenWithBIRs;

        public RptBIROverallSummary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptBIROverallSummary_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the parameters
            DateTime startDate = Convert.ToDateTime(Parameters["ParamStartDate"].Value);
            DateTime endDate = Convert.ToDateTime(Parameters["ParamEndDate"].Value);
            string reportFocus = Convert.ToString(Parameters["ParamReportFocus"].Value);
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
            intProgramFKs = (string.IsNullOrWhiteSpace(programFKs) ? new List<int>() : programFKs.Split(',').Select(int.Parse).ToList());
            intHubFKs = (string.IsNullOrWhiteSpace(hubFKs) ? new List<int>() : hubFKs.Split(',').Select(int.Parse).ToList());
            intCohortFKs = (string.IsNullOrWhiteSpace(cohortFKs) ? new List<int>() : cohortFKs.Split(',').Select(int.Parse).ToList());
            intStateFKs = (string.IsNullOrWhiteSpace(stateFKs) ? new List<int>() : stateFKs.Split(',').Select(int.Parse).ToList());
            intClassroomFKs = (string.IsNullOrWhiteSpace(classroomFKs) ? new List<int>() : classroomFKs.Split(',').Select(int.Parse).ToList());

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
            FillDayOfWeekChartsAndTables();
            FillIncidentsByDayChartsAndTables();
            FillIncidentsByHourChartsAndTables();
            FillProblemBehaviorSubReport();
            FillActivitySubReport();
            FillOthersInvolvedSubReport();
            FillPossibleMotivationSubReport();
            FillStrategyResponseSubReport();
            FillAdminFollowUpSubReport();

            switch (reportFocus.ToUpper())
            {
                case "CHI":
                    //Fill and display ONLY the child section
                    FillBIRsByChildChartsAndTables();
                    FillChildInformationTable();
                    ClassroomBIRsDetailReport.Visible = false;
                    ChildBIRsDetailReport.Visible = true;
                    ChildInformationDetailReport.Visible = true;
                    break;
                case "CR":
                    //Fill and display ONLY the classroom section
                    FillBIRsByClassroomChartsAndTables();
                    ClassroomBIRsDetailReport.Visible = true;
                    ChildBIRsDetailReport.Visible = false;
                    ChildInformationDetailReport.Visible = false;
                    break;
                default:
                    //Fill and display both sections
                    FillBIRsByChildChartsAndTables();
                    FillChildInformationTable();
                    FillBIRsByClassroomChartsAndTables();
                    ClassroomBIRsDetailReport.Visible = true;
                    ChildBIRsDetailReport.Visible = true;
                    ChildInformationDetailReport.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// This method fires before the chart is displayed and it sets the axis values
        /// so that they display correctly
        /// </summary>
        /// <param name="sender">The XRChart object</param>
        /// <param name="e"></param>
        private void DynamicSeriesChart_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the chart
            XRChart chart = (XRChart)sender;

            //Get the XY axis
            XYDiagram diagram = (XYDiagram)chart.Diagram;

            //Make sure spacing is correct
            diagram.AxisX.QualitativeScaleOptions.AutoGrid = false;
            diagram.AxisX.QualitativeScaleOptions.GridSpacing = 1;

            //Don't hide overlapping labels 
            diagram.AxisX.Label.ResolveOverlappingOptions.AllowHide = false;
        }

        /// <summary>
        /// This method formats the series names for charts that use months for
        /// dynamically-created series
        /// </summary>
        /// <param name="sender">The XRChart object</param>
        /// <param name="e"></param>
        private void MonthlySeriesChart_BoundDataChanged(object sender, EventArgs e)
        {
            //Get the chart
            XRChart chart = (XRChart)sender;

            //Loop through the series
            foreach (Series series in chart.Series)
            {
                //To hold the DateTime version of the series name
                DateTime date;

                //Get the series name as a DateTime
                if (DateTime.TryParse(series.Name, out date))
                {
                    //Format the series name
                    series.Name = date.ToString("MMM-yyyy");
                }
            }
        }

        /// <summary>
        /// This method configures the passed cross tab by formatting
        /// and setting expression bindings.
        /// </summary>
        /// <param name="crossTab">The XRCrossTab to configure</param>
        /// <param name="columnFormatString">The format string for the columns</param>
        /// <param name="totalExpression">The expression for the total cells</param>
        private void ConfigureMonthlyCrossTab(XRCrossTab crossTab, string columnFormatString, string totalExpression)
        {
            //Adjust generated cells
            foreach (var c in crossTab.ColumnDefinitions)
            {
                //Enable auto-width for all columns
                c.AutoWidthMode = AutoSizeMode.ShrinkAndGrow;
            }

            //Set cell text for static cells
            crossTab.Cells[0, 0].Text = "";
            crossTab.Cells[0, 2].Text = "Total";
            crossTab.Cells[2, 0].Text = "Total";

            //Set the format string for the column headers
            crossTab.Cells[1, 0].TextFormatString = columnFormatString;

            //Set the expression bindings for the totals
            crossTab.Cells[2, 1].ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", totalExpression));
            crossTab.Cells[1, 2].ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", totalExpression));
            crossTab.Cells[2, 2].ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", totalExpression));

            //Set the style for the header cells
            XRControlStyle headerStyle = StyleSheet["CrossTabHeaderStyle"];
            crossTab.CrossTabStyles.HeaderAreaStyle = headerStyle;

            //Set the style for the total cells
            XRControlStyle totalStyle = StyleSheet["CrossTabTotalStyle"];
            crossTab.CrossTabStyles.TotalAreaStyle = totalStyle;
        }


        #region Day of Week

        /// <summary>
        /// This method fills the incidents by day of week charts and tables with data
        /// </summary>
        private void FillDayOfWeekChartsAndTables()
        {
            //Get the BIRs grouped by incidents by day of week
            var dayOfWeekInfo = allBIRInfo.OrderBy(abi => abi.IncidentDatetime.ToString("ddd"))
                                                .GroupBy(abi => abi.IncidentDatetime.ToString("ddd"))
                                                .Select(g => new
                                                {
                                                    DayOfWeek = g.Key,
                                                    DayOfWeekNum = (int)g.Select(abi => abi.IncidentDatetime.DayOfWeek).FirstOrDefault(),
                                                    NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                    PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                }).ToList();

            //--------------------- Total Number of Incidents by Day of Week Chart Start -----------------------

            //Set the chart data source
            DayOfWeekChart.DataSource = dayOfWeekInfo;

            //Set the Total # of Incidents chart series
            DayOfWeekChart.Series[0].ToolTipHintDataMember = "DayOfWeek";
            DayOfWeekChart.Series[0].ArgumentDataMember = "DayOfWeekNum";
            DayOfWeekChart.Series[0].ValueScaleType = ScaleType.Numerical;
            DayOfWeekChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //--------------------- Total Number of Incidents by Day of Week Chart End -----------------------

            //--------------------- Day of Week Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.DayOfWeekDetailReport.DataSource = dayOfWeekInfo;
            this.DayOfWeekDetail.SortFields.Add(new GroupField("DayOfWeekNum", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblDayOfWeek.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "DayOfWeek"));
            lblDayOfWeekNumIncidents.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumIncidents"));
            lblDayOfWeekPercent.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotal"));

            //Set the group footer label expressions
            lblDayOfWeekNumIncidentsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([NumIncidents])"));
            lblDayOfWeekNumIncidentsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblDayOfWeekPercentAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfTotal])"));
            lblDayOfWeekPercentAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Day of Week Table End -----------------------
        }

        #endregion

        #region Incidents by Day

        /// <summary>
        /// This method fills the incidents by day charts and tables with data
        /// </summary>
        private void FillIncidentsByDayChartsAndTables()
        {
            //Get the BIRs grouped by incidents by day
            var incidentsByDayInfo = allBIRInfo.OrderBy(abi => abi.IncidentDatetime)
                                                .GroupBy(abi => new DateTime(abi.IncidentDatetime.Year, abi.IncidentDatetime.Month, abi.IncidentDatetime.Day, 1, 1, 1))
                                                .Select(g => new
                                                {
                                                    IncidentDatetime = g.Key,
                                                    NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                    PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                }).ToList();

            //--------------------- Total Number of Incidents by Day Chart Start -----------------------

            //Set the chart data source
            IncidentsByDayChart.DataSource = incidentsByDayInfo;

            //Set the Total # of Incidents chart series
            IncidentsByDayChart.Series[0].View.Color = Utilities.DevExChartColors.Green2;
            IncidentsByDayChart.Series[0].ArgumentScaleType = ScaleType.DateTime;
            IncidentsByDayChart.Series[0].ArgumentDataMember = "IncidentDatetime";
            IncidentsByDayChart.Series[0].ValueScaleType = ScaleType.Numerical;
            IncidentsByDayChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //--------------------- Total Number of Incidents by Day Chart End -----------------------

            //--------------------- Incidents by Day Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.IncidentsByDayDetailReport.DataSource = incidentsByDayInfo;
            this.IncidentsByDayDetail.SortFields.Add(new GroupField("IncidentDatetime", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblDay.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IncidentDatetime"));
            lblIncidentsByDayNumIncidents.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumIncidents"));
            lblIncidentsByDayPercent.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotal"));

            //Set the group footer label expressions
            lblIncidentsByDayNumIncidentsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([NumIncidents])"));
            lblIncidentsByDayNumIncidentsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblIncidentsByDayPercentAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfTotal])"));
            lblIncidentsByDayPercentAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Incidents by Day Table End -----------------------
        }

        #endregion

        #region Incidents by Hour

        /// <summary>
        /// This method fills the incidents by hour charts and tables with data
        /// </summary>
        private void FillIncidentsByHourChartsAndTables()
        {
            //Get the BIRs grouped by incidents by hour
            var incidentsByHourInfo = allBIRInfo.GroupBy(abi => abi.IncidentDatetime.Hour)
                                                        .Select(g => new
                                                        {
                                                            IncidentHour = g.Key,
                                                            NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                            PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                        }).ToList();

            //Left join the hours in a day on the grouped list of incidents by hour
            var finalIncidentsByHourInfo = allHoursInDay.GroupJoin(incidentsByHourInfo,
                                                        ahid => ahid,
                                                        ibti => ibti.IncidentHour,
                                                        (ahid, ibti) => new
                                                        {
                                                            IncidentHour = new DateTime(2020, 1, 1, ahid, 1, 1),
                                                            NumIncidents = (ibti.FirstOrDefault() == null ? 0 : ibti.First().NumIncidents),
                                                            PercentOfTotal = (ibti.FirstOrDefault() == null ? 0 : ibti.First().PercentOfTotal)
                                                        }).ToList();

            //--------------------- Total Number of Incidents by Hour Chart Start -----------------------

            //Set the chart data source
            IncidentsByHourChart.DataSource = finalIncidentsByHourInfo;

            //Set the Total # of Incidents chart series
            IncidentsByHourChart.Series[0].View.Color = Utilities.DevExChartColors.Green2;
            IncidentsByHourChart.Series[0].ArgumentScaleType = ScaleType.DateTime;
            IncidentsByHourChart.Series[0].ArgumentDataMember = "IncidentHour";
            IncidentsByHourChart.Series[0].ValueScaleType = ScaleType.Numerical;
            IncidentsByHourChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //--------------------- Total Number of Incidents by Hour Chart End -----------------------

            //--------------------- Incidents by Hour Table Start -----------------------

            //Set the data source and sort for the detail of the report
            IncidentsByHourDetailReport.DataSource = finalIncidentsByHourInfo;
            IncidentsByHourDetail.SortFields.Add(new GroupField("IncidentHour", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblHour.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "IncidentHour"));
            lblIncidentsByHourNumIncidents.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumIncidents"));
            lblIncidentsByHourPercent.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotal"));

            //Set the group footer label expressions
            lblIncidentsByHourNumIncidentsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([NumIncidents])"));
            lblIncidentsByHourNumIncidentsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblIncidentsByHourPercentAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfTotal])"));
            lblIncidentsByHourPercentAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Incidents by Hour Table End -----------------------
        }

        #endregion

        #region Problem Behaviors

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillProblemBehaviorSubReport()
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

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal problemBehaviorReport = new SubReports.SubRptBIRItemTotal();
            problemBehaviorReport.FillReport(allBIRInfo, allProblemBehaviors,
                                                "ProblemBehaviorCodeFK", "Problem Behavior",
                                                Utilities.DevExChartColors.Orange2);

            //Set the sub report source to the new report object
            subRptProblemBehavior.ReportSource = problemBehaviorReport;
        }

        #endregion

        #region Activities

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillActivitySubReport()
        {
            //To hold all the activities
            List<Utilities.CodeTableInfo> allActivities;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the activities
                allActivities = context.CodeActivity.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeActivityPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal activityReport = new SubReports.SubRptBIRItemTotal();
            activityReport.FillReport(allBIRInfo, allActivities,
                                                "ActivityCodeFK", "Activity",
                                                Utilities.DevExChartColors.Orange1);

            //Set the sub report source to the new report object
            subRptActivity.ReportSource = activityReport;
        }

        #endregion

        #region Others Involved

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillOthersInvolvedSubReport()
        {
            //To hold all the others involved
            List<Utilities.CodeTableInfo> allOthersInvolved;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the others involved
                allOthersInvolved = context.CodeOthersInvolved.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeOthersInvolvedPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal othersInvolvedReport = new SubReports.SubRptBIRItemTotal();
            othersInvolvedReport.FillReport(allBIRInfo, allOthersInvolved,
                                                "OthersInvolvedCodeFK", "Others Involved",
                                                Utilities.DevExChartColors.Green1);

            //Set the sub report source to the new report object
            subRptOthersInvolved.ReportSource = othersInvolvedReport;
        }

        #endregion

        #region Possible Motivations

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillPossibleMotivationSubReport()
        {
            //To hold all the possible motivations
            List<Utilities.CodeTableInfo> allPossibleMotivations;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the possible motivations
                allPossibleMotivations = context.CodePossibleMotivation.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodePossibleMotivationPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal possibleMotivationReport = new SubReports.SubRptBIRItemTotal();
            possibleMotivationReport.FillReport(allBIRInfo, allPossibleMotivations,
                                                "PossibleMotivationCodeFK", "Possible Motivation",
                                                Utilities.DevExChartColors.Blue4);

            //Set the sub report source to the new report object
            subRptPossibleMotivation.ReportSource = possibleMotivationReport;
        }

        #endregion

        #region Strategy Responses

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillStrategyResponseSubReport()
        {
            //To hold all the strategy responses
            List<Utilities.CodeTableInfo> allStrategyResponses;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the strategy responses
                allStrategyResponses = context.CodeStrategyResponse.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeStrategyResponsePK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal strategyResponseReport = new SubReports.SubRptBIRItemTotal();
            strategyResponseReport.FillReport(allBIRInfo, allStrategyResponses,
                                                "StrategyResponseCodeFK", "Strategy Response",
                                                Utilities.DevExChartColors.Purple1);

            //Set the sub report source to the new report object
            subRptStrategyResponse.ReportSource = strategyResponseReport;
        }

        #endregion

        #region Admin Follow-up

        /// <summary>
        /// This method fills the sub report by creating a new instance of
        /// the report object and calling a fill method.
        /// </summary>
        private void FillAdminFollowUpSubReport()
        {
            //To hold all the admin follow-ups
            List<Utilities.CodeTableInfo> allAdminFollowUps;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the admin follow-ups
                allAdminFollowUps = context.CodeAdminFollowUp.AsNoTracking()
                                            .OrderBy(cpb => cpb.Abbreviation)
                                            .Select(ca => new Utilities.CodeTableInfo()
                                            {
                                                CodeTablePK = ca.CodeAdminFollowUpPK,
                                                ItemAbbreviation = ca.Abbreviation,
                                                ItemDescription = ca.Description,
                                                OrderBy = ca.OrderBy
                                            })
                                            .ToList();
            }

            //Get the report object and fill it
            SubReports.SubRptBIRItemTotal adminFollowUpReport = new SubReports.SubRptBIRItemTotal();
            adminFollowUpReport.FillReport(allBIRInfo, allAdminFollowUps,
                                                "AdminFollowUpCodeFK", "Admin Follow-up",
                                                Utilities.DevExChartColors.Red2);

            //Set the sub report source to the new report object
            subRptAdminFollowUp.ReportSource = adminFollowUpReport;
        }

        #endregion

        #region Monthly BIRs By Child ID

        /// <summary>
        /// This method fills the monthly BIRs by child ID charts and tables
        /// </summary>
        private void FillBIRsByChildChartsAndTables()
        {
            //Cross join the enrolled children with the list of months
            var childrenWithMonths = allEnrolledChildren.SelectMany(aec => allYearMonthsInRange, (aec, ym) => new {
                aec.ChildProgramPK,
                aec.ChildPK,
                ChildID = aec.ProgramSpecificID,
                YearMonth = ym
            }).ToList();

            //Get the BIRs grouped by month and child
            var childInfo = allBIRInfo.GroupBy(abi => new { abi.IncidentDatetime.Year, abi.IncidentDatetime.Month, abi.ChildFK })
                                                        .Select(g => new
                                                        {
                                                            IncidentMonth = g.Key.Month,
                                                            IncidentYear = g.Key.Year,
                                                            g.Key.ChildFK,
                                                            YearMonthDateTime = new DateTime(g.Key.Year, g.Key.Month, 1),
                                                            NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                            PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                        }).ToList();

            //Left join the children on the grouped list by child and year month
            var joinedInfo = childrenWithMonths.GroupJoin(childInfo,
                                                            cwm => new { PKJoin = cwm.ChildPK, YearMonthJoin = cwm.YearMonth },
                                                            ci => new { PKJoin = ci.ChildFK, YearMonthJoin = ci.YearMonthDateTime },
                                                            (cwm, ci) => new
                                                            {
                                                                cwm.ChildID,
                                                                YearMonthDateTime = cwm.YearMonth,
                                                                NumIncidents = (ci.FirstOrDefault() == null ? 0 : ci.First().NumIncidents),
                                                                PercentOfTotal = (ci.FirstOrDefault() == null ? 0 : ci.First().PercentOfTotal)
                                                            }).ToList();

            //Set the child chart (exclude 0s)
            ChildBIRsChart.DataSource = joinedInfo.Where(ji => ji.NumIncidents > 0).ToList();
            ChildBIRsChart.SeriesSorting = SortingMode.Ascending;
            ChildBIRsChart.SeriesTemplate.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;
            ChildBIRsChart.SeriesTemplate.SeriesDataMember = "YearMonthDateTime";
            ChildBIRsChart.SeriesTemplate.ArgumentDataMember = "ChildID";
            ChildBIRsChart.SeriesTemplate.ValueDataMembers.AddRange(new string[] { "NumIncidents", "PercentOfTotal" });
            ChildBIRsChart.SeriesTemplate.SeriesPointsSorting = SortingMode.Ascending;
            ChildBIRsChart.SeriesTemplate.SeriesPointsSortingKey = SeriesPointKey.Argument;

            //Set the children cross tab
            ChildBIRsCrossTab.DataSource = joinedInfo;
            ChildBIRsCrossTab.ColumnFields.Add(new CrossTabColumnField() { FieldName = "YearMonthDateTime" });
            ChildBIRsCrossTab.RowFields.Add(new CrossTabRowField() { FieldName = "ChildID" });
            ChildBIRsCrossTab.DataFields.Add(new CrossTabDataField() { FieldName = "NumIncidents" });
            ChildBIRsCrossTab.GenerateLayout();

            //The total expression
            string totalExpression = "FormatString('{0} ({1:0%})', NumIncidents, (NumIncidents / " + totalBIRs.ToString() + "))";

            //The column format string
            string columnFormatString = "{0:MMM-yyyy}";

            //Configure the cross-tab
            ConfigureMonthlyCrossTab(ChildBIRsCrossTab, columnFormatString, totalExpression);
        }

        #endregion

        #region Child Details

        /// <summary>
        /// This method fills the child information table
        /// </summary>
        private void FillChildInformationTable()
        {
            //Set the detail report data source and sort
            ChildInformationDetailReport.DataSource = allEnrolledChildren;
            ChildInformationDetail.SortFields.Add(new GroupField("ProgramSpecificID", XRColumnSortOrder.Ascending));

            //Set the label expressions
            lblChildID.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ProgramSpecificID"));
            lblChildName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(?ParamViewPrivateChildInfo == True, Concat(FirstName, ' ', LastName), 'HIDDEN')"));
            lblChildDOB.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(?ParamViewPrivateChildInfo == True, FormatString('{0:MM/dd/yyyy}', BirthDate), 'HIDDEN')"));
            lblChildEthnicity.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Ethnicity"));
            lblChildGender.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Gender"));
            lblChildRace.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Race"));
            lblChildEnrollmentDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EnrollmentDate"));
            lblChildIEP.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(HasIEP == true, 'Has IEP', 'No IEP')"));
            lblChildDLL.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(IsDLL == true, 'Is DLL', 'Not DLL')"));
        }

        #endregion

        #region Monthly BIRs by Classroom ID

        /// <summary>
        /// This method fills the classroom charts and tables
        /// </summary>
        private void FillBIRsByClassroomChartsAndTables()
        {
            //To hold all the classrooms
            List<Classroom> allClassrooms;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the classrooms
                //The program, hub, cohort, and state criteria work in an additive manner, but all other criteria are reductive.
                //For example, if you select a state and a program (even if it is outside the state), you will get all the classrooms that
                //are within the state or within the program.  If you select a state, a program, and a few classrooms, you will get all the classrooms
                //that are within the state or within the program only if the classrooms are in the list of selected classrooms.
                allClassrooms = context.Classroom.Include(c => c.Program).AsNoTracking()
                                            .Where(c => (intProgramFKs.Contains(c.ProgramFK) ||
                                                        intHubFKs.Contains(c.Program.HubFK) ||
                                                        intCohortFKs.Contains(c.Program.CohortFK) ||
                                                        intStateFKs.Contains(c.Program.StateFK)) &&
                                                        (intClassroomFKs.Count == 0 || intClassroomFKs.Contains(c.ClassroomPK)))
                                            .OrderBy(c => c.ProgramSpecificID)
                                            .ToList();
            }

            //Cross join the classrooms with the list of months
            var classroomsWithMonths = allClassrooms.SelectMany(ac => allYearMonthsInRange, (ac, ym) => new {
                ac.ClassroomPK,
                ClassroomName = ac.Name,
                ClassroomID = ac.ProgramSpecificID,
                YearMonth = ym
            }).ToList();

            //Get the BIRs grouped by month and classroom
            var classroomInfo = allBIRInfo.GroupBy(abi => new { abi.IncidentDatetime.Year, abi.IncidentDatetime.Month, abi.ClassroomFK })
                                                        .Select(g => new
                                                        {
                                                            IncidentMonth = g.Key.Month,
                                                            IncidentYear = g.Key.Year,
                                                            g.Key.ClassroomFK,
                                                            YearMonthDateTime = new DateTime(g.Key.Year, g.Key.Month, 1),
                                                            NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                            PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                        }).ToList();

            //Left join the classrooms on the grouped list by classroom and year month
            var joinedInfo = classroomsWithMonths.GroupJoin(classroomInfo,
                                                                    cwm => new { PKJoin = cwm.ClassroomPK, YearMonthJoin = cwm.YearMonth },
                                                                    ci => new { PKJoin = ci.ClassroomFK, YearMonthJoin = ci.YearMonthDateTime },
                                                                    (cwm, ci) => new
                                                                    {
                                                                        cwm.ClassroomID,
                                                                        cwm.ClassroomName,
                                                                        YearMonthDateTime = cwm.YearMonth,
                                                                        NumIncidents = (ci.FirstOrDefault() == null ? 0 : ci.First().NumIncidents),
                                                                        PercentOfTotal = (ci.FirstOrDefault() == null ? 0 : ci.First().PercentOfTotal)
                                                                    }).ToList();

            //Set the classroom chart (exclude 0s)
            ClassroomBIRsChart.DataSource = joinedInfo.Where(ji => ji.NumIncidents > 0).ToList();
            ClassroomBIRsChart.SeriesSorting = SortingMode.Ascending;
            ClassroomBIRsChart.SeriesTemplate.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;
            ClassroomBIRsChart.SeriesTemplate.SeriesDataMember = "YearMonthDateTime";
            ClassroomBIRsChart.SeriesTemplate.ArgumentDataMember = "ClassroomID";
            ClassroomBIRsChart.SeriesTemplate.ValueDataMembers.AddRange(new string[] { "NumIncidents", "PercentOfTotal" });
            ClassroomBIRsChart.SeriesTemplate.SeriesPointsSorting = SortingMode.Ascending;
            ClassroomBIRsChart.SeriesTemplate.SeriesPointsSortingKey = SeriesPointKey.Argument;

            //Set the classroom cross tab
            ClassroomBIRsCrossTab.DataSource = joinedInfo;
            ClassroomBIRsCrossTab.ColumnFields.Add(new CrossTabColumnField() { FieldName = "YearMonthDateTime" });
            ClassroomBIRsCrossTab.RowFields.Add(new CrossTabRowField() { FieldName = "ClassroomID" });
            ClassroomBIRsCrossTab.DataFields.Add(new CrossTabDataField() { FieldName = "NumIncidents" });
            ClassroomBIRsCrossTab.GenerateLayout();

            //The total expression
            string totalExpression = "FormatString('{0} ({1:0%})', NumIncidents, (NumIncidents / " + totalBIRs.ToString() + "))";

            //The column format string
            string columnFormatString = "{0:MMM-yyyy}";

            //Configure the cross-tab
            ConfigureMonthlyCrossTab(ClassroomBIRsCrossTab, columnFormatString, totalExpression);
        }

        #endregion
    }
}
