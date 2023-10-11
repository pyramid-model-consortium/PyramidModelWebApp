using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;
using DevExpress.XtraReports.UI.CrossTab;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports.SubReports
{
    public partial class SubRptBIRDetailItemSection : DevExpress.XtraReports.UI.XtraReport
    {
        public SubRptBIRDetailItemSection()
        {
            InitializeComponent();
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

            //Create the header and total styles
            XRControlStyle headerStyle = new XRControlStyle()
            {
                Name = "CustomCrossTabHeaderStyle",
                BackColor = System.Drawing.SystemColors.Control,
                Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
            };
            XRControlStyle totalStyle = new XRControlStyle()
            {
                Name = "CustomCrossTabTotalStyle",
                Font = new System.Drawing.Font("Arial", 9.75F, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
            };

            //Set the style for the header and total cells
            crossTab.CrossTabStyles.HeaderAreaStyle = headerStyle;
            crossTab.CrossTabStyles.TotalAreaStyle = totalStyle;
        }

        /// <summary>
        /// This method fills the report charts, cross-tabs, and tables
        /// </summary>
        /// <param name="allYearMonthsInRange">All the year-month combos in the report range</param>
        /// <param name="listBIRInfo">All the BIR info</param>
        /// <param name="codeTableRows">All the item code table rows</param>
        /// <param name="groupingField">The field to group the BIR info by</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="barSeriesColor">The bar series color</param>
        /// <param name="lineSeriesColor">The line series color</param>
        public void FillReport(List<DateTime> allYearMonthsInRange, List<rspBIRAllInfo_Result> listBIRInfo,
                                List<Utilities.CodeTableInfo> codeTableRows, string groupingField, string itemName, 
                                Color barSeriesColor, Color lineSeriesColor)
        {
            //Get the total BIRs
            int totalBIRs = listBIRInfo.Count();

            //Cross join the code table information with the list of months
            var codeItemsWithMonths = codeTableRows.SelectMany(apb => allYearMonthsInRange, (apb, ym) => new {
                apb.CodeTablePK,
                apb.ItemAbbreviation,
                apb.ItemDescription,
                YearMonth = ym
            }).ToList();

            //Get the BIRs grouped by month and item
            var itemMonthlyInfo = listBIRInfo.GroupBy(abi => new { abi.IncidentDatetime.Year, abi.IncidentDatetime.Month, ItemCodeFK = Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField) })
                                            .Select(g => new
                                            {
                                                IncidentMonth = g.Key.Month,
                                                IncidentYear = g.Key.Year,
                                                ItemCodeFK = (int)g.Key.ItemCodeFK,
                                                YearMonthDateTime = new DateTime(g.Key.Year, g.Key.Month, 1),
                                                NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                            }).ToList();

            //Left join the code and year month info on the grouped BIR info
            var joinedMonthlyInfo = codeItemsWithMonths.GroupJoin(itemMonthlyInfo,
                                                    pbm => new { PKJoin = pbm.CodeTablePK, YearMonthJoin = pbm.YearMonth },
                                                    pbi => new { PKJoin = pbi.ItemCodeFK, YearMonthJoin = pbi.YearMonthDateTime },
                                                    (pbm, pbi) => new
                                                    {
                                                        pbm.CodeTablePK,
                                                        pbm.ItemAbbreviation,
                                                        pbm.ItemDescription,
                                                        YearMonthDateTime = pbm.YearMonth,
                                                        NumIncidents = (pbi.FirstOrDefault() == null ? 0 : pbi.First().NumIncidents),
                                                        PercentOfTotal = (pbi.FirstOrDefault() == null ? 0 : pbi.First().PercentOfTotal)
                                                    }).ToList();

            //Get the total info by item
            var joinedTotalInfo = codeTableRows.GroupJoin(listBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                            .Select(g => new
                                            {
                                                ItemCodeFK = (int)g.Key,
                                                NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                            }),
                                            apb => apb.CodeTablePK,
                                            pbi => pbi.ItemCodeFK,
                                            (apb, pbi) => new
                                            {
                                                apb.CodeTablePK,
                                                apb.ItemAbbreviation,
                                                apb.ItemDescription,
                                                NumIncidents = (pbi.FirstOrDefault() == null ? 0 : pbi.First().NumIncidents),
                                                PercentOfTotal = (pbi.FirstOrDefault() == null ? 0 : pbi.First().PercentOfTotal)
                                            }).ToList();

            //Set the labels and bookmarks
            lblItemSection.Bookmark = string.Format("{0} Analysis", itemName);
            lblMonthlyItemChartTitle.Text = string.Format("Monthly BIRs by {0}", itemName);
            lblMonthlyItemChartTitle.Bookmark = "Monthly BIRs";
            lblMonthlyItemChartTitle.BookmarkParent = lblItemSection;
            lblItemComboChartTitle.Text = string.Format("Total BIRs by {0}", itemName);
            lblItemComboChartTitle.Bookmark = "Total BIRs";
            lblItemComboChartTitle.BookmarkParent = lblItemSection;
            lblItemPieChartTitle.Text = string.Format("% of Total BIRs by {0}", itemName);
            lblItemPieChartTitle.Bookmark = "% of Total BIRs";
            lblItemPieChartTitle.BookmarkParent = lblItemSection;
            lblItemCrossTabTitle.Text = string.Format("{0} Chart Information", itemName);
            lblItemCrossTabTitle.Bookmark = "Chart Information";
            lblItemCrossTabTitle.BookmarkParent = lblItemSection;
            lblLegendTitle.Text = string.Format("{0} Legend", itemName);
            lblLegendTitle.Bookmark = string.Format("{0} Legend", itemName);
            lblLegendTitle.BookmarkParent = lblItemSection;

            //Fill the monthly chart
            MonthlyItemChart.DataSource = joinedMonthlyInfo;
            MonthlyItemChart.SeriesTemplate.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;
            MonthlyItemChart.SeriesTemplate.SeriesDataMember = "YearMonthDateTime";
            MonthlyItemChart.SeriesTemplate.ArgumentDataMember = "ItemAbbreviation";
            MonthlyItemChart.SeriesTemplate.ValueDataMembers.AddRange(new string[] { "NumIncidents", "PercentOfTotal" });
            MonthlyItemChart.SeriesTemplate.SeriesPointsSorting = SortingMode.Ascending;
            MonthlyItemChart.SeriesTemplate.SeriesPointsSortingKey = SeriesPointKey.Argument;

            //Fill the pie chart
            ItemPieChart.DataSource = joinedTotalInfo;
            ItemPieChart.Series[0].ArgumentDataMember = "ItemAbbreviation";
            ItemPieChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ItemPieChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //Fill the combo chart
            ItemComboChart.DataSource = joinedTotalInfo;
            ItemComboChart.Series[0].ArgumentDataMember = "ItemAbbreviation";
            ItemComboChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ItemComboChart.Series[0].View.Color = barSeriesColor;
            ItemComboChart.Series[0].ValueDataMembers.AddRange(new string[] { "PercentOfTotal" });
            ItemComboChart.Series[1].ArgumentDataMember = "ItemAbbreviation";
            ItemComboChart.Series[1].ValueScaleType = ScaleType.Numerical;
            ItemComboChart.Series[1].View.Color = lineSeriesColor;
            ItemComboChart.Series[1].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //Fill the cross tab
            ItemCrossTab.DataSource = joinedMonthlyInfo;
            ItemCrossTab.ColumnFields.Add(new CrossTabColumnField() { FieldName = "YearMonthDateTime" });
            ItemCrossTab.RowFields.Add(new CrossTabRowField() { FieldName = "ItemAbbreviation" });
            ItemCrossTab.DataFields.Add(new CrossTabDataField() { FieldName = "NumIncidents" });
            ItemCrossTab.GenerateLayout();

            //The total expression
            string totalExpression = "FormatString('{0} ({1:0%})', NumIncidents, (NumIncidents / " + totalBIRs.ToString() + "))";

            //The column format string
            string columnFormatString = "{0:MMM-yyyy}";

            //Configure the cross-tab to ensure proper formatting
            ConfigureMonthlyCrossTab(ItemCrossTab, columnFormatString, totalExpression);

            //Fill the legend
            LegendDetailReport.DataSource = codeTableRows;
            lblItemAbbreviation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemAbbreviation"));
            lblItemDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemDescription"));
        }
    }
}
