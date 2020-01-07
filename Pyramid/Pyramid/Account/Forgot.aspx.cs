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
    public partial class ForgotPassword : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                //Set the focus to the username field
                txtUsername.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Send Link button and it
        /// sends the user a reset password link via Email
        /// </summary>
        /// <param name="sender">The btnSendForgotLink DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnSendForgotLink_Click(object sender, EventArgs e)
        {
            //Only continue if the page is valid
            if (ASPxEdit.AreEditorsValid(this, btnSendForgotLink.ValidationGroup))
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
                    string callbackUrl = IdentityHelper.GetResetPasswordRedirectUrl(code, Request);
                    manager.SendEmail(user.Id, "Reset your password", Utilities.GetEmailHTML(callbackUrl, "Reset Password", true, "Password Reset Requested", "Please reset your password by clicking the Reset Password link below.", Request));

                    //Show the email sent div and hide the forgot div
                    divEmailSent.Visible = true;
                    divForgot.Visible = false;
                }
            }
        }
    }
}