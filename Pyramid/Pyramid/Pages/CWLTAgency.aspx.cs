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
    public partial class CWLTAgency : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CWLTA";
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
        private Models.CWLTAgency currentCWLTAgency;
        private int currentHubFK;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CWLTAgencyPK"]))
            {
                //Parse the Agency pk
                int.TryParse(Request.QueryString["CWLTAgencyPK"], out currentAgencyPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentAgencyPK == 0 && !string.IsNullOrWhiteSpace(hfCWLTAgencyPK.Value))
            {
                int.TryParse(hfCWLTAgencyPK.Value, out currentAgencyPK);
            }

            //Check to see if this is an edit
            isEdit = currentAgencyPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/CWLTDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the Agency program object
                currentCWLTAgency = context.CWLTAgency.AsNoTracking()
                                        .Include(sm => sm.Hub)
                                        .Where(sm => sm.CWLTAgencyPK == currentAgencyPK).FirstOrDefault();

                //Check to see if the program Agency exists
                if (currentCWLTAgency == null)
                {
                    //The Agency doesn't exist, set the Agency to a new Agency object
                    currentCWLTAgency = new Models.CWLTAgency();

                    //Set the hub label to the current user's hub
                    lblHub.Text = currentProgramRole.HubName;
                }
                else
                {
                    //Set the hub label to the form's hub
                    lblHub.Text = currentCWLTAgency.Hub.Name;
                }
            }

            //Don't allow users to view Agency information from other programs
            if (isEdit && !currentProgramRole.HubFKs.Contains(currentCWLTAgency.HubFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "NoAgency"));
            }

            //Get the proper FKs
            currentHubFK = (isEdit ? currentCWLTAgency.HubFK : currentProgramRole.CurrentHubFK.Value);
            currentStateFK = (isEdit ? currentCWLTAgency.Hub.StateFK : currentProgramRole.CurrentStateFK.Value);

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

                //Bind the drop-downs
                BindDropDowns();

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
        /// This method fills the input fields with data from the currentCWLTAgency
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                txtName.Value = currentCWLTAgency.Name;
                ddCWLTAgencyType.SelectedItem = ddCWLTAgencyType.Items.FindByValue(currentCWLTAgency.CWLTAgencyTypeFK);
                txtPhoneNumber.Value = currentCWLTAgency.PhoneNumber;
                txtWebsite.Value = currentCWLTAgency.Website;
                txtAddressStreet.Value = currentCWLTAgency.AddressStreet;
                txtAddressCity.Value = currentCWLTAgency.AddressCity;
                txtAddressState.Value = currentCWLTAgency.AddressState;
                txtAddressZIPCode.Value = currentCWLTAgency.AddressZIPCode;
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
            ddCWLTAgencyType.ClientEnabled = enabled;
            txtPhoneNumber.ClientEnabled = enabled;
            txtWebsite.ClientEnabled = enabled;
            txtAddressStreet.ClientEnabled = enabled;
            txtAddressCity.ClientEnabled = enabled;
            txtAddressState.ClientEnabled = enabled;
            txtAddressZIPCode.ClientEnabled = enabled;

            //Show/hide the submit button
            submitCWLTAgency.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitCWLTAgency.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the agencies
                var allAgencyTypes = context.CWLTAgencyType.AsNoTracking()
                                    .Where(a => a.StateFK == currentStateFK)
                                    .OrderBy(a => a.Name)
                                    .ToList();

                //Bind the dropdown
                ddCWLTAgencyType.DataSource = allAgencyTypes;
                ddCWLTAgencyType.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitCWLTAgency user control 
        /// </summary>
        /// <param name="sender">The submitCWLTAgency control</param>
        /// <param name="e">The Click event</param>
        protected void submitCWLTAgency_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard
                Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitCWLTAgency user control 
        /// </summary>
        /// <param name="sender">The submitCWLTAgency control</param>
        /// <param name="e">The Click event</param>
        protected void submitCWLTAgency_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Agency Dashboard
            Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "AgencyCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCWLTAgency control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCWLTAgency_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitCWLTAgency.ValidationGroup))
            {
                //Submit the form but don't show messages
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptCWLTAgency report = new Reports.PreBuiltReports.FormReports.RptCWLTAgency();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "CWLT Agency", currentAgencyPK);
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
                currentCWLTAgency.Name = txtName.Value.ToString();
                currentCWLTAgency.CWLTAgencyTypeFK = Convert.ToInt32(ddCWLTAgencyType.Value);
                currentCWLTAgency.PhoneNumber = (txtPhoneNumber.Value == null ? null : txtPhoneNumber.Value.ToString());
                currentCWLTAgency.Website = (txtWebsite.Value == null ? null : txtWebsite.Value.ToString());
                currentCWLTAgency.AddressStreet = (txtAddressStreet.Value == null ? null : txtAddressStreet.Value.ToString());
                currentCWLTAgency.AddressCity = (txtAddressCity.Value == null ? null : txtAddressCity.Value.ToString());
                currentCWLTAgency.AddressState = (txtAddressState.Value == null ? null : txtAddressState.Value.ToString());
                currentCWLTAgency.AddressZIPCode = (txtAddressZIPCode.Value == null ? null : txtAddressZIPCode.Value.ToString());

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "AgencyEdited";

                        //Set the fields
                        currentCWLTAgency.EditDate = DateTime.Now;
                        currentCWLTAgency.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.CWLTAgency existingCWLTAgency = context.CWLTAgency.Find(currentCWLTAgency.CWLTAgencyPK);

                        //Set the Agency object to the new values
                        context.Entry(existingCWLTAgency).CurrentValues.SetValues(currentCWLTAgency);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTAgencyPK.Value = currentCWLTAgency.CWLTAgencyPK.ToString();
                        currentAgencyPK = currentCWLTAgency.CWLTAgencyPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "AgencyAdded";

                        //Set the field values
                        currentCWLTAgency.CreateDate = DateTime.Now;
                        currentCWLTAgency.Creator = User.Identity.Name;
                        currentCWLTAgency.HubFK = currentHubFK;

                        //Add it to the context
                        context.CWLTAgency.Add(currentCWLTAgency);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTAgencyPK.Value = currentCWLTAgency.CWLTAgencyPK.ToString();
                        currentAgencyPK = currentCWLTAgency.CWLTAgencyPK;
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