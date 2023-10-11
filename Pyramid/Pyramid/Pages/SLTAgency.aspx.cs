using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class SLTAgency : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "SLTA";
            }
        }

        public CodeProgramRolePermission FormPermissions
        {
            get
            {
                return currentPermissions;
            }
            set
            {
                currentPermissions = value;
            }
        }

        private CodeProgramRolePermission currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.SLTAgency currentSLTAgency;
        private int currentStateFK;
        private int currentAgencyPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the Agency pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["SLTAgencyPK"]))
            {
                //Parse the Agency pk
                int.TryParse(Request.QueryString["SLTAgencyPK"], out currentAgencyPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentAgencyPK == 0 && !string.IsNullOrWhiteSpace(hfSLTAgencyPK.Value))
            {
                int.TryParse(hfSLTAgencyPK.Value, out currentAgencyPK);
            }

            //Check to see if this is an edit
            isEdit = currentAgencyPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/SLTDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the Agency program object
                currentSLTAgency = context.SLTAgency.AsNoTracking()
                                        .Include(sm => sm.State)
                                        .Where(sm => sm.SLTAgencyPK == currentAgencyPK).FirstOrDefault();

                //Check to see if the program Agency exists
                if (currentSLTAgency == null)
                {
                    //The Agency doesn't exist, set the Agency to a new Agency object
                    currentSLTAgency = new Models.SLTAgency();

                    //Set the state label to the current user's state
                    lblState.Text = currentProgramRole.StateName;
                }
                else
                {
                    //Set the state label to the form's state
                    lblState.Text = currentSLTAgency.State.Name;
                }
            }

            //Don't allow users to view Agency information from other programs
            if (isEdit && !currentProgramRole.StateFKs.Contains(currentSLTAgency.StateFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", "NoAgency"));
            }

            //Get the proper state fk
            currentStateFK = (isEdit ? currentSLTAgency.StateFK : currentProgramRole.CurrentStateFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                //Fill the form with data
                FillFormWithDataFromObject();

                //Allow adding/editing depending on the user's role and the action
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Agency";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Agency Information";
                }
                else
                {
                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Agency Information";
                }

                //Set focus to the name field
                txtName.Focus();
            }
        }

        /// <summary>
        /// This method fills the input fields with data from the currentSLTAgency
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                txtName.Value = currentSLTAgency.Name;
                txtPhoneNumber.Value = currentSLTAgency.PhoneNumber;
                txtWebsite.Value = currentSLTAgency.Website;
                txtAddressStreet.Value = currentSLTAgency.AddressStreet;
                txtAddressCity.Value = currentSLTAgency.AddressCity;
                txtAddressState.Value = currentSLTAgency.AddressState;
                txtAddressZIPCode.Value = currentSLTAgency.AddressZIPCode;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            txtName.ClientEnabled = enabled;
            txtPhoneNumber.ClientEnabled = enabled;
            txtWebsite.ClientEnabled = enabled;
            txtAddressStreet.ClientEnabled = enabled;
            txtAddressCity.ClientEnabled = enabled;
            txtAddressState.ClientEnabled = enabled;
            txtAddressZIPCode.ClientEnabled = enabled;

            //Show/hide the submit button
            submitSLTAgency.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitSLTAgency.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitSLTAgency user control 
        /// </summary>
        /// <param name="sender">The submitSLTAgency control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTAgency_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard
                Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitSLTAgency user control 
        /// </summary>
        /// <param name="sender">The submitSLTAgency control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTAgency_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Agency Dashboard
            Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", "AgencyCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitSLTAgency control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitSLTAgency_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the user clicks the print/download button
        /// and it displays the form as a report
        /// </summary>
        /// <param name="sender">The btnPrintPreview LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void btnPrintPreview_Click(object sender, EventArgs e)
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitSLTAgency.ValidationGroup))
            {
                //Submit the form but don't show messages
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptSLTAgency report = new Reports.PreBuiltReports.FormReports.RptSLTAgency();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "SLT Agency", currentAgencyPK);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method populates and saves the form
        /// </summary>
        /// <param name="showMessages">Whether to show messages from the save</param>
        /// <returns>The success message type, null if the save failed</returns>
        private string SaveForm(bool showMessages)
        {
            //To hold the type of change
            string successMessageType = null;

            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentSLTAgency.Name = txtName.Value.ToString();
                currentSLTAgency.PhoneNumber = (txtPhoneNumber.Value == null ? null : txtPhoneNumber.Value.ToString());
                currentSLTAgency.Website = (txtWebsite.Value == null ? null : txtWebsite.Value.ToString());
                currentSLTAgency.AddressStreet = (txtAddressStreet.Value == null ? null : txtAddressStreet.Value.ToString());
                currentSLTAgency.AddressCity = (txtAddressCity.Value == null ? null : txtAddressCity.Value.ToString());
                currentSLTAgency.AddressState = (txtAddressState.Value == null ? null : txtAddressState.Value.ToString());
                currentSLTAgency.AddressZIPCode = (txtAddressZIPCode.Value == null ? null : txtAddressZIPCode.Value.ToString());

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "AgencyEdited";

                        //Set the fields
                        currentSLTAgency.EditDate = DateTime.Now;
                        currentSLTAgency.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.SLTAgency existingSLTAgency = context.SLTAgency.Find(currentSLTAgency.SLTAgencyPK);

                        //Set the Agency object to the new values
                        context.Entry(existingSLTAgency).CurrentValues.SetValues(currentSLTAgency);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTAgencyPK.Value = currentSLTAgency.SLTAgencyPK.ToString();
                        currentAgencyPK = currentSLTAgency.SLTAgencyPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "AgencyAdded";

                        //Set the field values
                        currentSLTAgency.CreateDate = DateTime.Now;
                        currentSLTAgency.Creator = User.Identity.Name;
                        currentSLTAgency.StateFK = currentStateFK;

                        //Add it to the context
                        context.SLTAgency.Add(currentSLTAgency);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTAgencyPK.Value = currentSLTAgency.SLTAgencyPK.ToString();
                        currentAgencyPK = currentSLTAgency.SLTAgencyPK;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return successMessageType;
        }

        /// <summary>
        /// This method fires when the validation for the txtPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPhoneNumber_Validation(object sender, ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }
    }
}