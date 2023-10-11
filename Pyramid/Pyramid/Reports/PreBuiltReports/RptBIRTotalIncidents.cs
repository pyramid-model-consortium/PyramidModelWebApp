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
    public partial class RptBIRTotalIncidents : Pyramid.Reports.PreBuiltReports.MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspBIRAllInfo_Result> allBIRInfo;
        List<rspEnrolledChildren_Result> allEnrolledChildren;
        List<DateTime> allYearMonthsInRange;
        List<int> intProgramFKs, intHubFKs, intCohortFKs, intStateFKs, intClassroomFKs;
        int totalBIRs;
        int totalChildren;
        int totalChildrenWithBIRs;

        public RptBIRTotalIncidents()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptBIRTotalIncidents_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
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

            //If the report focus is for classrooms or children specifically, then ignore the non-relevant criteria
            if(reportFocus.ToUpper() == "CHI")
            {
                //Children were selected as the focus, ignore the classroom(s) criteria
                classroomFKs = null;
                intClassroomFKs.Clear();
            }
            else if (reportFocus.ToUpper() == "CR")
            {
                //Classrooms were selected as the focus, ignore the child(ren) criteria
                childFKs = null;
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
            FillIncidentsByDayChartsAndTables();

            switch (reportFocus.ToUpper())
            {
                case "CHI":
                    //Fill and display ONLY the child section
                    FillBIRsByChildChartsAndTables();
                    ClassroomBIRsDetailReport.Visible = false;
                    ChildBIRsDetailReport.Visible = true;
                    break;
                case "CR":
                    //Fill and display ONLY the classroom section
                    FillBIRsByClassroomChartsAndTables();
                    ClassroomBIRsDetailReport.Visible = true;
                    ChildBIRsDetailReport.Visible = false;
                    break;
                default:
                    //Fill and display both sections
                    FillBIRsByChildChartsAndTables();
                    FillBIRsByClassroomChartsAndTables();
                    ClassroomBIRsDetailReport.Visible = true;
                    ChildBIRsDetailReport.Visible = true;
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

            //Get the chart info
            var incidentsByDayChartInfo = incidentsByDayInfo.ToList();

            //--------------------- Total Number of Incidents by Day Chart Start -----------------------

            //Set the chart data source
            IncidentsByDayChart.DataSource = incidentsByDayChartInfo;

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
