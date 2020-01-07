using Microsoft.AspNet.Identity;
using Pyramid.Models;
using System;
using System.Web;
using Pyramid.Code;
using System.Text;

namespace Pyramid
{
    public partial class LoggedIn : System.Web.UI.MasterPage
    {
        protected ProgramAndRoleFromSession programRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Attempt to get the role from session
            programRole = Utilities.GetProgramRoleFromSession(Session);

            if (!IsPostBack)
            {
                //Show/hide the test site message
                divTestSiteMessage.Visible = Utilities.IsTestSite();

                //Set the labels' text to the user's role values
                lblUserProgram.Text = programRole.ProgramName;
                lblUserRole.Text = programRole.RoleName;

                //Set the disclaimer
                ltlStateDisclaimer.Text = programRole.StateDisclaimer;

                //Set the logo
                bsImgLogo.ImageUrl = "/Content/images/" + programRole.StateLogoFileName;

                //Set the application title label
                lblApplicationTitle.Text = Utilities.GetApplicationTitle(programRole);
            }

            //Prevent non-admins from accessing the admin menu and error log
            if(programRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                //Show the admin menu and error log link
                liAdminMenu.Visible = true;
                lnkErrorLog.Visible = true;
                lnkReportCatalogMaintenance.Visible = true;
                lnkReportDesigner.Visible = true;
            }
            else if(programRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
            {
                //Show the admin menu and hide the error log link
                liAdminMenu.Visible = true;
            }
            else
            {
                liAdminMenu.Visible = false;
                lnkErrorLog.Visible = false;
            }
        }

        protected void lbLogOut_Click(object sender, EventArgs e)
        {
            //Log the user out
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            //Record the logout if a record for the login existed
            if (Session["LoginHistoryPK"] != null && !String.IsNullOrWhiteSpace(Session["LoginHistoryPK"].ToString()))
            {
                //Get the login history pk from session
                int historyPK = Convert.ToInt32(Session["LoginHistoryPK"].ToString());

                //Add the record to the database with the logout time
                using (PyramidContext context = new PyramidContext())
                {
                    LoginHistory history = context.LoginHistory.Find(historyPK);
                    history.LogoutTime = DateTime.Now;
                    history.LogoutType = "User logged out via the logout button on the navbar";
                    context.SaveChanges();
                }
            }

            //Ensure that the user's session is clear
            Session.Abandon();

            //Redirect the user to login page
            Response.Redirect("/Account/Login.aspx?messageType=LogOutSuccess");
        }

        /// <summary>
        /// Hide the master page title so that the content page can create a custom one
        /// </summary>
        public void HideTitle()
        {
            divMasterPageTitle.Visible = false;
        }

        /// <summary>
        /// Hide the master page footer so that the content page can create a custom one
        /// </summary>
        public void HideFooter()
        {
            divMasterPageFooter.Visible = false;
        }
    }
}