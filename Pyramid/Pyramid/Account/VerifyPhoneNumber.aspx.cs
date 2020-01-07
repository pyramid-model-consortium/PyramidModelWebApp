using System;
using System.Web;
using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Code;

namespace Pyramid.Account
{
    public partial class VerifyPhoneNumber : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Get the user manager
                ApplicationUserManager manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //Get the phone number from the query string
                string phoneNumber = Request.QueryString["PhoneNumber"];

                //Generate the phone verification code
                string code = manager.GenerateChangePhoneNumberToken(User.Identity.GetUserId(), phoneNumber);

                //Set the hidden field value
                hfPhoneNumber.Value = phoneNumber;

                //Set the focus to the code field
                txtCode.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Verify Code button
        /// and it tries to verify the code that the user put in
        /// </summary>
        /// <param name="sender">The btnVerifyCode DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnVerifyCode_Click(object sender, EventArgs e)
        {
            //Only continue if the validation is successful
            if (ASPxEdit.AreEditorsValid(this, "vgVerifyCode"))
            {
                //Get the user manager
                ApplicationUserManager manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //Try to change the phone number
                IdentityResult result = manager.ChangePhoneNumber(User.Identity.GetUserId(), hfPhoneNumber.Value, txtCode.Text);

                //Check to see if the change succeeded
                if (result.Succeeded)
                {
                    //Succeeded, redirect the user
                    Response.Redirect("/Account/Manage?m=VerifyPhoneNumberSuccess");
                }
                else
                {
                    //Failed, show error message
                    msgSys.ShowMessageToUser("warning", "Verification Failed", "Failed to verify the phone.  Please check the code and try again.", 10000);
                }
            }
        }
    }
}