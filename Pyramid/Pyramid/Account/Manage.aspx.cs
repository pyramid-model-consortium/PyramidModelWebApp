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
using DevExpress.Web;

namespace Pyramid.Account
{
    public partial class Manage : System.Web.UI.Page
    {
        private PyramidUser currentUser;
        private ApplicationUserManager userManager;
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load()
        {
            //Get the user manager
            userManager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Get the current user
            currentUser = userManager.FindById(User.Identity.GetUserId());

            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            if (!IsPostBack)
            {
                //Hide the div if the state is not NY
                if (currentProgramRole.CurrentStateFK.Value != (int)Utilities.StateFKs.NEW_YORK)
                {
                    divAccountUpdateAlert.Visible = false;
                }
                else
                {
                    //Try to convert the date from session. If TryParse returns false then the div remains shown since the user has no UpdateTime.
                    DateTime updateTime;
                    if (Session["UserLastUpdateDate"] != null && DateTime.TryParse(Session["UserLastUpdateDate"].ToString(), out updateTime) == true)
                    {
                        //If the update time is within 6 months hide the alert
                        if (DateTime.Today.AddMonths(-6) < updateTime)
                        {
                            divAccountUpdateAlert.Visible = false;
                        }
                    }
                }
                //Bind the state dropdown
                BindStateDropdown();

                //Fill the name labels
                txtFirstName.Value = currentUser.FirstName;
                txtLastName.Value = currentUser.LastName;

                //Fill the text boxes
                txtStreet.Value = currentUser.Street;
                txtCity.Value = currentUser.City;
                ddState.SelectedItem = ddState.Items.FindByValue(currentUser.State);
                txtZIPCode.Value = currentUser.ZIPCode;
                txtRegionLocation.Value = currentUser.RegionLocation;
                txtPhoneNumber.Value = currentUser.PhoneNumber;
                txtWorkPhoneNumber.Value = currentUser.WorkPhoneNumber;
                txtEmail.Value = currentUser.Email;

                // Render success message
                var message = Request.QueryString["m"];
                if (message != null)
                {
                    // Strip the query string from action
                    Form.Action = ResolveUrl("~/Account/Manage");

                    //To hold the message text, type, and title
                    string messageText, messageType, messageTitle;

                    //Determine the message text and type
                    switch (message)
                    {
                        case "ChangePwdSuccess":
                            messageText = "Your password has been successfully changed!";
                            messageType = "success";
                            messageTitle = "Password Changed";
                            break;
                        case "SetPwdSuccess":
                            messageText = "Your password has been successfully set!";
                            messageType = "success";
                            messageTitle = "Password Set";
                            break;
                        case "RemoveLoginSuccess":
                            messageText = "The account was successfully removed!";
                            messageType = "success";
                            messageTitle = "Login Removed";
                            break;
                        case "AddPhoneNumberSuccess":
                            messageText = "Phone number has been successfully added!";
                            messageType = "success";
                            messageTitle = "Phone Added";
                            break;
                        case "VerifyPhoneNumberSuccess":
                            messageText = "Phone number has been successfully verified!";
                            messageType = "success";
                            messageTitle = "Phone Verified";
                            break;
                        case "VerifyPhoneNumberFailed":
                            messageText = "Phone number verification failed!  Please try again, and if it continues to fail, contact support.";
                            messageType = "danger";
                            messageTitle = "Phone Verification Failed";
                            break;
                        case "RemovePhoneNumberSuccess":
                            messageText = "Phone number was successfully removed!";
                            messageType = "success";
                            messageTitle = "Phone Removed";
                            break;
                        default:
                            messageText = null;
                            messageType = null;
                            messageTitle = null;
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(messageText))
                    {
                        //Show the message
                        msgSys.ShowMessageToUser(messageType, messageTitle, messageText, 25000);
                    }
                }

                //Show the last login date
                DisplayLastLogin(currentUser.UserName);

                //Show or hide the two-factor buttons
                ShowHideTwoFactorButtons(currentUser.TwoFactorEnabled);

                //Show or hide the phone buttons
                ShowHidePhoneButtons(currentUser.PhoneNumber, currentUser.PhoneNumberConfirmed);
                ShowHideWorkPhoneButtons(currentUser.WorkPhoneNumber);

                //Bind the customization options
                BindCustomizationOptions(currentUser.UserName);
            }
        }

        /// <summary>
        /// This method displays the last login date for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        private void DisplayLastLogin(string username)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //-----------------  Last Login  -------------------------
                //To hold the current login PK
                int currentLoginPK = 0;

