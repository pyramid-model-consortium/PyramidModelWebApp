using DevExpress.XtraReports.UI;
using Pyramid.Code;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Web;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptMasterCadreDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptMasterCadreDataDump()
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
                    sheetName = "Activity Trackers";
                    break;
                case 1:
                    sheetName = "Activity Tracker Dates";
                    break;
                case 2:
                    sheetName = "Training Debriefs";
                    break;
                case 3:
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

        private void RptMasterCadreDataDump_BeforePrint(object sender, CancelEventArgs e)
        {
            //Get the user's program role from session
            ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(HttpContext.Current.Session);

            //Determine if the ASPIRE fields should be shown or not
            bool showAspireFields = (currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK ? true : false);

            //Set the visibility of the ASPIRE fields
            lblTDUploadedAspireHeader.Visible = showAspireFields;
            lblTDUploadedAspire.Visible = showAspireFields;
        }
    }
}
