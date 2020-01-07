using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.XtraCharts;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptTPOTTrend : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptTPOTTrend()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method fires before the chart is shown and it formats the series names
        /// </summary>
        /// <param name="sender">The chartKeyPractices XRChart</param>
        /// <param name="e"></param>
        private void chartKeyPractices_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //Go through each series in the chart
            foreach (Series series in chartKeyPractices.Series)
            {
                //Get the series name
                string seriesName = series.Name;

                //Split the series name on the dashes
                string[] seriesNameList = series.Name.Split('-');

                //If the name list has enough records, set the series name
                if(seriesNameList.Length >= 3)
                    series.Name = seriesNameList[2] + " " + seriesNameList[0];
            }
        }
    }
}
