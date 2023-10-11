using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptProgramManagementDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptProgramManagementDataDump()
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
                    sheetName = "Cohorts";
                    break;
                case 1:
                    sheetName = "Hubs";
                    break;
                case 2:
                    sheetName = "Programs";
                    break;
                case 3:
                    sheetName = "Program Types";
                    break;
                case 4:
                    sheetName = "Program Statuses";
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
