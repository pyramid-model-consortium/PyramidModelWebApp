using Microsoft.AspNet.Identity;
using Pyramid.Models;
using System;
using System.Web;
using Pyramid.Code;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Pyramid
{
    public partial class LoggedIn : System.Web.UI.MasterPage
    {
        public List<CodeProgramRolePermission> CurrentPermissions { get; set; }
        protected ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Attempt to get the role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permissions
            CurrentPermissions = Utilities.GetProgramRolePermissionsFromDatabase(currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Check to see if confidentiality is enabled
            if (Utilities.IsConfidentialityEnabled(currentProgramRole.CurrentStateFK.Value))
            {
                //Determine if the user needs to accept the confidentiality agreement
                if (!Utilities.IsConfidentialityAccepted(Context.User.Identity.Name, currentProgramRole.CurrentStateFK.Value) &&
                        currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
                {
                    //The user needs to accept the confidentiality agreement, redirect them to the proper page
                    Response.Redirect(string.Format("/Pages/Confidentiality.aspx?ReturnUrl={0}", Server.UrlEncode(Request.Url.PathAndQuery)));
                }
                else
                {
                    //Set the confidentiality link URL
                    lnkConfidentiality.NavigateUrl = string.Format("/Pages/ViewFile.aspx?StatePK={0}", currentProgramRole.CurrentStateFK.Value.ToString());
                }
            }
            else
            {
                //Hide the confidentiality link
                lnkConfidentiality.Visible = false;
            }

            if (!IsPostBack)
            {
                //Show/hide the test site message
                divTestSiteMessage.Visible = Utilities.IsTestSite();

                //Show/hide the locked program message
                if(currentProgramRole.IsProgramLocked.HasValue && currentProgramRole.IsProgramLocked.Value)
                {
                    //Show the message
                    divLockedProgram.Visible = true;

                    //Set the message texta
                    lblLockedProgram.Text = "Your role is restricted since all the programs you are authorized to access are currently inactive.  You can view information and reports, but you cannot add, edit, or delete any information.";
                }
                else
                {
                    divLockedProgram.Visible = false;
                }

                //Set link visibility
                SetLinkVisibility(currentProgramRole, CurrentPermissions);

                //Set the labels' text to the user's role values
                lblUserProgram.Text = currentProgramRole.ProgramName;
                lblUserRole.Text = currentProgramRole.RoleName;

                //Set the disclaimer
                if (!string.IsNullOrWhiteSpace(currentProgramRole.StateDisclaimer))
                {
                    ltlStateDisclaimer.Text = currentProgramRole.StateDisclaimer.Replace("*br*", "<br/>");
                }

                //Set the logo
                bsImgLogo.ImageUrl = "/Content/images/" + currentProgramRole.StateThumbnailLogoFileName;

                //Set the application title label
                lblApplicationTitle.Text = Utilities.GetApplicationTitle(currentProgramRole);
            }

            //Prevent non-admins from accessing the admin menu and error log
            if(currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                //Show the admin menu and all the options
                liAdminMenu.Visible = true;

                lnkUserManagement.Visible = true;
                lnkProgramManagement.Visible = true;
                lnkBulkTraining.Visible = true;
                lnkStateSettings.Visible = true;
                lnkReportCatalogMaintenance.Visible = true;
                lnkReportDesigner.Visible = true;
                lnkErrorLog.Visible = true;
            }
            else if(currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN || 
                        currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN)
            {
                //Show the admin menu and relevant options
                liAdminMenu.Visible = true;

                lnkUserManagement.Visible = true;
                lnkProgramManagement.Visible = true;
                lnkBulkTraining.Visible = true;
                lnkStateSettings.Visible = true;

                lnkReportCatalogMaintenance.Visible = false;
                lnkReportDesigner.Visible = false;
                lnkErrorLog.Visible = false;
            }
            else if(currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
            {
                //Note: For now, the only national role that can access the admin menu is the National Data Admin.
                //They can only see the bulk training link and user management link.
                liAdminMenu.Visible = true;

                lnkBulkTraining.Visible = true;
                lnkUserManagement.Visible = true;

                lnkProgramManagement.Visible = false;
                lnkStateSettings.Visible = false;
                lnkReportCatalogMaintenance.Visible = false;
                lnkReportDesigner.Visible = false;
                lnkErrorLog.Visible = false;
            }
            else
            {
                //Hide the admin menu and all options
                liAdminMenu.Visible = false;

                lnkUserManagement.Visible = false;
                lnkProgramManagement.Visible = false;
                lnkBulkTraining.Visible = false;
                lnkStateSettings.Visible = false;
                lnkReportCatalogMaintenance.Visible = false;
                lnkReportDesigner.Visible = false;
                lnkErrorLog.Visible = false;
            }
        }

        /// <summary>
        /// This method sets the visibility of dashboard links based on the user's permissions.
        /// </summary>
        /// <param name="programRole">The user's role information</param>
        /// <param name="permissions">The user's permissions</param>
        private void SetLinkVisibility(ProgramAndRoleFromSession programRole, List<CodeProgramRolePermission> permissions)
        {
            //A list of all the links and their form abbreviations
            var links = new[] {
                new {LinkDiv = lnkASQSEDashboard, FormAbbreviations = new string[] { "ASQSE" } },
                new {LinkDiv = lnkBIRDashboard, FormAbbreviations = new string[] { "BIR" } },
                new {LinkDiv = lnkBOQDashboard, FormAbbreviations = new string[] { "BOQ" } },
                new {LinkDiv = lnkBOQFCCDashboard, FormAbbreviations = new string[] { "BOQFCC" } },
                new {LinkDiv = lnkCCLDashboard, FormAbbreviations = new string[] { "CCL" } },
                new {LinkDiv = lnkChildDashboard, FormAbbreviations = new string[] { "CHILD" } },
                new {LinkDiv = lnkClassDashboard, FormAbbreviations = new string[] { "CLASS" } },
                new {LinkDiv = lnkCWLTDashboard, FormAbbreviations = new string[] { "CWLTAP", "BOQCWLT", "CWLTM", "CWLTA", "CWLTAT" } },
                new {LinkDiv = lnkLCDashboard, FormAbbreviations = new string[] { "LCL", "LCPS", "LCHS", "LCPN", "LCHN", "LCPD", "LCHD", "LCCS", "LCCD" } },
                new {LinkDiv = lnkMCDashboard, FormAbbreviations = new string[] { "MCTT", "MCTD" } },
                new {LinkDiv = lnkOSESDashboard, FormAbbreviations = new string[] { "OSES" } },
                new {LinkDiv = lnkPEDashboard, FormAbbreviations = new string[] { "PE" } },
                new {LinkDiv = lnkPLTDashboard, FormAbbreviations = new string[] { "PAP", "PAPFCC", "PLTM", "PA" } },
                new {LinkDiv = lnkSLTDashboard, FormAbbreviations = new string[] { "SLTAP", "BOQSLT", "SLTM", "SLTA", "SLTWG" } },
                new {LinkDiv = lnkTPOTDashboard, FormAbbreviations = new string[] { "TPOT" } },
                new {LinkDiv = lnkTPITOSDashboard, FormAbbreviations = new string[] { "TPITOS" } },
                new {LinkDiv = lnkULFDashboard, FormAbbreviations = new string[] { "ULF" } }
            }.ToList();

            //Loop through all the links
            foreach (var link in links)
            {
                //Determine if the user is allowed to view the dashboard for ANY of the forms inside the dashboard
                if (permissions.Where(p => link.FormAbbreviations.Contains(p.CodeForm.FormAbbreviation) && p.AllowedToViewDashboard == true).Count() > 0)
                {
                    //Allowed, show the link
                    link.LinkDiv.Visible = true;
                }
                else
                {
                    //Not allowed, hide the link
                    link.LinkDiv.Visible = false;
                }
            }

            //Show/hide the BOQFCC link
            if (lnkBOQFCCDashboard.Visible &&
                programRole.ShowBOQFCC.HasValue &&
                programRole.ShowBOQFCC.Value)
            {
                lnkBOQFCCDashboard.Visible = true;
            }
            else
            {
                lnkBOQFCCDashboard.Visible = false;
            }

            //Show/hide the BOQ link
            if (lnkBOQDashboard.Visible &&
                programRole.ShowBOQ.HasValue &&
                programRole.ShowBOQ.Value)
            {
                lnkBOQDashboard.Visible = true;
            }
            else
            {
                lnkBOQDashboard.Visible = false;
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