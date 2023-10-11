using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptLeadershipCoachCCDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptLeadershipCoachCCDataDump()
        {
            InitializeComponent();
        }

        public override void PrintingSystem_XlSheetCreated(object sender, DevExpress.XtraPrinting.XlSheetCreatedEventArgs e)
        {
            //To hold the Excel worksheet name
            string sheetName;

            //Set the worksheet name based on the index
            switch (e.Index)
            {
                case 0:
                    sheetName = "CC Meeting Schedules";
                    break;
                case 1:
                    sheetName = "CC Meeting Debriefs";
                    break;
                case 2:
                    sheetName = "CC Debrief Group Members";
                    break;
                case 3:
                    sheetName = "CC Session Debriefs";
                    break;
                case 4:
                    sheetName = "CC Session Attendees";
                    break;
                case 5:
                    sheetName = "Criteria";
                    break;
                default:
                    sheetName = "";
                    break;
            }

            //If the worksheet name has value, set the Excel worksheet name
            if (!string.IsNullOrWhiteSpace(sheetName))
            {
                e.SheetName = sheetName;
            }
        }
    }
}
