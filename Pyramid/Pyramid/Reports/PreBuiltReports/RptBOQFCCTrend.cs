﻿using DevExpress.XtraCharts;
using DevExpress.DataAccess.Sql.DataApi;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBOQFCCTrend : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptBOQFCCTrend()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method fires before the chart is shown and it formats the series dates
        /// </summary>
        /// <param name="sender">The chartCriticalElementRating XRChart</param>
        /// <param name="e"></param>
        private void chartCriticalElementRating_BeforePrint(object sender, CancelEventArgs e)
        {
            //Go through each series in the chart
            foreach (Series series in chartCriticalElementRating.Series)
            {
                //Split the series name on the dashes
                string[] seriesNameList = series.Name.Split('-');

                //If the name list has enough records, set the series name
                if (seriesNameList.Length >= 3)
                    series.Name = seriesNameList[2] + " " + seriesNameList[0];
            }
        }

        /// <summary>
        /// This method is used to set custom argument text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chartPercentagesByTimeframe_CustomDrawAxisLabel(object sender, CustomDrawAxisLabelEventArgs e)
        {
            //Get the axis
            AxisBase axis = e.Item.Axis;

            //Only do this on the X axis
            if (axis is AxisX)
            {
                //Get the argument text
                string currentArgumentText = e.Item.Text;

                //Split the argument text on the dashes
                string[] argumentNameList = currentArgumentText.Split('-');

                //If the argument text list has enough records, set the argument text
                if (argumentNameList.Length >= 3)
                    e.Item.Text = argumentNameList[2] + " " + argumentNameList[0];
            }
        }

        class GroupedValues
        {
            public string GroupingValue { get; set; }
            public string GroupingText { get; set; }
            public int? TotalOfAllIndicators => NAIndicators + InPlaceIndicators + PartialIndicators + NotInPlaceIndicators;
            public int? NAIndicators { get; set; }
            public int? InPlaceIndicators { get; set; }
            public int? PartialIndicators { get; set; }
            public int? NotInPlaceIndicators { get; set; }
            public double? PercentNA => (TotalOfAllIndicators > 0 && NAIndicators > 0 ? NAIndicators / (double?)TotalOfAllIndicators : 0.00d);
            public double? PercentInPlace => (TotalOfAllIndicators > 0 && InPlaceIndicators > 0 ? InPlaceIndicators / (double?)TotalOfAllIndicators : 0.00d);
            public double? PercentPartial => (TotalOfAllIndicators > 0 && PartialIndicators > 0 ? PartialIndicators / (double?)TotalOfAllIndicators : 0.00d);
            public double? PercentNotInPlace => (TotalOfAllIndicators > 0 && NotInPlaceIndicators > 0 ? NotInPlaceIndicators / (double?)TotalOfAllIndicators : 0.00d);
        }

        private void RptBOQFCCTrend_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the report data source
            ITable src = sqlDataSource1.Result.FirstOrDefault();

            //Convert the report data source in to a DataTable
            DataTable dest = new DataTable(src.Name);
            foreach (IColumn column in src.Columns)
                dest.Columns.Add(column.Name, column.Type);
            foreach (IRow row in src)
                dest.Rows.Add(row.ToArray());

            //Convert the DataTable into a list of objects
            List<GroupedValues> groupedValueList = dest.AsEnumerable().GroupBy(r => new { GroupingValue = Convert.ToString(r["GroupingValue"]), GroupingText = Convert.ToString(r["GroupingText"]) }).Select(g => new GroupedValues()
            {
                GroupingValue = g.Key.GroupingValue,
                GroupingText = g.Key.GroupingText,
                NAIndicators = g.Sum(r => (int?)r["CriticalElementNumNotApplicable"]),
                InPlaceIndicators = g.Sum(r => (int?)r["CriticalElementNumInPlace"]),
                PartialIndicators = g.Sum(r => (int?)r["CriticalElementNumPartial"]),
                NotInPlaceIndicators = g.Sum(r => (int?)r["CriticalElementNumNotInPlace"])
            }).ToList();

            //Set the chart data source
            chartPercentagesByTimeframe.DataSource = groupedValueList;

            //Set the chart series argument and value members
            chartPercentagesByTimeframe.Series[0].ArgumentDataMember = "GroupingValue";
            chartPercentagesByTimeframe.Series[0].ValueDataMembers.AddRange(new string[] { "PercentInPlace" });
            chartPercentagesByTimeframe.Series[1].ArgumentDataMember = "GroupingValue";
            chartPercentagesByTimeframe.Series[1].ValueDataMembers.AddRange(new string[] { "PercentPartial" });
            chartPercentagesByTimeframe.Series[2].ArgumentDataMember = "GroupingValue";
            chartPercentagesByTimeframe.Series[2].ValueDataMembers.AddRange(new string[] { "PercentNotInPlace" });
            chartPercentagesByTimeframe.Series[3].ArgumentDataMember = "GroupingValue";
            chartPercentagesByTimeframe.Series[3].ValueDataMembers.AddRange(new string[] { "PercentNA" });

            //If the report was run for more than one state, hide the individual forms section
            if (ParamStateFKs != null && ParamStateFKs.Value != null && !string.IsNullOrWhiteSpace(ParamStateFKs.Value.ToString()))
            {
                string[] allStateFKs = ParamStateFKs.Value.ToString().Split(',');

                if (allStateFKs.Length > 1)
                {
                    IndividualFormsDetailReport.DataSource = null;
                    IndividualFormsDetailReport.Visible = false;
                }
            }
        }
    }
}
