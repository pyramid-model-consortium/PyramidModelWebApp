using Microsoft.AspNet.Identity;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Pyramid
{
    public partial class NotLoggedIn : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                //Display/hide the test site message
                divTestSiteMessage.Visible = Utilities.IsTestSite();
            }
        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
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