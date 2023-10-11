using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace Pyramid.Pages
{
    public partial class CWLTAgencyType : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CWLTAT";
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
        private Models.CWLTAgencyType currentCWLTAgencyType;
        private int currentStateFK;
        private int currentAgencyTypePK = 0;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CWLTAgencyTypePK"]))
            {
                //Parse the Agency pk
                int.TryParse(Request.QueryString["CWLTAgencyTypePK"], out currentAgencyTypePK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentAgencyTypePK == 0 && !string.IsNullOrWhiteSpace(hfCWLTAgencyTypePK.Value))
            {
                int.TryParse(hfCWLTAgencyTypePK.Value, out currentAgencyTypePK);
            }

            //Check to see if this is an edit
            isEdit = currentAgencyTypePK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/CWLTDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the agency type object
                currentCWLTAgencyType = context.CWLTAgencyType.AsNoTracking()
                                        .Include(cat => cat.State)
                                        .Where(cat => cat.CWLTAgencyTypePK == currentAgencyTypePK).FirstOrDefault();

                //Check to see if the agency type exists
                if (currentCWLTAgencyType == null)
                {
                    //It doesn't exist, set to a new object
                    currentCWLTAgencyType = new Models.CWLTAgencyType();

                    //Set the state label to the current user's state
                    lblState.Text = currentProgramRole.StateName;
                }
                else
                {
                    //Set the state label to the form's state
                    lblState.Text = currentCWLTAgencyType.State.Name;
                }
            }

            //Don't allow users to view agency types from other states
            if (isEdit && !currentProgramRole.StateFKs.Contains(currentCWLTAgencyType.StateFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "NoAgencyType"));
            }

            //Get the proper state fk
            currentStateFK = (isEdit ? currentCWLTAgencyType.StateFK : currentProgramRole.CurrentStateFK.Value);

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
                    lblPageTitle.Text = "Add New Agency Type";
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
                    lblPageTitle.Text = "Edit Agency Type Information";
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
                    lblPageTitle.Text = "View Agency Type Information";
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
                txtName.Value = currentCWLTAgencyType.Name;
                txtDescription.Value = currentCWLTAgencyType.Description;
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
            txtDescription.ClientEnabled = enabled;

            //Show/hide the submit button
            submitCWLTAgencyType.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitCWLTAgencyType.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitCWLTAgencyType user control 
        /// </summary>
        /// <param name="sender">The submitCWLTAgencyType control</param>
        /// <param name="e">The Click event</param>
        protected void submitCWLTAgencyType_Click(object sender, EventArgs e)
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
        /// submitCWLTAgencyType user control 
        /// </summary>
        /// <param name="sender">The submitCWLTAgencyType control</param>
        /// <param name="e">The Click event</param>
        protected void submitCWLTAgencyType_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Agency Dashboard
            Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "AgencyTypeCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCWLTAgencyType control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCWLTAgencyType_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitCWLTAgencyType.ValidationGroup))
            {
                //Submit the form but don't show messages
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptCWLTAgencyType report = new Reports.PreBuiltReports.FormReports.RptCWLTAgencyType();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "CWLT Agency Type", currentAgencyTypePK);
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
                currentCWLTAgencyType.Name = txtName.Value.ToString();
                currentCWLTAgencyType.Description = txtDescription.Value.ToString();

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "AgencyTypeEdited";

                        //Set the fields
                        currentCWLTAgencyType.EditDate = DateTime.Now;
                        currentCWLTAgencyType.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.CWLTAgencyType existingCWLTAgencyType = context.CWLTAgencyType.Find(currentCWLTAgencyType.CWLTAgencyTypePK);

                        //Set the Agency object to the new values
                        context.Entry(existingCWLTAgencyType).CurrentValues.SetValues(currentCWLTAgencyType);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTAgencyTypePK.Value = currentCWLTAgencyType.CWLTAgencyTypePK.ToString();
                        currentAgencyTypePK = currentCWLTAgencyType.CWLTAgencyTypePK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "AgencyTypeAdded";

                        //Set the field values
                        currentCWLTAgencyType.CreateDate = DateTime.Now;
                        currentCWLTAgencyType.Creator = User.Identity.Name;
                        currentCWLTAgencyType.StateFK = currentStateFK;

                        //Add it to the context
                        context.CWLTAgencyType.Add(currentCWLTAgencyType);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTAgencyTypePK.Value = currentCWLTAgencyType.CWLTAgencyTypePK.ToString();
                        currentAgencyTypePK = currentCWLTAgencyType.CWLTAgencyTypePK;
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
    }
}