using Pyramid.Models;
using System;
using System.Data.Entity;
using System.Linq;
using Pyramid.Code;
using Pyramid.MasterPages;
using DevExpress.Web;
using System.Collections.Generic;

namespace Pyramid.Pages
{
    public partial class BOQFCCV2 : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "BOQFCC";
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
        private BenchmarkOfQualityFCC currentBOQFCC;
        private int BOQFCCPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the BOQ PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["BOQFCCPK"]))
            {
                int.TryParse(Request.QueryString["BOQFCCPK"], out BOQFCCPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (BOQFCCPK == 0 && !string.IsNullOrWhiteSpace(hfBOQFCCPK.Value))
            {
                int.TryParse(hfBOQFCCPK.Value, out BOQFCCPK);
            }

            //Check to see if this is an edit
            isEdit = BOQFCCPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Set a message to display after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect back to the dashboard
                Response.Redirect("/Pages/BOQFCCDashboard.aspx");
            }

            //Get the BOQ from the database
            using (PyramidContext context = new PyramidContext())
            {
                //Get the BOQ
                currentBOQFCC = context.BenchmarkOfQualityFCC.AsNoTracking().Where(boqfcc => boqfcc.BenchmarkOfQualityFCCPK == BOQFCCPK).FirstOrDefault();

                //If the BOQ is null (this is an add)
                if (currentBOQFCC == null)
                {
                    //Set the current BOQ to a blank BOQ
                    currentBOQFCC = new BenchmarkOfQualityFCC();
                }
            }

            //Don't allow users to view BOQs from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentBOQFCC.ProgramFK))
            {
                //Set a message to display after redirect
                msgSys.AddMessageToQueue("warning", "Warning", "The specified Benchmarks of Quality FCC V2 form could not be found, please try again.", 15000);

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/BOQFCCDashboard.aspx");
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Bind the drop-downs
                BindDropDowns();

                //Populate the page with values from the object
                PopulatePage(currentBOQFCC);

                //Get the action
                string action;
                if (Request.QueryString["action"] != null)
                {
                    action = Request.QueryString["action"];
                }
                else
                {
                    action = "View";
                }

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
                    lblPageTitle.Text = "Add New Benchmarks of Quality FCC V2 Form";
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
                    lblPageTitle.Text = "Edit Benchmarks of Quality FCC V2 Form";
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
                    lblPageTitle.Text = "View Benchmarks of Quality FCC V2 Form";
                }

                //Set the max value for the form date field
                deFormDate.MaxDate = DateTime.Now;

                //Set focus to the first field
                ddProgram.Focus();
            }
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            //To hold all the necessary items
            List<Program> allFCCPrograms = new List<Program>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allFCCPrograms = context.Program
                                        .Include(p => p.ProgramType)
                                        .AsNoTracking()
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK) &&
                                                    p.ProgramType.Where(pt => pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE ||
                                                                            pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE).Count() > 0)
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();
            }

            //Bind the program dropdown
            ddProgram.DataSource = allFCCPrograms;
            ddProgram.DataBind();
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitBOQFCC user control 
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQFCC_Click(object sender, EventArgs e)
        {
            //To hold the success message
            string successMessageType = SaveForm(true);

            //Only redirect is the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Set a message to display after redirect
                if (successMessageType == "BOQFCCAdded")
                {
                    msgSys.AddMessageToQueue("success", "Success", "Benchmarks Of Quality FCC V2 form successfully added!", 1000);
                }
                else if (successMessageType == "BOQFCCEdited")
                {
                    msgSys.AddMessageToQueue("success", "Success", "Benchmarks Of Quality FCC V2 form successfully edited!", 1000);
                }

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/BOQFCCDashboard.aspx");
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitBOQFCC user control 
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQFCC_CancelClick(object sender, EventArgs e)
        {
            //Set a message to display after redirect
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect to the dashboard
            Response.Redirect("/Pages/BOQFCCDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitBOQFCC_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitBOQFCC.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptBOQFCCV2 report = new Reports.PreBuiltReports.FormReports.RptBOQFCCV2();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "Benchmarks of Quality FCC V2", BOQFCCPK);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddProgram.ClientEnabled = enabled;
            deFormDate.ClientEnabled = enabled;
            txtTeamMembers.ClientEnabled = enabled;
            ddIndicator1.ClientEnabled = enabled;
            ddIndicator2.ClientEnabled = enabled;
            ddIndicator3.ClientEnabled = enabled;
            ddIndicator4.ClientEnabled = enabled;
            ddIndicator5.ClientEnabled = enabled;
            ddIndicator6.ClientEnabled = enabled;
            ddIndicator7.ClientEnabled = enabled;
            ddIndicator8.ClientEnabled = enabled;
            ddIndicator9.ClientEnabled = enabled;
            ddIndicator10.ClientEnabled = enabled;
            ddIndicator11.ClientEnabled = enabled;
            ddIndicator12.ClientEnabled = enabled;
            ddIndicator13.ClientEnabled = enabled;
            ddIndicator14.ClientEnabled = enabled;
            ddIndicator15.ClientEnabled = enabled;
            ddIndicator16.ClientEnabled = enabled;
            ddIndicator17.ClientEnabled = enabled;
            ddIndicator18.ClientEnabled = enabled;
            ddIndicator19.ClientEnabled = enabled;
            ddIndicator20.ClientEnabled = enabled;
            ddIndicator21.ClientEnabled = enabled;
            ddIndicator22.ClientEnabled = enabled;
            ddIndicator23.ClientEnabled = enabled;
            ddIndicator24.ClientEnabled = enabled;
            ddIndicator25.ClientEnabled = enabled;
            ddIndicator26.ClientEnabled = enabled;
            ddIndicator27.ClientEnabled = enabled;
            ddIndicator28.ClientEnabled = enabled;
            ddIndicator29.ClientEnabled = enabled;
            ddIndicator30.ClientEnabled = enabled;
            ddIndicator31.ClientEnabled = enabled;
            chkIsComplete.ClientEnabled = enabled;

            //Show/hide the submit button
            submitBOQFCC.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitBOQFCC.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method populates the controls on the page with values
        /// from the passed BenchmarkOfQualityFCC object
        /// </summary>
        /// <param name="boq">The BenchmarkOfQualityFCC object to fill the page controls</param>
        private void PopulatePage(BenchmarkOfQualityFCC boq)
        {
            if (isEdit)
            {
                //Get the matching drop-down item for the program
                var matchingProgramDDItem = ddProgram.Items.FindByValue(boq.ProgramFK);

                if (matchingProgramDDItem != null)
                {
                    //Set the value in the program dropdown
                    ddProgram.SelectedItem = matchingProgramDDItem;

                    //Set the program labels
                    SetProgramLabels(boq.ProgramFK);
                }

                //Set the indicator dropdown selected values
                ddIndicator1.SelectedItem = ddIndicator1.Items.FindByValue(boq.Indicator1);
                ddIndicator2.SelectedItem = ddIndicator2.Items.FindByValue(boq.Indicator2);
                ddIndicator3.SelectedItem = ddIndicator3.Items.FindByValue(boq.Indicator3);
                ddIndicator4.SelectedItem = ddIndicator4.Items.FindByValue(boq.Indicator4);
                ddIndicator5.SelectedItem = ddIndicator5.Items.FindByValue(boq.Indicator5);
                ddIndicator6.SelectedItem = ddIndicator6.Items.FindByValue(boq.Indicator6);
                ddIndicator7.SelectedItem = ddIndicator7.Items.FindByValue(boq.Indicator7);
                ddIndicator8.SelectedItem = ddIndicator8.Items.FindByValue(boq.Indicator8);
                ddIndicator9.SelectedItem = ddIndicator9.Items.FindByValue(boq.Indicator9);
                ddIndicator10.SelectedItem = ddIndicator10.Items.FindByValue(boq.Indicator10);
                ddIndicator11.SelectedItem = ddIndicator11.Items.FindByValue(boq.Indicator11);
                ddIndicator12.SelectedItem = ddIndicator12.Items.FindByValue(boq.Indicator12);
                ddIndicator13.SelectedItem = ddIndicator13.Items.FindByValue(boq.Indicator13);
                ddIndicator14.SelectedItem = ddIndicator14.Items.FindByValue(boq.Indicator14);
                ddIndicator15.SelectedItem = ddIndicator15.Items.FindByValue(boq.Indicator15);
                ddIndicator16.SelectedItem = ddIndicator16.Items.FindByValue(boq.Indicator16);
                ddIndicator17.SelectedItem = ddIndicator17.Items.FindByValue(boq.Indicator17);
                ddIndicator18.SelectedItem = ddIndicator18.Items.FindByValue(boq.Indicator18);
                ddIndicator19.SelectedItem = ddIndicator19.Items.FindByValue(boq.Indicator19);
                ddIndicator20.SelectedItem = ddIndicator20.Items.FindByValue(boq.Indicator20);
                ddIndicator21.SelectedItem = ddIndicator21.Items.FindByValue(boq.Indicator21);
                ddIndicator22.SelectedItem = ddIndicator22.Items.FindByValue(boq.Indicator22);
                ddIndicator23.SelectedItem = ddIndicator23.Items.FindByValue(boq.Indicator23);
                ddIndicator24.SelectedItem = ddIndicator24.Items.FindByValue(boq.Indicator24);
                ddIndicator25.SelectedItem = ddIndicator25.Items.FindByValue(boq.Indicator25);
                ddIndicator26.SelectedItem = ddIndicator26.Items.FindByValue(boq.Indicator26);
                ddIndicator27.SelectedItem = ddIndicator27.Items.FindByValue(boq.Indicator27);
                ddIndicator28.SelectedItem = ddIndicator28.Items.FindByValue(boq.Indicator28);
                ddIndicator29.SelectedItem = ddIndicator29.Items.FindByValue(boq.Indicator29);
                ddIndicator30.SelectedItem = ddIndicator30.Items.FindByValue(boq.Indicator30);
                ddIndicator31.SelectedItem = ddIndicator31.Items.FindByValue(boq.Indicator31);

                //Set the form date
                deFormDate.Date = boq.FormDate;

                //Set the team members
                txtTeamMembers.Text = boq.TeamMembers;

                //Set the IsComplete checkbox
                chkIsComplete.Checked = boq.IsComplete;

                //Disable the IsComplete field and show/hide the section
                chkIsComplete.ClientReadOnly = boq.IsComplete;
                divIsComplete.Visible = (boq.IsComplete ? false : true);

                //Set the required fields based on whether the form is complete or not
                HandleIsComplete(boq.IsComplete);
            }
            else
            {
                //Get the matching drop-down item for the program
                var matchingProgramDDItem = ddProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK.Value);

                if (matchingProgramDDItem != null)
                {
                    //Set the value in the program dropdown
                    ddProgram.SelectedItem = matchingProgramDDItem;

                    //Set the program labels
                    SetProgramLabels(currentProgramRole.CurrentProgramFK.Value);
                }

                //Set the required fields to least restrictive
                HandleIsComplete(false);
            }
        }

        /// <summary>
        /// This method fires when the user selects a program from the ddProgram control.
        /// </summary>
        /// <param name="sender">The ddProgram BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddProgram_ValueChanged(object sender, EventArgs e)
        {
            //Get the selected values
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);

            //Set the labels related to the program 
            SetProgramLabels(selectedProgramFK);

            //Re-focus on the control
            ddProgram.Focus();
        }

        /// <summary>
        /// This method fires when the chkIsComplete checkbox is checked or unchecked and
        /// it sets validation based on the checked value
        /// </summary>
        /// <param name="sender">The chkIsComplete BootstrapCheckBox</param>
        /// <param name="e">The event args</param>
        protected void chkIsComplete_CheckedChanged(object sender, EventArgs e)
        {
            //Set the required field values for the controls
            HandleIsComplete(chkIsComplete.Checked);
        }

        /// <summary>
        /// This method sets the program labels based on the passed parameter
        /// </summary>
        /// <param name="currentProgramFK">The program FK</param>
        private void SetProgramLabels(int? currentProgramFK)
        {
            //Make sure the selected program is valid before continuing
            if (currentProgramFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program info
                    Program selectedProgram = context.Program
                                                        .AsNoTracking()
                                                        .Where(p => p.ProgramPK == currentProgramFK.Value)
                                                        .FirstOrDefault();

                    //Make sure the program exists
                    if (selectedProgram != null)
                    {
                        //Set the program-specific labels
                        lblProgramLocation.Text = (string.IsNullOrWhiteSpace(selectedProgram.Location) ? "No location found..." : selectedProgram.Location);
                    }
                }
            }
        }

        /// <summary>
        /// This method enables/disables validation based on whether the form is complete or not
        /// </summary>
        /// <param name="isComplete">True if the form is complete, false otherwise</param>
        private void HandleIsComplete(bool isComplete)
        {
            //Set certain fields to required or not
            ddIndicator1.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator2.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator3.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator4.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator5.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator6.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator7.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator8.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator9.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator10.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator11.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator12.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator13.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator14.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator15.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator16.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator17.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator18.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator19.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator20.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator21.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator22.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator23.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator24.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator25.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator26.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator27.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator28.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator29.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator30.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIndicator31.ValidationSettings.RequiredField.IsRequired = isComplete;
        }

        /// <summary>
        /// This method populates and saves the form
        /// </summary>
        /// <param name="showMessages">Whether to show messages from the save</param>
        /// <returns>The success message type, null if the save failed</returns>
        private string SaveForm(bool showMessages)
        {
            //To hold the success message
            string successMessageType = null;

            //Determine if the user is allowed to save the form
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the fields of the object from the form
                currentBOQFCC.ProgramFK = Convert.ToInt32(ddProgram.Value);
                currentBOQFCC.FormDate = deFormDate.Date;
                currentBOQFCC.TeamMembers = txtTeamMembers.Text;
                currentBOQFCC.Indicator1 = (ddIndicator1.Value == null ? (int?)null : Convert.ToInt32(ddIndicator1.Value));
                currentBOQFCC.Indicator2 = (ddIndicator2.Value == null ? (int?)null : Convert.ToInt32(ddIndicator2.Value));
                currentBOQFCC.Indicator3 = (ddIndicator3.Value == null ? (int?)null : Convert.ToInt32(ddIndicator3.Value));
                currentBOQFCC.Indicator4 = (ddIndicator4.Value == null ? (int?)null : Convert.ToInt32(ddIndicator4.Value));
                currentBOQFCC.Indicator5 = (ddIndicator5.Value == null ? (int?)null : Convert.ToInt32(ddIndicator5.Value));
                currentBOQFCC.Indicator6 = (ddIndicator6.Value == null ? (int?)null : Convert.ToInt32(ddIndicator6.Value));
                currentBOQFCC.Indicator7 = (ddIndicator7.Value == null ? (int?)null : Convert.ToInt32(ddIndicator7.Value));
                currentBOQFCC.Indicator8 = (ddIndicator8.Value == null ? (int?)null : Convert.ToInt32(ddIndicator8.Value));
                currentBOQFCC.Indicator9 = (ddIndicator9.Value == null ? (int?)null : Convert.ToInt32(ddIndicator9.Value));
                currentBOQFCC.Indicator10 = (ddIndicator10.Value == null ? (int?)null : Convert.ToInt32(ddIndicator10.Value));
                currentBOQFCC.Indicator11 = (ddIndicator11.Value == null ? (int?)null : Convert.ToInt32(ddIndicator11.Value));
                currentBOQFCC.Indicator12 = (ddIndicator12.Value == null ? (int?)null : Convert.ToInt32(ddIndicator12.Value));
                currentBOQFCC.Indicator13 = (ddIndicator13.Value == null ? (int?)null : Convert.ToInt32(ddIndicator13.Value));
                currentBOQFCC.Indicator14 = (ddIndicator14.Value == null ? (int?)null : Convert.ToInt32(ddIndicator14.Value));
                currentBOQFCC.Indicator15 = (ddIndicator15.Value == null ? (int?)null : Convert.ToInt32(ddIndicator15.Value));
                currentBOQFCC.Indicator16 = (ddIndicator16.Value == null ? (int?)null : Convert.ToInt32(ddIndicator16.Value));
                currentBOQFCC.Indicator17 = (ddIndicator17.Value == null ? (int?)null : Convert.ToInt32(ddIndicator17.Value));
                currentBOQFCC.Indicator18 = (ddIndicator18.Value == null ? (int?)null : Convert.ToInt32(ddIndicator18.Value));
                currentBOQFCC.Indicator19 = (ddIndicator19.Value == null ? (int?)null : Convert.ToInt32(ddIndicator19.Value));
                currentBOQFCC.Indicator20 = (ddIndicator20.Value == null ? (int?)null : Convert.ToInt32(ddIndicator20.Value));
                currentBOQFCC.Indicator21 = (ddIndicator21.Value == null ? (int?)null : Convert.ToInt32(ddIndicator21.Value));
                currentBOQFCC.Indicator22 = (ddIndicator22.Value == null ? (int?)null : Convert.ToInt32(ddIndicator22.Value));
                currentBOQFCC.Indicator23 = (ddIndicator23.Value == null ? (int?)null : Convert.ToInt32(ddIndicator23.Value));
                currentBOQFCC.Indicator24 = (ddIndicator24.Value == null ? (int?)null : Convert.ToInt32(ddIndicator24.Value));
                currentBOQFCC.Indicator25 = (ddIndicator25.Value == null ? (int?)null : Convert.ToInt32(ddIndicator25.Value));
                currentBOQFCC.Indicator26 = (ddIndicator26.Value == null ? (int?)null : Convert.ToInt32(ddIndicator26.Value));
                currentBOQFCC.Indicator27 = (ddIndicator27.Value == null ? (int?)null : Convert.ToInt32(ddIndicator27.Value));
                currentBOQFCC.Indicator28 = (ddIndicator28.Value == null ? (int?)null : Convert.ToInt32(ddIndicator28.Value));
                currentBOQFCC.Indicator29 = (ddIndicator29.Value == null ? (int?)null : Convert.ToInt32(ddIndicator29.Value));
                currentBOQFCC.Indicator30 = (ddIndicator30.Value == null ? (int?)null : Convert.ToInt32(ddIndicator30.Value));
                currentBOQFCC.Indicator31 = (ddIndicator31.Value == null ? (int?)null : Convert.ToInt32(ddIndicator31.Value));
                currentBOQFCC.IsComplete = chkIsComplete.Checked;

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an edit
                        successMessageType = "BOQFCCEdited";

                        //Fill the edit-specific fields
                        currentBOQFCC.EditDate = DateTime.Now;
                        currentBOQFCC.Editor = User.Identity.Name;

                        //Get the existing database values
                        BenchmarkOfQualityFCC existingBOQFCC = context.BenchmarkOfQualityFCC.Find(currentBOQFCC.BenchmarkOfQualityFCCPK);

                        //Set the BOQ object to the new values
                        context.Entry(existingBOQFCC).CurrentValues.SetValues(currentBOQFCC);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfBOQFCCPK.Value = currentBOQFCC.BenchmarkOfQualityFCCPK.ToString();
                        BOQFCCPK = currentBOQFCC.BenchmarkOfQualityFCCPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an add
                        successMessageType = "BOQFCCAdded";

                        //Set the create-specific fields
                        currentBOQFCC.VersionNumber = 2;
                        currentBOQFCC.CreateDate = DateTime.Now;
                        currentBOQFCC.Creator = User.Identity.Name;

                        //Add the Benchmark to the database and save
                        context.BenchmarkOfQualityFCC.Add(currentBOQFCC);
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfBOQFCCPK.Value = currentBOQFCC.BenchmarkOfQualityFCCPK.ToString();
                        BOQFCCPK = currentBOQFCC.BenchmarkOfQualityFCCPK;
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