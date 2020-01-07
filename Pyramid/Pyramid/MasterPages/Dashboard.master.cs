using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Linq;

namespace Pyramid.MasterPages
{
    public partial class Dashboard : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Get the current program role
                ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

                //Show/hide the BOQFCC link
                if(currentProgramRole.ShowBOQFCC.HasValue && currentProgramRole.ShowBOQFCC.Value)
                {
                    divBOQFCCDashboardLink.Visible = true;
                }
                else
                {
                    divBOQFCCDashboardLink.Visible = false;
                }

                //Show/hide the BOQ link
                if(currentProgramRole.ShowBOQ.HasValue && currentProgramRole.ShowBOQ.Value)
                {
                    divBOQDashboardLink.Visible = true;
                }
                else
                {
                    divBOQDashboardLink.Visible = false;
                }

                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                using(PyramidContext context = new PyramidContext())
                {
                    //Get all the counts for the dashboard navigation
                    var allCounts = context.spGetCountsForDashboardMaster(DateTime.Now, string.Join(",", currentProgramRole.ProgramFKs), 
                                        currentProgramRole.HubFK, currentProgramRole.StateFK).FirstOrDefault();

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
        }

        /// <summary>
        /// Hide the master page title so that the content page can create a custom one
        /// </summary>
        public void HideTitle()
        {
            divMasterPageTitle.Visible = false;
        }
    }
}