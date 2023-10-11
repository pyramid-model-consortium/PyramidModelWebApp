using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports.MasterReports
{
    public partial class RptDataDumpMaster : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptDataDumpMaster()
        {
            InitializeComponent();

            //Add an event to Excel exports that allows setting custom worksheet names
            this.PrintingSystem.XlSheetCreated += PrintingSystem_XlSheetCreated;
        }

        public virtual void PrintingSystem_XlSheetCreated(object sender, DevExpress.XtraPrinting.XlSheetCreatedEventArgs e)
        {
            //To hold the Excel worksheet name
            string sheetName;

            //Set the worksheet name based on the index
            switch (e.Index)
            {
                case 0:
                    sheetName = "Report";
                    break;
                case 1:
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
