using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptClassroomCoachingLogDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptClassroomCoachingLogDataDump()
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
                    sheetName = "Coaching Log";
                    break;
                case 1:
                    sheetName = "Coachees";
                    break;
                case 2:
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
