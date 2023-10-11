using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class ProgramAddress : System.Web.UI.Page
    {

        public string FormAbbreviation
        {
            get
            {
                return "PA";
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
        private Models.ProgramAddress currentProgramAddress;
        private int currentAddressPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the member pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ProgramAddressPK"]))
            {
                //Parse the member pk
                int.TryParse(Request.QueryString["ProgramAddressPK"], out currentAddressPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentAddressPK == 0 && !string.IsNullOrWhiteSpace(hfProgramAddressPK.Value))
            {
                int.TryParse(hfProgramAddressPK.Value, out currentAddressPK);
            }

            //Check to see if this is an edit
            isEdit = currentAddressPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Add a message that will display after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect back to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the member object
                currentProgramAddress = context.ProgramAddress.AsNoTracking()
                                        .Include(pm => pm.Program)
                                        .Where(pm => pm.ProgramAddressPK == currentAddressPK).FirstOrDefault();

                //Check to see if the member exists
                if (currentProgramAddress == null)
                {
                    //The member doesn't exist, set the member to a new member object
                    currentProgramAddress = new Models.ProgramAddress();

                    //set program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //set the program label to the child's program
                    lblProgram.Text = currentProgramAddress.Program.ProgramName;
                }
            }

            //Don't allow users to view member information from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentProgramAddress.ProgramFK))
            {
                //Add a message to show after redirect
                msgSys.AddMessageToQueue("warning", "Warning", "The specified Program Address could not be found, please try again.", 15000);

                //Redirect the user to the dashboard with an error message
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

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

                //Bind dropdowns
                BindDropdowns();
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
                    lblPageTitle.Text = "Add New Program Address";
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
                    lblPageTitle.Text = "Edit Program Address Information";
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
                    lblPageTitle.Text = "View Program Address Information";
                }

                //Set focus to the first field
                txtCity.Focus();

                //Check for the printing item in the query string
                string strIsPrinting = Request.QueryString["Print"];

                //Check to see if printing
                if (!string.IsNullOrWhiteSpace(strIsPrinting))
                {
                    //To hold the printing value
                    bool isPrinting = false;

                    //Print the form if the query string value is true
                    if (bool.TryParse(strIsPrinting, out isPrinting) && isPrinting == true)
                    {
                        //Print the form
                        PrintForm();
                    }
                }
            }
        }

        /// <summary>
        /// This method binds the dropdowns with the necessary values
        /// </summary>
        private void BindDropdowns()
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
        /// This method prints the form
        /// </summary>
        private void PrintForm()
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitProgramAddress.ValidationGroup))
            {
                //Try to save the form to the database
                bool formSaved = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptProgramAddress report = new Reports.PreBuiltReports.FormReports.RptProgramAddress(); 

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Program Address Information", currentAddressPK);
                }
                else
                {
                    //Get the action
                    string action = "View";
                    if (formSaved)
                    {
                        //The save was successful, the user will be editing
                        action = "Edit";
                    }

                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Program Address was successfully saved!", 12000);

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/ProgramAddress.aspx?ProgramAddressPK={0}&Action={1}&Print=True",
                                                        currentAddressPK, action));
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        private bool SaveForm(bool showMessages)
        {
            //Whether or not the save succeeded
            bool didSaveSucceed = false;

            //Check the user permissions
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentProgramAddress.City = txtCity.Value.ToString();
                currentProgramAddress.Street = txtStreet.Value.ToString();
                currentProgramAddress.ZIPCode = txtZIPCode.Value.ToString();
                currentProgramAddress.Notes = (string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text);
                currentProgramAddress.State = ddState.Value.ToString();
                currentProgramAddress.IsMailingAddress = Convert.ToBoolean(ddMailing.Value);
                currentProgramAddress.LicenseNumber = (string.IsNullOrWhiteSpace(txtLicenseNumber.Text) ? null : txtLicenseNumber.Text);

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the fields
                        currentProgramAddress.EditDate = DateTime.Now;
                        currentProgramAddress.Editor = User.Identity.Name;

                        //Get the existing Program Address
                        Models.ProgramAddress existingProgramAddress = context.ProgramAddress.Find(currentProgramAddress.ProgramAddressPK);

                        //Set the address object to the new values
                        context.Entry(existingProgramAddress).CurrentValues.SetValues(currentProgramAddress);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfProgramAddressPK.Value = currentProgramAddress.ProgramAddressPK.ToString();
                        currentAddressPK = currentProgramAddress.ProgramAddressPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the field values
                        currentProgramAddress.CreateDate = DateTime.Now;
                        currentProgramAddress.Creator = User.Identity.Name;
                        currentProgramAddress.ProgramFK = Convert.ToInt32(currentProgramRole.CurrentProgramFK.Value);

                        //Add it to the context and save
                        context.ProgramAddress.Add(currentProgramAddress);
                        context.SaveChanges();


                        //Set the hidden field and local variable
                        hfProgramAddressPK.Value = currentProgramAddress.ProgramAddressPK.ToString();
                        currentAddressPK = currentProgramAddress.ProgramAddressPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            return didSaveSucceed;

        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            txtCity.ClientEnabled = enabled;
            txtStreet.ClientEnabled = enabled;
            ddState.ClientEnabled = enabled;
            txtZIPCode.ClientEnabled = enabled;
            txtNote.ClientEnabled = enabled;
            txtLicenseNumber.ClientEnabled = enabled;
            ddMailing.ClientEnabled = enabled;

            //Show/hide the submit button
            submitProgramAddress.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitProgramAddress.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method fills the input fields with data from the currentProgramAddress
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                txtCity.Value = currentProgramAddress.City;
                txtStreet.Value = currentProgramAddress.Street;
                txtZIPCode.Value = currentProgramAddress.ZIPCode;
                ddState.SelectedItem = ddState.Items.FindByValue(currentProgramAddress.State);
                txtNote.Value = currentProgramAddress.Notes;
                txtLicenseNumber.Value = currentProgramAddress.LicenseNumber;
                ddMailing.SelectedItem = ddMailing.Items.FindByValue(currentProgramAddress.IsMailingAddress);
            }
        }
        /// <summary>
        /// This method fires when the user clicks the print/download button
        /// and it displays the form as a report
        /// </summary>
        /// <param name="sender">The btnPrintPreview LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void btnPrintPreview_Click(object sender, EventArgs e)
        {
            //Print the form
            PrintForm();
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitProgramAddress user control 
        /// </summary>
        /// <param name="sender">The submitProgramAddress control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgramAddress_Click(object sender, EventArgs e)
        {
            //Try to save the form to the database
            bool formSaved = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (formSaved)
            {
                //Add a message to show after redirect
                msgSys.AddMessageToQueue("success", "Success", "The Program Address was successfully edited!", 10000);

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitProgramAddress user control 
        /// </summary>
        /// <param name="sender">The submitProgramAddress control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgramAddress_CancelClick(object sender, EventArgs e)
        {
            //Add a message that will show after redirect
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect the user to the PLT dashboard
            Response.Redirect("/Pages/PLTDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitProgramAddress control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitProgramAddress_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }
    }

}