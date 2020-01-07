using System;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Text;

namespace Pyramid.Account
{
    public partial class Manage : System.Web.UI.Page
    {
        protected string SuccessMessage
        {
            get;
            private set;
        }

        protected PyramidUser CurrentUser
        {
            get;
            private set;
        }

        private ApplicationUserManager userManager;

        protected void Page_Load()
        {
            //Get the user manager
            userManager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Get the current user
            CurrentUser = userManager.FindById(User.Identity.GetUserId());

            if (!IsPostBack)
            {
                //Fill the text boxes
                txtPhoneNumber.Value = CurrentUser.PhoneNumber;
                txtEmail.Text = CurrentUser.Email;

                // Render success message
                var message = Request.QueryString["m"];
                if (message != null)
                {
                    // Strip the query string from action
                    Form.Action = ResolveUrl("~/Account/Manage");

                    SuccessMessage =
                        message == "ChangePwdSuccess" ? "Your password has been successfully changed!"
                        : message == "SetPwdSuccess" ? "Your password has been successfully set!"
                        : message == "RemoveLoginSuccess" ? "The account was successfully removed!"
                        : message == "AddPhoneNumberSuccess" ? "Phone number has been successfully added!"
                        : message == "VerifyPhoneNumberSuccess" ? "Phone number has been successfully verified!"
                        : message == "RemovePhoneNumberSuccess" ? "Phone number was successfully removed!"
                        : String.Empty;

                    //Show the message
                    msgSys.ShowMessageToUser("success", "Success", SuccessMessage, 15000);
                }

                //Show or hide the two-factor buttons
                ShowHideTwoFactorButtons(CurrentUser.TwoFactorEnabled);

                //Show or hide the phone buttons
                ShowHidePhoneButtons(CurrentUser.PhoneNumber, CurrentUser.PhoneNumberConfirmed);

                using(PyramidContext context = new PyramidContext())
                {
                    //Get the user's selected customization options
                    List<spGetUserCustomizationOptions_Result> selectedOptions = context.spGetUserCustomizationOptions(CurrentUser.UserName).ToList();

                    //Fill and set the user customization option dropdowns

                    //-----------------  Fireworks  -------------------------
                    List<CodeCustomizationOptionValue> fireworkOptions = context.CodeCustomizationOptionValue.AsNoTracking()
                                            .Include(ccov => ccov.CodeCustomizationOptionType)
                                            .Where(ccov => ccov.CodeCustomizationOptionType.Description.ToLower() == "fireworks")
                                            .OrderBy(ccov => ccov.OrderBy)
                                            .ToList();

                    ddFireworks.DataSource = fireworkOptions;
                    ddFireworks.DataBind();

                    //Set the selected value
                    int fireworksOption = selectedOptions.Where(so => so.OptionTypeDescription.ToLower() == "fireworks").Select(so => so.OptionValuePK).FirstOrDefault().GetValueOrDefault();
                    ddFireworks.SelectedItem = ddFireworks.Items.FindByValue(fireworksOption);
                    //-----------------  End Fireworks  -------------------------
                }
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// This method shows/hides the buttons related to two-factor
        /// authentication based on the boolean value passed to it
        /// </summary>
        /// <param name="twoFactorEnabled">A boolean that is true if two-factor is enabled and false otherwise</param>
        private void ShowHideTwoFactorButtons(bool twoFactorEnabled)
        {
            //Show or hide the Two-Factor buttons
            if (twoFactorEnabled)
            {
                lbEnableTwoFactor.Visible = false;
                lbDisableTwoFactor.Visible = true;
            }
            else
            {
                lbEnableTwoFactor.Visible = true;
                lbDisableTwoFactor.Visible = false;
            }
        }

        /// <summary>
        /// This method shows/hides the buttons related to the user's phone
        /// depending on what values are passed to it
        /// </summary>
        /// <param name="phoneNumber">The user's phone number</param>
        /// <param name="confirmed">A boolean that is true if the user's phone is confirmed and false otherwise</param>
        private void ShowHidePhoneButtons(string phoneNumber, bool confirmed)
        {
            //Check to see if the user has a phone number
            if(String.IsNullOrWhiteSpace(phoneNumber))
            {
                //The user does not have a phone number, only show the add button
                btnAddPhone.Visible = true;
                btnEditPhone.Visible = false;
                btnRemovePhone.Visible = false;
                btnVerifyPhone.Visible = false;
            }
            else
            {
                //The user has a phone number, hide the add button and show the edit and remove buttons
                btnAddPhone.Visible = false;
                btnEditPhone.Visible = true;
                btnRemovePhone.Visible = true;

                //Only show the verify button if the phone number is not confirmed
                if (confirmed)
                    btnVerifyPhone.Visible = false;
                else
                    btnVerifyPhone.Visible = true;
            }
        }

        // Remove phone number from user
        protected void btnRemovePhone_Click(object sender, EventArgs e)
        {
            var result = userManager.SetPhoneNumber(CurrentUser.Id, null);

            if (result.Succeeded == true)
            {
                //Clear the phone text box
                txtPhoneNumber.Value = "";

                //Set the button visibility
                ShowHidePhoneButtons(CurrentUser.PhoneNumber, CurrentUser.PhoneNumberConfirmed);

                //Give the user a success message
                msgSys.ShowMessageToUser("success", "Success", "Phone number successfully removed!", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }

        }
        
        /// <summary>
        /// This method fires when the user clicks a button to disable two-factor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TwoFactorDisable_Click(object sender, EventArgs e)
        {
            //Disable two-factor
            IdentityResult result = userManager.SetTwoFactorEnabled(CurrentUser.Id, false);

            if (result.Succeeded)
            {
                //Set the display of the two-factor buttons
                ShowHideTwoFactorButtons(false);

                //Give the user a success message
                msgSys.ShowMessageToUser("success", "Success", "Two-Factor Authentication successfully disabled!", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }
        }
        
        /// <summary>
        /// This method fires when the user clicks a button to enable two-factor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TwoFactorEnable_Click(object sender, EventArgs e)
        {
            //Enable two-factor
            IdentityResult result = userManager.SetTwoFactorEnabled(CurrentUser.Id, true);

            if (result.Succeeded)
            {
                //Set the display of the two-factor buttons
                ShowHideTwoFactorButtons(true);

                //Give the user a success message
                msgSys.ShowMessageToUser("success", "Success", "Two-Factor Authentication successfully enabled!", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the btnVerifyPhone DevExpress button
        /// and it will allow the user to verify their phone number via SMS code.
        /// </summary>
        /// <param name="sender">The btnVerifyPhone DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void btnVerifyPhone_Click(object sender, EventArgs e)
        {
            //Generate the phone number change code
            string code = userManager.GenerateChangePhoneNumberToken(CurrentUser.Id, CurrentUser.PhoneNumber);

            //Ensure that the SMS service is available
            if (userManager.SmsService != null)
            {
                //Create the message with the security code in it
                var message = new IdentityMessage
                {
                    Destination = CurrentUser.PhoneNumber,
                    Body = "Your Pyramid Model Implementation Data System security code is: " + code
                };

                //Send the code to the user
                userManager.SmsService.Send(message);
            }

            //Send the user to the verify page
            Response.Redirect("/Account/VerifyPhoneNumber?PhoneNumber=" + HttpUtility.UrlEncode(CurrentUser.PhoneNumber));
        }

        /// <summary>
        /// This method fires when the user edits their phone number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EditPhone_Click(object sender, EventArgs e)
        {
            //Set the user's phone number
            IdentityResult result = userManager.SetPhoneNumber(CurrentUser.Id, txtPhoneNumber.Value.ToString());

            //Set the display of the buttons
            ShowHidePhoneButtons(CurrentUser.PhoneNumber, CurrentUser.PhoneNumberConfirmed);

            if (result.Succeeded)
            {
                msgSys.ShowMessageToUser("success", "Success", "Phone number successfully changed!", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }
        }

        /// <summary>
        /// This method fires when the user is editing their email address
        /// </summary>
        /// <param name="sender">The lbConfirmEmailChange LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbConfirmEmailChange_Click(object sender, EventArgs e)
        {
            //Set the user's email
            IdentityResult result = userManager.SetEmail(CurrentUser.Id, txtEmail.Text);

            if (result.Succeeded)
            {
                //Generate the user confirmation url
                string code = userManager.GenerateEmailConfirmationToken(CurrentUser.Id);
                string callbackUrl = IdentityHelper.GetEmailConfirmationRedirectUrl(code, CurrentUser.Id, Request);

                //Send the confirmation email to the user
                userManager.SendEmail(CurrentUser.Id, "Confirm your email address change", Utilities.GetEmailHTML(callbackUrl, "Confirm Email", true, "Email Updated", "Please confirm your email address change by clicking the Confirm Email link below.", Request));

                //Give the user a message
                msgSys.ShowMessageToUser("success", "Success", "Email updated, you should receive an email soon to confirm the change.", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitCustomizationOptions user control 
        /// </summary>
        /// <param name="sender">The submitCustomizationOptions control</param>
        /// <param name="e">The Click event</param>
        protected void submitCustomizationOptions_Click(object sender, EventArgs e)
        {
            try {
                List<spGetUserCustomizationOptions_Result> userCustomizationOptions = new List<spGetUserCustomizationOptions_Result>();
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the user's customization options
                    userCustomizationOptions = context.spGetUserCustomizationOptions(CurrentUser.UserName).ToList();

                    //Get the selected fireworks option
                    spGetUserCustomizationOptions_Result selectedFireworksOption = userCustomizationOptions
                                                .Where(uco => uco.OptionTypeDescription.ToLower() == "fireworks")
                                                .FirstOrDefault();

                    //Update the customization options

                    //------------------ Fireworks --------------------------
                    UserCustomizationOption fireworksOption;
                    if (selectedFireworksOption.UserCustomizationOptionPK.HasValue
                            && selectedFireworksOption.UserCustomizationOptionPK.Value > 0) {

                        //Edit the fireworks option
                        fireworksOption = context.UserCustomizationOption
                                                                    .Where(uco => uco.UserCustomizationOptionPK == selectedFireworksOption.UserCustomizationOptionPK.Value)
                                                                    .FirstOrDefault();

                        fireworksOption.Editor = User.Identity.Name;
                        fireworksOption.EditDate = DateTime.Now;
                        fireworksOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddFireworks.Value);
                        context.SaveChanges();
                    }
                    else
                    {
                        //Create the fireworks option
                        fireworksOption = new UserCustomizationOption();
                        fireworksOption.Creator = User.Identity.Name;
                        fireworksOption.CreateDate = DateTime.Now;
                        fireworksOption.Username = CurrentUser.UserName;
                        fireworksOption.CustomizationOptionTypeCodeFK = selectedFireworksOption.OptionTypePK;
                        fireworksOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddFireworks.Value);
                        context.UserCustomizationOption.Add(fireworksOption);
                        context.SaveChanges();
                    }
                    //------------------ End Fireworks --------------------------


                    //Refresh the user's customization options
                    userCustomizationOptions = context.spGetUserCustomizationOptions(CurrentUser.UserName).ToList();
                }

                //Set the customization options cookie
                bool isCookieSaved = Utilities.SetCustomizationOptionCookie(userCustomizationOptions);

                //Check to see if the cookie saved
                if (isCookieSaved)
                {
                    //Show a success message
                    msgSys.ShowMessageToUser("success", "Options Saved", "The customization options have been saved!", 10000);
                }
                else
                {
                    //Tell the user it failed
                    msgSys.ShowMessageToUser("warning", "Save Failed", "The customization options failed to save.", 10000);
                }
            }
            catch (Exception ex)
            {
                //Log any exceptions
                Utilities.LogException(ex);

                //Tell the user it failed
                msgSys.ShowMessageToUser("warning", "Save Failed", "The customization options failed to save.", 10000);
            }
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCustomizationOptions control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCustomizationOptions_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }
    }
}