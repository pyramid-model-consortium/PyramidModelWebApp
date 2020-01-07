using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.XtraCharts;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptASQSETrend : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptASQSETrend()
        {
            InitializeComponent();
        }

        private void xrChart1_CustomDrawAxisLabel(object sender, DevExpress.XtraCharts.CustomDrawAxisLabelEventArgs e)
        {
            AxisBase axis = e.Item.Axis;

            if (axis is AxisY)
            {
                int scoreTypeCode = Convert.ToInt32(e.Item.AxisValue);

                if (scoreTypeCode == 1)
                    e.Item.Text = "Well Below";
                else if (scoreTypeCode == 2)
                    e.Item.Text = "Monitor";
                else if (scoreTypeCode == 3)
                    e.Item.Text = "Above Cutoff";
                else
                    e.Item.Text = "ERROR";
            }
        }
    }
}
