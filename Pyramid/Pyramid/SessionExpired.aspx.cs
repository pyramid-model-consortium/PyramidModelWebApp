using Microsoft.AspNet.Identity;
using Pyramid.Models;
using System;
using System.Web;

namespace Pyramid
{
    public partial class SessionExpired : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
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
                    history.LogoutType = "Session timeout expired and user was logged out automatically.";
                    context.SaveChanges();
                }
            }

            //Ensure that the user's session is clear
            Session.Abandon();
        }
    }
}