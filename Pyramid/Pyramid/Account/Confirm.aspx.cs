using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using Pyramid.Models;

namespace Pyramid.Account
{
    public partial class Confirm : Page
    {
        protected string StatusMessage
        {
            get;
            private set;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the code, user ID, and mode from the query string
            string code = IdentityHelper.GetCodeFromRequest(Request);
            string userId = IdentityHelper.GetUserIdFromRequest(Request);
            string mode = IdentityHelper.GetConfirmModeFromRequest(Request);

            //Make sure the code and user ID exist
            if (code != null && userId != null)
            {
                //To hold the confirmation result
                IdentityResult result = new IdentityResult();

                using(ApplicationDbContext context = new ApplicationDbContext())
                {
                    //Try to confirm the email
                    var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    result = manager.ConfirmEmail(userId, code);

                    //Set the edit fields for the user
                    PyramidUser userRecord = context.Users.Where(u => u.Id == userId).FirstOrDefault();

                    //Ensure the user record exists
                    if(userRecord != null)
                    {
                        //Set the edit fields
                        userRecord.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                        userRecord.UpdateTime = DateTime.Now;

                        //Save the changes
                        context.SaveChanges();
                    }
                }

                //Check to see if the email was confirmed
                if (result.Succeeded)
                {
                    //The email was confirmed
                    //Show the success panel
                    successPanel.Visible = true;

                    if(mode == "AccountConfirm")
                    {
                        //Set the page title
                        Page.Title = "Account Confirmed";

                        //Show the proper div
                        divAccountConfirm.Visible = true;
                        divEmailConfirm.Visible = false;
                    }
                    else if(mode == "EmailConfirm")
                    {
                        //Set the page title
                        Page.Title = "Email Confirmed";

                        //Show the proper div
                        divAccountConfirm.Visible = false;
                        divEmailConfirm.Visible = true;
                    }
                }
                else
                {
                    //The email was not confirmed, show the error panel
                    successPanel.Visible = false;
                    errorPanel.Visible = true;
                }
            }
            else
            {
                //Show the error panel
                successPanel.Visible = false;
                errorPanel.Visible = true;
            }
        }
    }
}