                //Record the logout if a record for the login existed
                if (Session["LoginHistoryPK"] != null && !String.IsNullOrWhiteSpace(Session["LoginHistoryPK"].ToString()))
                {
                    //Get the login history pk from session
                    currentLoginPK = Convert.ToInt32(Session["LoginHistoryPK"].ToString());
                }

                //Get the last login information
                LoginHistory lastLogin = context.LoginHistory.AsNoTracking()
                                            .Where(lh => lh.Username == username && lh.LoginHistoryPK != currentLoginPK)
                                            .OrderByDescending(lh => lh.LoginTime)
                                            .FirstOrDefault();

                //See if there is a last login
                if (lastLogin != null)
                {
                    //Set the last login label
                    lblLastLoginDate.Text = lastLogin.LoginTime.ToString("MM/dd/yyyy hh:mm tt") + " ET";
                }
                //-----------------  End Last Login  -------------------------
            }
        }

        /// <summary>
        /// This method binds the customization options
        /// </summary>
        /// <param name="username">The user's username</param>
        private void BindCustomizationOptions(string username)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the user's selected customization options
                List<spGetUserCustomizationOptions_Result> selectedOptions = context.spGetUserCustomizationOptions(username).ToList();

                //Get all customization options
                List<CodeCustomizationOptionValue> customizationOptions = new List<CodeCustomizationOptionValue>();
                customizationOptions = context.CodeCustomizationOptionValue.AsNoTracking()
                                        .Include(ccov => ccov.CodeCustomizationOptionType)
                                        .OrderBy(ccov => ccov.OrderBy)
                                        .ToList();

                //Fill and set the user customization option dropdowns

                //-----------------  Cancel Confirmation  -------------------------
                List<CodeCustomizationOptionValue> cancelConfirmOptions = customizationOptions
                                        .Where(ccov => ccov.CustomizationOptionTypeCodeFK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.CANCEL_CONFIRMATION)
                                        .OrderBy(ccov => ccov.OrderBy)
                                        .ToList();

                ddCancelConfirmation.DataSource = cancelConfirmOptions;
                ddCancelConfirmation.DataBind();

                //Set the selected value
                int? currentCancelConfirm = selectedOptions.Where(so => so.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.CANCEL_CONFIRMATION)
                                                     .Select(so => so.OptionValuePK).FirstOrDefault();
                if (currentCancelConfirm.HasValue)
                {
                    ddCancelConfirmation.SelectedItem = ddCancelConfirmation.Items.FindByValue(currentCancelConfirm.Value);
                }
                //-----------------  End Cancel Confirmation  -------------------------

                //-----------------  Fireworks  -------------------------
                List<CodeCustomizationOptionValue> fireworkOptions = customizationOptions
                                        .Where(ccov => ccov.CustomizationOptionTypeCodeFK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.FIREWORKS)
                                        .OrderBy(ccov => ccov.OrderBy)
                                        .ToList();

                ddFireworks.DataSource = fireworkOptions;
                ddFireworks.DataBind();

                //Set the selected value
                int? currentFireworksOption = selectedOptions.Where(so => so.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.FIREWORKS)
                                                     .Select(so => so.OptionValuePK).FirstOrDefault();
                if (currentFireworksOption.HasValue)
                {
                    ddFireworks.SelectedItem = ddFireworks.Items.FindByValue(currentFireworksOption.Value);
                }
                //-----------------  End Fireworks  -------------------------

                //-----------------  Welcome message  -------------------------
                List<CodeCustomizationOptionValue> welcomeMessageOptions = customizationOptions
                                        .Where(ccov => ccov.CustomizationOptionTypeCodeFK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.WELCOME_MESSAGE)
                                        .OrderBy(ccov => ccov.OrderBy)
                                        .ToList();

                ddWelcomeMessage.DataSource = welcomeMessageOptions;
                ddWelcomeMessage.DataBind();

                //Set the selected value
                int? currentWelcomeMessageOption = selectedOptions.Where(so => so.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.WELCOME_MESSAGE)
                                                     .Select(so => so.OptionValuePK).FirstOrDefault();
                if (currentWelcomeMessageOption.HasValue)
                {
                    ddWelcomeMessage.SelectedItem = ddWelcomeMessage.Items.FindByValue(currentWelcomeMessageOption.Value);
                }
                //-----------------  End Welcome message  -------------------------
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
            if (string.IsNullOrWhiteSpace(phoneNumber))
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
                {
                    btnVerifyPhone.Visible = false;
                }
                else
                {
                    btnVerifyPhone.Visible = true;
                }
            }
        }
        /// <summary>
        /// This method shows/hides the buttons related to the user's work phone
        /// depending on what values are passed to it
        /// </summary>
        /// <param name="workPhoneNumber">The user's work phone number</param>
        private void ShowHideWorkPhoneButtons(string workPhoneNumber)
        {
            //Check to see if the user has a work phone number
            if (string.IsNullOrWhiteSpace(workPhoneNumber))
            {
                //The user does not have a work phone number, only show the add button
                btnAddWorkPhone.Visible = true;
                btnEditWorkPhone.Visible = false;
                btnRemoveWorkPhone.Visible = false;
            }
            else
            {
                //The user has a work phone number, hide the add button and show the edit and remove buttons
                btnAddWorkPhone.Visible = false;
                btnEditWorkPhone.Visible = true;
                btnRemoveWorkPhone.Visible = true;
            }
        }

        // Remove phone number from user
        protected void btnRemovePhone_Click(object sender, EventArgs e)
        {
            //Remove the phone number
            var result = userManager.SetPhoneNumber(currentUser.Id, null);

            //Set the edit fields
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

            //Set the update time in session and hide the account update alert div
            Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
            divAccountUpdateAlert.Visible = false;


            if (result.Succeeded == true)
            {
                //Clear the phone text box
                txtPhoneNumber.Value = "";

                //Set the button visibility
                ShowHidePhoneButtons(currentUser.PhoneNumber, currentUser.PhoneNumberConfirmed);

                //Give the user a success message
                msgSys.ShowMessageToUser("success", "Success", "Phone number successfully removed!", 10000);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
            }
        }

        // Remove work phone number from user
        protected void btnRemoveWorkPhone_Click(object sender, EventArgs e)
        {
            //Set the fields
            currentUser.WorkPhoneNumber = null;
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

            //Set the update time in session and hide the account update alert div
            Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
            divAccountUpdateAlert.Visible = false;

            //Clear the phone text box
            txtWorkPhoneNumber.Value = "";

            //Set the button visibility
            ShowHideWorkPhoneButtons(currentUser.WorkPhoneNumber);

            //Give the user a success message
            msgSys.ShowMessageToUser("success", "Success", "Work phone number successfully removed!", 10000);
        }

        /// <summary>
        /// This method fires when the user clicks a button to disable two-factor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TwoFactorDisable_Click(object sender, EventArgs e)
        {
            //Disable two-factor
            IdentityResult result = userManager.SetTwoFactorEnabled(currentUser.Id, false);

            //Set the edit fields
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

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
            IdentityResult result = userManager.SetTwoFactorEnabled(currentUser.Id, true);

            //Set the edit fields
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

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
            if (ASPxEdit.AreEditorsValid(this.Page, btnVerifyPhone.ValidationGroup))
            {
                //Generate the phone number change code
                string code = userManager.GenerateChangePhoneNumberToken(currentUser.Id, currentUser.PhoneNumber);

                //Ensure that the SMS service is available
                if (userManager.SmsService != null)
                {
                    //Create the message with the security code in it
                    var message = new IdentityMessage
                    {
                        Destination = currentUser.PhoneNumber,
                        Body = "Your Pyramid Model Implementation Data System security code is: " + code
                    };

                    //Send the code to the user
                    userManager.SmsService.Send(message);

                    //Send the user to the verify page
                    Response.Redirect("/Account/VerifyPhoneNumber?PhoneNumber=" + HttpUtility.UrlEncode(currentUser.PhoneNumber));
                }
                else
                {
                    //Show a failure message
                    msgSys.ShowMessageToUser("danger", "Error", "Could not send the verification code!  Please try again, and if it continues to fail, contact support.", 20000);
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method fires when the user edits their phone number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EditPhone_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this.Page, btnEditPhone.ValidationGroup))
            {
                //Set the user's phone number
                IdentityResult result = userManager.SetPhoneNumber(currentUser.Id, txtPhoneNumber.Value.ToString());

                //Set the edit fields
                currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                currentUser.UpdateTime = DateTime.Now;
                userManager.Update(currentUser);

                //Set the update time in session and hide the account update alert div
                Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
                divAccountUpdateAlert.Visible = false;

                //Set the display of the buttons
                ShowHidePhoneButtons(currentUser.PhoneNumber, currentUser.PhoneNumberConfirmed);

                if (result.Succeeded)
                {
                    msgSys.ShowMessageToUser("success", "Success", "Phone number successfully changed!", 10000);
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method fires when the user edits their work phone number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EditWorkPhone_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this.Page, btnEditWorkPhone.ValidationGroup))
            {
                //Set the fields
                currentUser.WorkPhoneNumber = txtWorkPhoneNumber.Text;
                currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                currentUser.UpdateTime = DateTime.Now;
                userManager.Update(currentUser);

                //Set the update time in session and hide the account update alert div
                Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
                divAccountUpdateAlert.Visible = false;

                //Set the display of the buttons
                ShowHideWorkPhoneButtons(currentUser.WorkPhoneNumber);

                //Show a message
                msgSys.ShowMessageToUser("success", "Success", "Work phone number successfully changed!", 10000);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPhoneNumber_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtWorkPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtWorkPhoneNumber_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtWorkPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtWorkPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }

        /// <summary>
        /// This method fires when the user is editing their email address
        /// </summary>
        /// <param name="sender">The lbConfirmEmailChange LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbConfirmEmailChange_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this.Page, btnEditWorkPhone.ValidationGroup))
            {
                //Set the user's email
                IdentityResult result = userManager.SetEmail(currentUser.Id, txtEmail.Text);

                //Set the edit fields
                currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                currentUser.UpdateTime = DateTime.Now;
                userManager.Update(currentUser);

                //Set the update time in session and hide the account update alert div
                Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
                divAccountUpdateAlert.Visible = false;

                if (result.Succeeded)
                {
                    //Generate the user confirmation url
                    string code = userManager.GenerateEmailConfirmationToken(currentUser.Id);
                    string callbackUrl = IdentityHelper.GetEmailConfirmationRedirectUrl(code, currentUser.Id, Request);

                    //Send the confirmation email to the user
                    userManager.SendEmail(currentUser.Id, "Confirm your email address change",
                            Utilities.GetEmailHTML(callbackUrl, "Confirm Email", true, "Email Updated",
                                "Please confirm your email address change by clicking the Confirm Email link below.",
                                "This link will expire in 7 days.", Request));

                    //Give the user a message
                    msgSys.ShowMessageToUser("success", "Success", "Email updated, you should receive an email soon to confirm the change.", 10000);
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// When the user clicks the Save button, save the changes to the general info
        /// </summary>
        /// <param name="sender">The submitGeneralInfo control</param>
        /// <param name="e">The Click event</param>
        protected void submitGeneralInfo_Click(object sender, EventArgs e)
        {
            //Set the fields
            currentUser.FirstName = txtFirstName.Value.ToString();
            currentUser.LastName = txtLastName.Value.ToString();
            currentUser.Street = (string.IsNullOrWhiteSpace(txtStreet.Text) ? null : txtStreet.Text);
            currentUser.City = (string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text);
            currentUser.State = (ddState.Value == null ? null : ddState.Value.ToString());
            currentUser.ZIPCode= (string.IsNullOrWhiteSpace(txtZIPCode.Text) ? null : txtZIPCode.Text);
            currentUser.RegionLocation = (string.IsNullOrWhiteSpace(txtRegionLocation.Text) ? null : txtRegionLocation.Text);
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

            //Set the update time in session and hide the account update alert div
            Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();
            divAccountUpdateAlert.Visible = false;

            //Show a message
            msgSys.ShowMessageToUser("success", "Success", "General Info successfully changed!", 10000);
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitGeneralInfo control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitGeneralInfo_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method binds the dropdowns with the necessary values
        /// </summary>
        private void BindStateDropdown()
        {
            using (PyramidContext context = new PyramidContext())
            {
                var allStates = context.State.AsNoTracking().Where(s => s.Name != "National" && s.Name != "Example")
                                                            .OrderBy(st => st.Name)
                                                            .ToList();

                ddState.DataSource = allStates;
                ddState.DataBind();
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
            try
            {
                List<spGetUserCustomizationOptions_Result> userCustomizationOptions = new List<spGetUserCustomizationOptions_Result>();
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the user's customization options
                    userCustomizationOptions = context.spGetUserCustomizationOptions(currentUser.UserName).ToList();


                    //Update the customization options

                    //------------------ Cancel Confirmation --------------------------

                    //Get the selected cancel confirmation option
                    spGetUserCustomizationOptions_Result selectedCancelConfirmationOption = userCustomizationOptions
                                                .Where(uco => uco.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.CANCEL_CONFIRMATION)
                                                .FirstOrDefault();

                    UserCustomizationOption cancelConfirmationOption;
                    if (selectedCancelConfirmationOption.UserCustomizationOptionPK.HasValue
                            && selectedCancelConfirmationOption.UserCustomizationOptionPK.Value > 0)
                    {

                        //Edit the cancel confirmation option
                        cancelConfirmationOption = context.UserCustomizationOption
                                                                    .Where(uco => uco.UserCustomizationOptionPK == selectedCancelConfirmationOption.UserCustomizationOptionPK.Value)
                                                                    .FirstOrDefault();

                        cancelConfirmationOption.Editor = User.Identity.Name;
                        cancelConfirmationOption.EditDate = DateTime.Now;
                        cancelConfirmationOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddCancelConfirmation.Value);
                        context.SaveChanges();
                    }
                    else
                    {
                        //Create the cancel confirmation option
                        cancelConfirmationOption = new UserCustomizationOption()
                        {
                            Creator = User.Identity.Name,
                            CreateDate = DateTime.Now,
                            Username = currentUser.UserName,
                            CustomizationOptionTypeCodeFK = selectedCancelConfirmationOption.OptionTypePK,
                            CustomizationOptionValueCodeFK = Convert.ToInt32(ddCancelConfirmation.Value)
                        };
                        context.UserCustomizationOption.Add(cancelConfirmationOption);
                        context.SaveChanges();
                    }
                    //------------------ End Cancel Confirmation --------------------------

                    //------------------ Fireworks --------------------------

                    //Get the selected fireworks option
                    spGetUserCustomizationOptions_Result selectedFireworksOption = userCustomizationOptions
                                                .Where(uco => uco.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.FIREWORKS)
                                                .FirstOrDefault();

                    UserCustomizationOption fireworksOption;
                    if (selectedFireworksOption.UserCustomizationOptionPK.HasValue
                            && selectedFireworksOption.UserCustomizationOptionPK.Value > 0)
                    {

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
                        fireworksOption.Username = currentUser.UserName;
                        fireworksOption.CustomizationOptionTypeCodeFK = selectedFireworksOption.OptionTypePK;
                        fireworksOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddFireworks.Value);
                        context.UserCustomizationOption.Add(fireworksOption);
                        context.SaveChanges();
                    }
                    //------------------ End Fireworks --------------------------

                    //------------------ Welcome Message --------------------------

                    //Get the selected fireworks option
                    spGetUserCustomizationOptions_Result selectedWelcomeMessageOption = userCustomizationOptions
                                                .Where(uco => uco.OptionTypePK == (int)UserCustomizationOption.CustomizationOptionTypeFKs.WELCOME_MESSAGE)
                                                .FirstOrDefault();

                    UserCustomizationOption welcomeMessageOption;
                    if (selectedWelcomeMessageOption.UserCustomizationOptionPK.HasValue
                            && selectedWelcomeMessageOption.UserCustomizationOptionPK.Value > 0)
                    {

                        //Edit the welcome message option
                        welcomeMessageOption = context.UserCustomizationOption
                                                                    .Where(uco => uco.UserCustomizationOptionPK == selectedWelcomeMessageOption.UserCustomizationOptionPK.Value)
                                                                    .FirstOrDefault();

                        welcomeMessageOption.Editor = User.Identity.Name;
                        welcomeMessageOption.EditDate = DateTime.Now;
                        welcomeMessageOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddWelcomeMessage.Value);
                        context.SaveChanges();
                    }
                    else
                    {
                        //Create the welcome message option
                        welcomeMessageOption = new UserCustomizationOption();
                        welcomeMessageOption.Creator = User.Identity.Name;
                        welcomeMessageOption.CreateDate = DateTime.Now;
                        welcomeMessageOption.Username = currentUser.UserName;
                        welcomeMessageOption.CustomizationOptionTypeCodeFK = selectedWelcomeMessageOption.OptionTypePK;
                        welcomeMessageOption.CustomizationOptionValueCodeFK = Convert.ToInt32(ddWelcomeMessage.Value);
                        context.UserCustomizationOption.Add(welcomeMessageOption);
                        context.SaveChanges();
                    }
                    //------------------ End Fireworks --------------------------


                    //Refresh the user's customization options
                    userCustomizationOptions = context.spGetUserCustomizationOptions(currentUser.UserName).ToList();
                }

                //Set the customization options cookie
                bool isCookieSaved = UserCustomizationOption.SetCustomizationOptionCookie(userCustomizationOptions);

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
        /// This method fires when the user clicks a button to confirm their account info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnConfirmAccountInfo_Click(object sender, EventArgs e)
        {
            //Set the edit fields and update the update time
            currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
            currentUser.UpdateTime = DateTime.Now;
            userManager.Update(currentUser);

            //Update the sesion variable
            Session["UserLastUpdateDate"] = currentUser.UpdateTime.ToString();

            //Show a message
            msgSys.ShowMessageToUser("success", "Success", "Thanks for confirming your account information!", 10000);

            //Hide the reminder to update the account info
            divAccountUpdateAlert.Visible = false;
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