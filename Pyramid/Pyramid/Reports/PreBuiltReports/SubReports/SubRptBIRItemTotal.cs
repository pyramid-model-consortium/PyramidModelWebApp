using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports.SubReports
{
    public partial class SubRptBIRItemTotal : XtraReport
    {

        public SubRptBIRItemTotal()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method fills the report charts, cross-tabs, and tables
        /// </summary>
        /// <param name="listBIRInfo">All the BIR info</param>
        /// <param name="codeTableRows">All the item code table rows</param>
        /// <param name="groupingField">The field to group the BIR info by</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="itemChartColor">The chart color for the item</param>
        public void FillReport(List<rspBIRAllInfo_Result> listBIRInfo, List<Utilities.CodeTableInfo> codeTableRows, 
                                string groupingField, string itemName, Color itemChartColor)
        {
            //Get the total BIRs
            int totalBIRs = listBIRInfo.Count;

            //Get the BIRs grouped by item
            var groupedBIRs = listBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                                        .Select(g => new
                                                        {
                                                            ItemCodeFK = (int)g.Key,
                                                            NumIncidents = g.Select(abi => abi.BehaviorIncidentPK).Count(),
                                                            PercentOfTotal = (double)g.Select(abi => abi.BehaviorIncidentPK).Count() / totalBIRs
                                                        }).ToList();

            //Left join the code information on the grouped BIRs
            var finalBIRInfo = codeTableRows.GroupJoin(groupedBIRs,
                                                        pb => pb.CodeTablePK,
                                                        pbi => pbi.ItemCodeFK,
                                                        (pb, pbi) => new
                                                        {
                                                            pb.ItemAbbreviation,
                                                            pb.ItemDescription,
                                                            NumIncidents = (pbi.FirstOrDefault() == null ? 0 : pbi.First().NumIncidents),
                                                            PercentOfTotal = (pbi.FirstOrDefault() == null ? 0 : pbi.First().PercentOfTotal)
                                                        }).ToList();

            //Set the labels and bookmarks
            lblItemSection.Bookmark = string.Format("{0} Analysis", itemName);
            lblItemTotalChartTitle.Text = string.Format("BIRs by {0} Chart", itemName);
            lblItemTotalChartTitle.Bookmark = "Chart";
            lblItemTotalChartTitle.BookmarkParent = lblItemSection;
            lblChartInformationTitle.Text = string.Format("BIRs by {0} Table", itemName);
            lblChartInformationTitle.Bookmark = "Table with Averages";
            lblChartInformationTitle.BookmarkParent = lblItemSection;
            lblItemNameTitle.Text = itemName;

            //--------------------- Total Number of Incidents by Item Chart Start -----------------------

            //Set the chart data source
            ItemTotalChart.DataSource = finalBIRInfo;

            //Set the Total # of Incidents chart series
            ItemTotalChart.Series[0].View.Color = itemChartColor;
            ItemTotalChart.Series[0].ArgumentScaleType = ScaleType.Auto;
            ItemTotalChart.Series[0].ArgumentDataMember = "ItemAbbreviation";
            ItemTotalChart.Series[0].ValueScaleType = ScaleType.Numerical;
            ItemTotalChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumIncidents" });

            //--------------------- Total Number of Incidents by Item Chart End -----------------------

            //--------------------- Item Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.DataSource = finalBIRInfo;
            this.ItemDetail.SortFields.Add(new GroupField("NumIncidents", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblItemDescription.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemDescription"));
            lblItemAbbreviation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemAbbreviation"));
            lblActivityNumIncidents.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumIncidents"));
            lblActivityPercent.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotal"));

            //Set the group footer label expressions
            lblActivityNumIncidentsAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([NumIncidents])"));
            lblActivityNumIncidentsAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblActivityPercentAverage.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumAvg([PercentOfTotal])"));
            lblActivityPercentAverage.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Item Table End -----------------------
        }
    }
}
