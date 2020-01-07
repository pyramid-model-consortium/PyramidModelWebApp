using System;
using System.Web;
using System.Web.UI;
using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using Pyramid.Code;
using Pyramid.Models;

namespace Pyramid.Account
{
    public partial class CreatePasswordLink : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Set the focus to the username field
                txtUsername.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Send Link button and it
        /// sends the user a reset password link via Email
        /// </summary>
        /// <param name="sender">The btnSendCreateLink DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnSendCreateLink_Click(object sender, EventArgs e)
        {
            //Only continue if the page is valid
            if (ASPxEdit.AreEditorsValid(this, btnSendCreateLink.ValidationGroup))
            {
                // Validate the user's email address
                var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                PyramidUser user = manager.FindByName(txtUsername.Text);

                if (user == null || !manager.IsEmailConfirmed(user.Id))
                {
                    msgSys.ShowMessageToUser("warning", "Email Failure", "The user either does not exist or is not confirmed.", 25000);
                }
                else
                {
                    //Send the reset link to the user
                    string code = manager.GeneratePasswordResetToken(user.Id);
                    string callbackUrl = IdentityHelper.GetCreatePasswordRedirectUrl(code, Request);
                    manager.SendEmail(user.Id, "Create your password", Utilities.GetEmailHTML(callbackUrl, "Create Password", true, "Password Creation Requested", "Please create your password by clicking the Create Password link below.", Request));

                    //Show the email sent div and hide the create div
                    divEmailSent.Visible = true;
                    divCreate.Visible = false;
                }
            }
        }
    }
}