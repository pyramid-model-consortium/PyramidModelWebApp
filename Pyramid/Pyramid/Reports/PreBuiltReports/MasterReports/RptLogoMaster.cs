using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Web;

namespace Pyramid.Reports.PreBuiltReports.MasterReports
{
    public partial class RptLogoMaster : DevExpress.XtraReports.UI.XtraReport
    {
        public RptLogoMaster()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method sets the logo url to the proper path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgLogo_BeforePrint(object sender, CancelEventArgs e)
        {
            if(ParamLogoPath.Value != null)
            {
                imgLogo.ImageUrl = ParamLogoPath.Value.ToString();
            }
        }
    }
}
