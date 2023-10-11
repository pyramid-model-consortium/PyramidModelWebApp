using Pyramid.Code;
using System;

namespace Pyramid.Reports.UserReports
{
    public partial class ReportDesigner : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(Session);
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                Response.Redirect("/Default.aspx");
            }
        }
    }
}