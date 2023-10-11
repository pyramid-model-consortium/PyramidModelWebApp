using DevExpress.XtraReports.Web;
using Pyramid.Code;
using Pyramid.Models;
using Pyramid.Reports.PreBuiltReports.MasterReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Pyramid.MasterPages
{
    public partial class Dashboard : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                //Get the current program role
                ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

                //Get the user permissions from the master page
                List<CodeProgramRolePermission> currentPermissions = ((LoggedIn)this.Master).CurrentPermissions;

                //Set link visibility
                SetLinkVisibility(currentProgramRole, currentPermissions);

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the counts for the dashboard navigation
                    var allCounts = context.spGetCountsForDashboardMaster(DateTime.Now, string.Join(",", currentProgramRole.ProgramFKs), 
                                        string.Join(",", currentProgramRole.HubFKs), string.Join(",", currentProgramRole.CohortFKs),
                                        string.Join(",", currentProgramRole.StateFKs), currentProgramRole.CodeProgramRoleFK.Value, HttpContext.Current.User.Identity.Name).FirstOrDefault();

                    //Display the counts
                    spanASQSECount.InnerText = allCounts.ASQSECount.ToString();
                    spanBehaviorIncidentCount.InnerText = allCounts.BehaviorIncidentCount.ToString();
                    spanBOQCount.InnerText = allCounts.BOQCount.ToString();
                    spanBOQFCCCount.InnerText = allCounts.BOQFCCCount.ToString();
                    spanChildCount.InnerText = allCounts.ChildrenCount.ToString();
                    spanClassroomCount.InnerText = allCounts.ClassroomCount.ToString();
                    spanCoachingLogCount.InnerText = allCounts.CoachingLogCount.ToString();
                    spanEmployeeCount.InnerText = allCounts.EmployeeCount.ToString();
                    spanOtherSEScreenCount.InnerText = allCounts.OtherSEScreenCount.ToString();
                    spanTPITOSCount.InnerText = allCounts.TPITOSCount.ToString();
                    spanTPOTCount.InnerText = allCounts.TPOTCount.ToString();
                    spanFileUploadCount.InnerText = allCounts.FileUploadCount.ToString();
                }
            }

            //To hold the body width
            double bodyWidth = 0.00;

            //Try to get the body width
            if(double.TryParse(Utilities.GetSessionValue(Utilities.SessionKey.BODY_WIDTH), out bodyWidth))
            {
                //Use mobile mode for the report viewer if the body is under 1200 pixels wide
                if (bodyWidth < 1200)
                {
                    //Get the new height for the report viewer (sized for an 8x11 paper)
                    double newHeight = bodyWidth * 1.18;

                    //Use mobile mode
                    formReportViewer.MobileMode = true;

                    //Set the report viewer height
                    formReportViewer.Height = Convert.ToInt32(newHeight);
                }
                else
                {
                    formReportViewer.MobileMode = false;
                }
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
                new {LinkDiv = divASQSEDashboardLink, FormAbbreviations = new string[] { "ASQSE" } },
                new {LinkDiv = divBIRDashboardLink, FormAbbreviations = new string[] { "BIR" } },
                new {LinkDiv = divBOQDashboardLink, FormAbbreviations = new string[] { "BOQ" } },
                new {LinkDiv = divBOQFCCDashboardLink, FormAbbreviations = new string[] { "BOQFCC" } },
                new {LinkDiv = divCCLDashboardLink, FormAbbreviations = new string[] { "CCL" } },
                new {LinkDiv = divChildDashboardLink, FormAbbreviations = new string[] { "CHILD" } },
                new {LinkDiv = divClassDashboardLink, FormAbbreviations = new string[] { "CLASS" } },
                new {LinkDiv = divLCDashboardLink, FormAbbreviations = new string[] { "LCL", "LCPS", "LCHS", "LCPN", "LCHN", "LCPD", "LCHD", "LCCS", "LCCD" } },
                new {LinkDiv = divMCDashboardLink, FormAbbreviations = new string[] { "MCTT", "MCTD" } },
                new {LinkDiv = divOSESDashboardLink, FormAbbreviations = new string[] { "OSES" } },
                new {LinkDiv = divPEDashboardLink, FormAbbreviations = new string[] { "PE" } },
                new {LinkDiv = divCWLTDashboardLink, FormAbbreviations = new string[] { "CWLTAP", "BOQCWLT", "CWLTM", "CWLTA", "CWLTAT" } },
                new {LinkDiv = divPLTDashboardLink, FormAbbreviations = new string[] { "PAP", "PAPFCC", "PLTM", "PA"  } },
                new {LinkDiv = divSLTDashboardLink, FormAbbreviations = new string[] { "SLTAP", "BOQSLT", "SLTM", "SLTA", "SLTWG" } },
                new {LinkDiv = divTPOTDashboardLink, FormAbbreviations = new string[] { "TPOT" } },
                new {LinkDiv = divTPITOSDashboardLink, FormAbbreviations = new string[] { "TPITOS" } },
                new {LinkDiv = divULFDashboardLink, FormAbbreviations = new string[] { "ULF" } }
            }.ToList();

            //Loop through all the links
            foreach(var link in links)
            {
                //Determine if the user is allowed to view the dashboard for ANY of the forms inside the dashboard
                if(permissions.Where(p => link.FormAbbreviations.Contains(p.CodeForm.FormAbbreviation) && p.AllowedToViewDashboard == true).Count() > 0)
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
            if (divBOQFCCDashboardLink.Visible &&
                programRole.ShowBOQFCC.HasValue &&
                programRole.ShowBOQFCC.Value)
            {
                divBOQFCCDashboardLink.Visible = true;
            }
            else
            {
                divBOQFCCDashboardLink.Visible = false;
            }

            //Show/hide the BOQ link
            if (divBOQDashboardLink.Visible &&
                programRole.ShowBOQ.HasValue &&
                programRole.ShowBOQ.Value)
            {
                divBOQDashboardLink.Visible = true;
            }
            else
            {
                divBOQDashboardLink.Visible = false;
            }
        }

        /// <summary>
        /// Hide the master page title so that the content page can create a custom one
        /// </summary>
        public void HideTitle()
        {
            divMasterPageTitle.Visible = false;
        }

        /// <summary>
        /// This method displays the passed report
        /// </summary>
        /// <param name="report">The XtraReport to fill with parameter values</param>
        /// <param name="reportTitle">The title for the report</param>
        /// <param name="formPK">The PK of the form</param>
        public void DisplayReport(ProgramAndRoleFromSession programRole, RptFormReportMaster report, string reportTitle, int formPK)
        {
            //Display the modal
            hfShowReportModal.Value = "show";

            //Set the state name and catchphrase
            if (programRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                programRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
            {
                report.lblStateName.Text = programRole.StateName;
            }
            else
            {
                report.lblStateName.Text = programRole.StateName + " State";
            }
            report.lblStateCatchphrase.Text = programRole.StateCatchphrase;

            //Set the report title
            report.lblReportTitle.Text = reportTitle;

            //Set the logo url
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            report.ParamLogoPath.Value = baseUrl + "Content/images/" + programRole.StateThumbnailLogoFileName;

            //Set the visibility of information
            report.ParamViewPrivateChildInfo.Value = programRole.ViewPrivateChildInfo.Value;
            report.ParamViewPrivateEmployeeInfo.Value = programRole.ViewPrivateEmployeeInfo.Value;
            
            //Add the form PK parameter
            report.Parameters["ParamFormPK"].Value = formPK;

            //Display the report
            formReportViewer.OpenReport(report);

            //Update the modal update panel
            upFormReport.Update();
        }
    }
}