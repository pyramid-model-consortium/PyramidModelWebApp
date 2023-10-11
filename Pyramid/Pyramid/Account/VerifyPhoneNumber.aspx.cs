using System;
using System.Text.RegularExpressions;
using System.Web;
using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Code;
using Pyramid.Models;

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

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    //Set the hidden field value
                    hfPhoneNumber.Value = phoneNumber;

                    //Set the phone number label
                    lblPhoneNum.Text = Regex.Replace(phoneNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3");

                    //Set the focus to the code field
                    txtCode.Focus();
                }
                else
                {
                    //Failed, redirect the user
                    Response.Redirect("/Account/Manage?m=VerifyPhoneNumberFailed");
                }
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
            if (ASPxEdit.AreEditorsValid(this, btnVerifyCode.ValidationGroup))
            {
                //Get the user manager
                ApplicationUserManager manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //Get the user
                PyramidUser currentUser = manager.FindById(User.Identity.GetUserId());

                //Ensure the user exists
                if(currentUser != null)
                {
                    //Try to change the phone number
                    IdentityResult result = manager.ChangePhoneNumber(currentUser.Id, hfPhoneNumber.Value, txtCode.Text);

                    //Set the edit fields
                    currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                    currentUser.UpdateTime = DateTime.Now;
                    manager.Update(currentUser);

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
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "No user found!", 120000);
                }
            }
            else
            {
                //Validation failed, show a message
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the resend code button and it
        /// send the code to the user again
        /// </summary>
        /// <param name="sender">The btnResendCode BootstrapButton</param>
        /// <param name="e">The Click event</param>
        protected void btnResendCode_Click(object sender, EventArgs e)
        {
            //Get the user manager
            ApplicationUserManager userManager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Get the current user
            PyramidUser currentUser = userManager.FindById(User.Identity.GetUserId());

            //Generate the phone number change code
            string code = userManager.GenerateChangePhoneNumberToken(currentUser.Id, currentUser.PhoneNumber);

            //Show the warning div
            divWarning.Visible = true;

            //Ensure that the SMS service is available and the phone number exists
            if (userManager.SmsService != null && !string.IsNullOrWhiteSpace(currentUser.PhoneNumber))
            {
                //Create the message with the security code in it
                var message = new IdentityMessage
                {
                    Destination = currentUser.PhoneNumber,
                    Body = "Your Pyramid Model Implementation Data System security code is: " + code
                };

                //Send the code to the user
                userManager.SmsService.Send(message);

                //Show a success message
                msgSys.ShowMessageToUser("success", "Code Sent", string.Format("The verification code has been sent to {0}.", Regex.Replace(currentUser.PhoneNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3")), 15000);
            }
            else
            {
                //Show a failure message
                msgSys.ShowMessageToUser("danger", "Error", "Could not send the verification code!  Please try again, and if it continues to fail, contact support.", 20000);
            }
        }
    }
}