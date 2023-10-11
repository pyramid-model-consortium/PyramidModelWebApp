using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using Pyramid.Code;
using System.Web;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptLeadershipCoachLogDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptLeadershipCoachLogDataDump()
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
                    sheetName = "Leadership Coach Logs";
                    break;
                case 1:
                    sheetName = "Involved Coaches";
                    break;
                case 2:
                    sheetName = "Team Members Engaged";
                    break;
                case 3:
                    sheetName = "Multi Select Fields";
                    break;
                case 4:
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

        private void RptLeadershipCoachLogDataDump_BeforePrint(object sender, CancelEventArgs e)
        {
            //Get the user's program role from session
            ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(HttpContext.Current.Session);

            //Set the parameter based on the user's role
            if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH)
            {
                ParamIsUserLeadershipCoach.Value = true;
            }
            else
            {
                ParamIsUserLeadershipCoach.Value = false;
            }
        }
    }
}
