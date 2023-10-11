using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class ProgramActionPlanFCC : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "PAPFCC";
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
        private Models.ProgramActionPlanFCC currentActionPlan;
        private List<PyramidUser> allLeadershipCoaches;
        private int currentActionPlanPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ProgramActionPlanFCCPK"]))
            {
                //Parse the pk
                int.TryParse(Request.QueryString["ProgramActionPlanFCCPK"], out currentActionPlanPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentActionPlanPK == 0 && !string.IsNullOrWhiteSpace(hfProgramActionPlanFCCPK.Value))
            {
                int.TryParse(hfProgramActionPlanFCCPK.Value, out currentActionPlanPK);
            }

            //Check to see if this is an edit
            isEdit = currentActionPlanPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.ProgramActionPlanFCC.AsNoTracking()
                                        .Include(d => d.Program)
                                        .Include(d => d.Program.Cohort)
                                        .Where(d => d.ProgramActionPlanFCCPK == currentActionPlanPK).FirstOrDefault();

                //Check to see if the debrief exists
                if (currentActionPlan == null)
                {
                    //The debrief doesn't exist, set the debrief to a new debrief object
                    currentActionPlan = new Models.ProgramActionPlanFCC();
                }
                else
                {
                    //Show a message to the user if this is not the most recent action plan for the program
                    int numMoreRecentPlans = context.ProgramActionPlanFCC.AsNoTracking()
                                                    .Where(ap => ap.ProgramActionPlanFCCPK != currentActionPlan.ProgramActionPlanFCCPK &&
                                                                 ap.ProgramFK == currentActionPlan.ProgramFK &&
                                                                 ap.ActionPlanStartDate > currentActionPlan.ActionPlanStartDate)
                                                    .Count();

                    if (numMoreRecentPlans > 0)
                    {
                        divActionPlanAlert.Visible = true;
                        lblActionPlanAlert.Text = "This is not the most recent action plan for this program.  Any new or changed information should be added to the most recent action plan for this program.";
                    }
                }
            }

            //Prevent users from viewing action plans from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentActionPlan.ProgramFK))
            {
                //Add a message that will show after redirect
                msgSys.AddMessageToQueue("danger", "Not Found", "The action plan you are attempting to access does not exist.", 10000);

                //Redirect the user back to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Show the edit only div if this is an edit
                divEditOnly.Visible = isEdit;

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                //Get the leadership coaches
                BindLeadershipCoachList(currentActionPlan.ProgramFK);

                //Bind the drop-downs
                BindDropDowns();
                BindLeadershipCoachDropDown(currentActionPlan.ProgramFK, currentActionPlan.LeadershipCoachUsername);

                //Fill the form with data
                FillFormWithDataFromObject();

                if (isEdit)
                {
                    //Set the program and leadership coach labels
                    SetProgramLabels(currentActionPlan.ProgramFK);
                    SetLeadershipCoachLabels();

                    //Bind the tables
                    BindMeetings();
                    BindLeadershipCoachSchedule(currentActionPlan.ProgramFK);
                    BindLeadershipTeamMembers(currentActionPlan.ProgramFK);
                    BindGroundRules();

                    //Bind the program staff numbers
                    BindProgramStaffInformation(currentActionPlan.ProgramFK);

                    //Bind the BOQ information
                    BindMostRecentBOQInformation(currentActionPlan.ProgramFK);

                    //Bind the action steps
                    BindActionSteps();
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
                    lblPageTitle.Text = "Add New Family Child Care Action Plan";
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
                    lblPageTitle.Text = "Edit Family Child Care Action Plan";
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
                    lblPageTitle.Text = "View Family Child Care Action Plan";
                }

                //Set focus to the first field
                ddProgram.Focus();

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

        #region Main Debrief Form 

        /// <summary>
        /// This method fills the input fields with data from the relevant object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Check if this is an edit or not
            if (isEdit)
            {
                //Set the prefill hidden field
                hfDisplayReviewSections.Value = (currentActionPlan.IsPrefilled && currentActionPlan.IsFullyReviewed == false ? "True" : "False");

                //Fill the review checkboxes
                chkReviewedActionSteps.Checked = currentActionPlan.ReviewedActionSteps;
                chkReviewedBasicInfo.Checked = currentActionPlan.ReviewedBasicInfo;
                chkReviewedGroundRules.Checked = currentActionPlan.ReviewedGroundRules;

                //Fill the input fields
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentActionPlan.ProgramFK);
                deActionPlanStartDate.Value = currentActionPlan.ActionPlanStartDate;
                deActionPlanEndDate.Value = currentActionPlan.ActionPlanEndDate;
                ddIsLeadershipCoachInvolved.Value = currentActionPlan.IsLeadershipCoachInvolved;
                ddLeadershipCoach.Value = currentActionPlan.LeadershipCoachUsername;
                txtMissionStatement.Text = currentActionPlan.MissionStatement;
                txtAdditionalNotes.Text = currentActionPlan.AdditionalNotes;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            chkReviewedActionSteps.ClientEnabled = enabled;
            chkReviewedBasicInfo.ClientEnabled = enabled;
            chkReviewedGroundRules.ClientEnabled = enabled;
            ddProgram.ClientEnabled = enabled;
            deActionPlanStartDate.ClientEnabled = enabled;
            deActionPlanEndDate.ClientEnabled = enabled;
            ddIsLeadershipCoachInvolved.ClientEnabled = enabled;
            ddLeadershipCoach.ClientEnabled = enabled;
            txtMissionStatement.ClientEnabled = enabled;
            txtAdditionalNotes.ClientEnabled = enabled;

            //Show/hide the submit button
            submitActionPlan.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitActionPlan.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            //To hold all the necessary items
            List<Program> allPrograms = new List<Program>();
            List<CodeBOQIndicator> allIndicators = new List<CodeBOQIndicator>();
            List<CodeActionPlanActionStepStatus> allActionStepStatuses = new List<CodeActionPlanActionStepStatus>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allPrograms = context.Program.AsNoTracking()
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK) &&
                                                    p.ProgramType.Where(pt => pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE ||
                                                                            pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE).Count() > 0)
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();

                allIndicators = context.CodeBOQIndicator
                                        .Include(i => i.CodeBOQCriticalElement)
                                        .AsNoTracking()
                                        .Where(i => i.CodeBOQCriticalElement.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQFCCV2)
                                        .OrderBy(i => i.OrderBy)
                                        .ToList();

                allActionStepStatuses = context.CodeActionPlanActionStepStatus.AsNoTracking()
                                                .OrderBy(s => s.OrderBy)
                                                .ToList();
            }

            //Bind the program dropdown
            ddProgram.DataSource = allPrograms;
            ddProgram.DataBind();

            //Bind the indicator dropdown
            ddActionStepIndicator.DataSource = allIndicators.Select(i => new
            {
                i.CodeBOQIndicatorPK,
                IndicatorNumAndElement = string.Format("({0}) {1}", i.CodeBOQCriticalElement.Abbreviation, i.IndicatorNumber)
            });
            ddActionStepIndicator.DataBind();

            //Bind the action step status dropdowns
            ddActionStepInitialStatus.DataSource = allActionStepStatuses;
            ddActionStepInitialStatus.DataBind();

            ddActionStepStatus.DataSource = allActionStepStatuses;
            ddActionStepStatus.DataBind();
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
                    Program selectedProgram = context.Program.AsNoTracking().Include(p => p.Cohort).Where(p => p.ProgramPK == currentProgramFK.Value).FirstOrDefault();

                    //Make sure the program exists
                    if (selectedProgram != null)
                    {
                        //Set the program-specific labels
                        lblProgramStartDate.Text = selectedProgram.ProgramStartDate.ToString("MM/dd/yyyy");
                        lblProgramCohort.Text = selectedProgram.Cohort.CohortName;
                    }
                }
            }
        }

        /// <summary>
        /// This method binds the list of leadership coaches
        /// </summary>
        /// <param name="currentProgramFK">The program FK used to filter the dropdown</param>
        private void BindLeadershipCoachList(int? currentProgramFK)
        {
            //Make sure the program is valid
            if (currentProgramFK.HasValue && currentProgramFK.Value > 0)
            {
                if (currentProgramFK.Value == currentActionPlan.ProgramFK)
                {
                    //Get the leadership coaches for the program and
                    //always include the coach saved to the form in case
                    //the coach's role was removed.
                    allLeadershipCoaches = PyramidUser.GetProgramLeadershipCoachUserRecords(new List<int> { currentProgramFK.Value }, currentActionPlan.LeadershipCoachUsername);
                }
                else
                {
                    //Get the leadership coaches for the program
                    allLeadershipCoaches = PyramidUser.GetProgramLeadershipCoachUserRecords(new List<int> { currentProgramFK.Value });
                }
            }
            else
            {
                //Set to a blank list
                allLeadershipCoaches = new List<PyramidUser>();
            }
        }

        /// <summary>
        /// This method populates the leadership coach dropdown based on the passed parameters
        /// </summary>
        /// <param name="currentProgramFK">The program FK used to filter the dropdown</param>
        /// <param name="currentLCUsername">The current selected leadership coach username (if any)</param>
        private void BindLeadershipCoachDropDown(int? currentProgramFK, string currentLCUsername)
        {
            //Make sure the program is valid
            if (currentProgramFK.HasValue && currentProgramFK.Value > 0)
            {
                //Bind the leadership coach dropdown
                var leadershipCoaches = allLeadershipCoaches.Select(lc => new { lc.UserName, FullName = string.Format("{0} {1}", lc.FirstName, lc.LastName) }).ToList();
                ddLeadershipCoach.DataSource = leadershipCoaches;
                ddLeadershipCoach.DataBind();

                //Check to see how many coaches there are
                if (ddLeadershipCoach.Items.Count > 0)
                {
                    //There is at least 1 coach, enable the child dropdown
                    ddLeadershipCoach.ReadOnly = false;

                    //Try to select the coach passed to this method
                    ddLeadershipCoach.SelectedItem = ddLeadershipCoach.Items.FindByValue(currentLCUsername);
                }
                else
                {
                    //There are no coaches in the list, disable the coach dropdown
                    ddLeadershipCoach.Value = "";
                    ddLeadershipCoach.ReadOnly = true;
                }
            }
            else
            {
                //Clear the leadership coach dropdown and disable it
                ddLeadershipCoach.Value = "";
                ddLeadershipCoach.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method fills the leadership coach labels
        /// </summary>
        private void SetLeadershipCoachLabels()
        {
            //To hold the necessary values
            string leadershipCoachUsername;

            //Get the program FK
            leadershipCoachUsername = (ddLeadershipCoach.Value == null ? null : Convert.ToString(ddLeadershipCoach.Value));

            //Make sure the selected program is valid before continuing
            if (!string.IsNullOrWhiteSpace(leadershipCoachUsername))
            {
                //Get the user record
                PyramidUser leadershipCoachUserRecord = PyramidUser.GetUserRecordByUsername(leadershipCoachUsername);

                if (leadershipCoachUserRecord != null)
                {
                    //Set the email field
                    lblLeadershipCoachEmail.Text = leadershipCoachUserRecord.Email;
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitActionPlan user control 
        /// </summary>
        /// <param name="sender">The submitActionPlan control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionPlan_Click(object sender, EventArgs e)
        {
            //Try to save the form to the database
            bool formSaved = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (formSaved)
            {
                //Redirect differently if add or edit
                if (isEdit)
                {
                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "Action Plan successfully edited!", 1000);

                    //Redirect the user to the dashboard
                    Response.Redirect(string.Format("/Pages/PLTDashboard.aspx"));
                }
                else
                {
                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Action Plan was successfully added!<br/><br/>More detailed information can now be added to the Action Plan.", 10000);

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK={0}&Action=Edit",
                                                        currentActionPlanPK));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitActionPlan user control 
        /// </summary>
        /// <param name="sender">The submitActionPlan control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionPlan_CancelClick(object sender, EventArgs e)
        {
            //Show a message after redirect
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect the user to the dashboard
            Response.Redirect("/Pages/PLTDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitActionPlan control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitActionPlan_ValidationFailed(object sender, EventArgs e)
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
            //Print the form
            PrintForm();
        }

        /// <summary>
        /// This method prints the form
        /// </summary>
        private void PrintForm()
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitActionPlan.ValidationGroup))
            {
                //Submit the form
                bool formSaved = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptProgramActionPlanFCC report = new Reports.PreBuiltReports.FormReports.RptProgramActionPlanFCC();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Family Child Care Action Plan", currentActionPlanPK);
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
                    msgSys.AddMessageToQueue("success", "Success", "The Action Plan was successfully saved!", 12000);

                    //Redirect the user back to this page with the PK
                    Response.Redirect(string.Format("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK={0}&Action={1}&Print=True",
                                                        currentActionPlanPK, action));
                }
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
        private bool SaveForm(bool showMessages)
        {
            //Whether or not the save succeeded
            bool didSaveSucceed = false;

            //Check the user permissions
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentActionPlan.ReviewedActionSteps = chkReviewedActionSteps.Checked;
                currentActionPlan.ReviewedBasicInfo = chkReviewedBasicInfo.Checked;
                currentActionPlan.ReviewedGroundRules = chkReviewedGroundRules.Checked;
                currentActionPlan.ActionPlanStartDate = deActionPlanStartDate.Date;
                currentActionPlan.ActionPlanEndDate = deActionPlanEndDate.Date;
                currentActionPlan.IsLeadershipCoachInvolved = Convert.ToBoolean(ddIsLeadershipCoachInvolved.Value);
                currentActionPlan.LeadershipCoachUsername = (Convert.ToBoolean(ddIsLeadershipCoachInvolved.Value) ? Convert.ToString(ddLeadershipCoach.Value) : null);
                currentActionPlan.MissionStatement = (string.IsNullOrWhiteSpace(txtMissionStatement.Text) ? null : txtMissionStatement.Text);
                currentActionPlan.AdditionalNotes = (string.IsNullOrWhiteSpace(txtAdditionalNotes.Text) ? null : txtAdditionalNotes.Text);
                currentActionPlan.ProgramFK = Convert.ToInt32(ddProgram.Value);

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit-only fields
                        currentActionPlan.EditDate = DateTime.Now;
                        currentActionPlan.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.ProgramActionPlanFCC existingActionPlan = context.ProgramActionPlanFCC.Find(currentActionPlan.ProgramActionPlanFCCPK);

                        //Set the object to the new values
                        context.Entry(existingActionPlan).CurrentValues.SetValues(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfProgramActionPlanFCCPK.Value = currentActionPlan.ProgramActionPlanFCCPK.ToString();
                        currentActionPlanPK = currentActionPlan.ProgramActionPlanFCCPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the create-only fields
                        currentActionPlan.CreateDate = DateTime.Now;
                        currentActionPlan.Creator = User.Identity.Name;

                        //Add it to the context
                        context.ProgramActionPlanFCC.Add(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfProgramActionPlanFCCPK.Value = currentActionPlan.ProgramActionPlanFCCPK.ToString();
                        currentActionPlanPK = currentActionPlan.ProgramActionPlanFCCPK;

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
        /// This method fires when the validation for the ddProgram DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddProgram BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddProgram_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedProgramFK;

            //Get the program FK
            selectedProgramFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

            //Validate
            if (selectedProgramFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Program is required!";
            }
            else if (currentActionPlan.IsPrefilled == false || chkReviewedBasicInfo.Checked)
            {
                //Only do this validation if a program was selected, and the form was either not prefilled or the section was reviewed
                //Check for duplication
                if (deActionPlanStartDate.Date != DateTime.MinValue && deActionPlanEndDate.Date != DateTime.MinValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check to see if there are any schedules
                        List<Models.ProgramActionPlanFCC> duplicateDebriefs = context.ProgramActionPlanFCC.AsNoTracking()
                                        .Where(d => d.ProgramFK == selectedProgramFK.Value &&
                                                d.ProgramActionPlanFCCPK != currentActionPlanPK &&
                                                ((d.ActionPlanStartDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanStartDate >= deActionPlanStartDate.Date) ||
                                                (d.ActionPlanEndDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanEndDate >= deActionPlanStartDate.Date))).ToList();

                        //Check the count of duplicate schedules
                        if (duplicateDebriefs.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "There is already an action plan for the selected program within the timeframe entered here!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddLeadershipCoach DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddLeadershipCoach BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddLeadershipCoach_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the leadership coach is involved
            if (ddIsLeadershipCoachInvolved.Value != null &&
                bool.TryParse(ddIsLeadershipCoachInvolved.Value.ToString(), out bool isLeadershipCoachInvolved) &&
                isLeadershipCoachInvolved == true)
            {
                //Get the selected leadership coach's username
                string leadershipCoachUsername = (ddLeadershipCoach.Value == null ? null : Convert.ToString(ddLeadershipCoach.Value));

                //Get the selected program FK
                int? selectedProgramFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

                if (string.IsNullOrWhiteSpace(leadershipCoachUsername))
                {
                    e.IsValid = false;
                    e.ErrorText = "Primary Leadership Coach is required!";
                }
                else if (selectedProgramFK.HasValue)
                {
                    //Both the coach and program are selected, make sure the coach is allowed for that program
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the matching leadership coach roles for the selected leadership coach and the selected program
                        List<UserProgramRole> matchingLeadershipCoachRoles = context.UserProgramRole.AsNoTracking()
                                .Include(upr => upr.Program)
                                .Where(upr => upr.Username == leadershipCoachUsername &&
                                                (upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                                                    upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH) &&
                                                upr.ProgramFK == selectedProgramFK)
                                .ToList();

                        //Validate
                        if (currentActionPlan.ProgramFK == selectedProgramFK &&
                                currentActionPlan.LeadershipCoachUsername == leadershipCoachUsername)
                        {
                            //Allow saving the action plan if the program FK and leadership coach have not changed since the leadership coach
                            //may lose the role in their account, but we don't have a date for that.
                            e.IsValid = true;
                        }
                        else if (matchingLeadershipCoachRoles == null || matchingLeadershipCoachRoles.Count <= 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "That combination of Program and Primary Leadership Coach is not valid!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deActionPlanStartDate DevExpress
        /// BootstrapDateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deActionPlanStartDate BootstrapDateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deActionPlanStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Check to see if the start date was entered
            if (actionPlanStartDate == DateTime.MinValue)
            {
                e.IsValid = false;
                e.ErrorText = "Action Plan Start Date is required!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the deActionPlanEndDate DevExpress
        /// BootstrapDateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deActionPlanEndDate BootstrapDateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deActionPlanEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Check to see if the end date was entered
            if (actionPlanEndDate == DateTime.MinValue)
            {
                e.IsValid = false;
                e.ErrorText = "Action Plan End Date is required!";
            }
            else if (actionPlanStartDate != DateTime.MinValue)
            {
                if (actionPlanEndDate < actionPlanStartDate)
                {
                    e.IsValid = false;
                    e.ErrorText = "Action Plan End Date must be after the Action Plan Start Date!";
                }
                else if (actionPlanEndDate.AddYears(-1) > actionPlanStartDate)
                {
                    e.IsValid = false;
                    e.ErrorText = "The total time period between the Action Plan Start Date and Action Plan End Date must be one year or less!";
                }
                else if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check for invalid meeting dates
                        List<Models.ProgramActionPlanFCCMeeting> invalidMeetingDates = context.ProgramActionPlanFCCMeeting
                                                    .AsNoTracking()
                                                    .Where(m => m.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK &&
                                                                (m.MeetingDate < actionPlanStartDate ||
                                                                    m.MeetingDate > actionPlanEndDate))
                                                    .ToList();

                        if (invalidMeetingDates.Count > 0)
                        {
                            //Set the validation
                            e.IsValid = false;
                            e.ErrorText = "At least one meeting date is invalid because it is not between the Action Plan Start Date and Action Plan End Date!  See the notification message for details.";

                            //Format the invalid attendee and session information into a list of strings
                            List<string> errorTextList = invalidMeetingDates.Select(a => string.Format("Meeting Date: {0:MM/dd/yyyy}", a.MeetingDate)).ToList();

                            //Create a single string that is separated by breaks
                            string errorTextString = string.Join("<br/>", errorTextList);

                            //Show the message
                            msgSys.ShowMessageToUser("danger", "Meeting Date Validation Error", string.Format("Invalid meeting dates:<br/><br/>{0}", errorTextString), 200000);
                        }

                        //Check for invalid action steps if the form was not prefilled or the action steps are
                        //marked as reviewed
                        if (currentActionPlan.IsPrefilled == false || chkReviewedActionSteps.Checked)
                        {
                            //Check for invalid action steps
                            List<Models.ProgramActionPlanFCCActionStep> invalidActionSteps = context.ProgramActionPlanFCCActionStep
                                                        .Include(s => s.CodeBOQIndicator)
                                                        .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                        .AsNoTracking()
                                                        .Where(s => s.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK &&
                                                                    (s.TargetDate < actionPlanStartDate ||
                                                                        s.TargetDate > actionPlanEndDate))
                                                        .ToList();

                            if (invalidActionSteps.Count > 0)
                            {
                                //Set the validation
                                e.IsValid = false;
                                e.ErrorText = "At least one action step is invalid because the target date is not between the Action Plan Start Date and Action Plan End Date.  See the notification message for details.";

                                //Format the invalid attendee and session information into a list of strings
                                List<string> errorTextList = invalidActionSteps.Select(s => string.Format("Critical Element: {0}, Indicator: {1}, Target Date: {2:MM/dd/yyyy}",
                                                                                            s.CodeBOQIndicator.CodeBOQCriticalElement.Abbreviation,
                                                                                            s.CodeBOQIndicator.IndicatorNumber,
                                                                                            s.TargetDate)).ToList();

                                //Create a single string that is separated by breaks
                                string errorTextString = string.Join("<br/>", errorTextList);

                                //Show the message
                                msgSys.ShowMessageToUser("danger", "Action Step Validation Error", string.Format("Invalid action steps:<br/><br/>{0}", errorTextString), 200000);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user changes the dates for the action plan.
        /// </summary>
        /// <param name="sender">A BootstrapDateEdit</param>
        /// <param name="e">EventArgs</param>
        protected void ActionPlanDate_ValueChanged(object sender, EventArgs e)
        {
            var currentDateEdit = (BootstrapDateEdit)sender;

            //Get the selected program
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);

            //Bind the leadership coach list
            BindLeadershipCoachList(selectedProgramFK);

            //Bind the leadership coach schedules
            BindLeadershipCoachSchedule(selectedProgramFK);

            //Bind the program leadership team members
            BindLeadershipTeamMembers(selectedProgramFK);

            //Bind the program staff numbers
            BindProgramStaffInformation(selectedProgramFK);

            //Bind the BOQ information
            BindMostRecentBOQInformation(selectedProgramFK);

            //Re-focus on the control
            currentDateEdit.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a program from the ddProgram control.
        /// </summary>
        /// <param name="sender">The ddProgram BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddProgram_ValueChanged(object sender, EventArgs e)
        {
            //Get the selected program
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);

            //Get the selected leadership coach
            string selectedLCUsername = (ddLeadershipCoach.Value != null ? ddLeadershipCoach.Value.ToString() : null);

            //Set the labels 
            SetProgramLabels(selectedProgramFK);

            //Bind the leadership coach list
            BindLeadershipCoachList(selectedProgramFK);

            //Bind the leadership coach dropdown
            BindLeadershipCoachDropDown(selectedProgramFK, selectedLCUsername);

            //Bind the leadership coach schedules
            BindLeadershipCoachSchedule(selectedProgramFK);

            //Bind the program leadership team members
            BindLeadershipTeamMembers(selectedProgramFK);

            //Bind the program staff numbers
            BindProgramStaffInformation(selectedProgramFK);

            //Bind the BOQ information
            BindMostRecentBOQInformation(selectedProgramFK);

            //Re-focus on the control
            ddProgram.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a program from the ddLeadershipCoach control.
        /// </summary>
        /// <param name="sender">The ddLeadershipCoach BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddLeadershipCoach_ValueChanged(object sender, EventArgs e)
        {
            //Set the labels 
            SetLeadershipCoachLabels();

            //Re-focus on the control
            ddLeadershipCoach.Focus();
        }

        #endregion

        #region Meetings

        /// <summary>
        /// This method populates the meeting repeater with up-to-date information
        /// </summary>
        private void BindMeetings()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<ProgramActionPlanFCCMeeting> Meetings = context.ProgramActionPlanFCCMeeting.AsNoTracking()
                                            .Where(t => t.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK)
                                            .ToList();
                repeatMeetings.DataSource = Meetings;
                repeatMeetings.DataBind();
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetMeetingControlUsability(bool enabled)
        {
            //Enable/disable the controls
            deMeetingDate.ClientEnabled = enabled;
            ddMeetingLeadershipCoachAttendance.ClientEnabled = enabled;
            txtMeetingNotes.ClientEnabled = enabled;

            //Show/hide the submit button
            submitMeeting.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitMeeting.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitMeeting.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the meetings
        /// and it opens a div that allows the user to add a meeting
        /// </summary>
        /// <param name="sender">The lbAddMeeting LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddMeeting_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditMeetingPK.Value = "0";
            deMeetingDate.Value = "";
            ddMeetingLeadershipCoachAttendance.Value = "";
            txtMeetingNotes.Text = "";

            //Set the title
            lblAddEditMeeting.Text = "Add Meeting";

            //Show the input div
            divAddEditMeeting.Visible = true;

            //Set focus to the meeting date field
            deMeetingDate.Focus();

            //Enable the controls
            SetMeetingControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a meeting
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewMeeting LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewMeeting_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblMeetingPK = (Label)item.FindControl("lblMeetingPK");

            //Get the PK from the label
            int? meetingPK = (string.IsNullOrWhiteSpace(lblMeetingPK.Text) ? (int?)null : Convert.ToInt32(lblMeetingPK.Text));

            if (meetingPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the meeting to view
                    ProgramActionPlanFCCMeeting currentMeeting = context.ProgramActionPlanFCCMeeting.AsNoTracking().Where(m => m.ProgramActionPlanFCCMeetingPK == meetingPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditMeeting.Text = "View Meeting";
                    deMeetingDate.Date = currentMeeting.MeetingDate;
                    ddMeetingLeadershipCoachAttendance.SelectedItem = ddMeetingLeadershipCoachAttendance.Items.FindByValue(currentMeeting.LeadershipCoachAttendance);
                    txtMeetingNotes.Text = currentMeeting.MeetingNotes;
                    hfAddEditMeetingPK.Value = meetingPK.Value.ToString();
                }

                //Show the edit div
                divAddEditMeeting.Visible = true;

                //Set focus to the first field
                deMeetingDate.Focus();

                //Disable the controls since this is a view
                SetMeetingControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected meeting!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a meeting
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditMeeting LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditMeeting_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblMeetingPK = (Label)item.FindControl("lblMeetingPK");

            //Get the PK from the label
            int? meetingPK = (string.IsNullOrWhiteSpace(lblMeetingPK.Text) ? (int?)null : Convert.ToInt32(lblMeetingPK.Text));

            if (meetingPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the meeting to edit
                    ProgramActionPlanFCCMeeting editMeeting = context.ProgramActionPlanFCCMeeting.AsNoTracking().Where(m => m.ProgramActionPlanFCCMeetingPK == meetingPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditMeeting.Text = "Edit Meeting";
                    deMeetingDate.Date = editMeeting.MeetingDate;
                    ddMeetingLeadershipCoachAttendance.SelectedItem = ddMeetingLeadershipCoachAttendance.Items.FindByValue(editMeeting.LeadershipCoachAttendance);
                    txtMeetingNotes.Text = editMeeting.MeetingNotes;
                    hfAddEditMeetingPK.Value = meetingPK.Value.ToString();
                }

                //Show the edit div
                divAddEditMeeting.Visible = true;

                //Set focus to the first field
                deMeetingDate.Focus();

                //Enable the controls
                SetMeetingControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected meeting!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the meeting
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitMeeting submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitMeeting_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditMeetingPK.Value = "0";
            divAddEditMeeting.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitMeeting_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the meeting
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitMeeting submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitMeeting_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the meeting PK
                int meetingPK = Convert.ToInt32(hfAddEditMeetingPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    ProgramActionPlanFCCMeeting currentMeeting;

                    //Check to see if this is an add or an edit
                    if (meetingPK == 0)
                    {
                        //Add
                        currentMeeting = new ProgramActionPlanFCCMeeting();
                        currentMeeting.MeetingDate = deMeetingDate.Date;
                        currentMeeting.LeadershipCoachAttendance = Convert.ToBoolean(ddMeetingLeadershipCoachAttendance.Value);
                        currentMeeting.MeetingNotes = (string.IsNullOrWhiteSpace(txtMeetingNotes.Text) ? null : txtMeetingNotes.Text);
                        currentMeeting.ProgramActionPlanFCCFK = currentActionPlanPK;
                        currentMeeting.CreateDate = DateTime.Now;
                        currentMeeting.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramActionPlanFCCMeeting.Add(currentMeeting);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added meeting!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentMeeting = context.ProgramActionPlanFCCMeeting.Find(meetingPK);
                        currentMeeting.MeetingDate = deMeetingDate.Date;
                        currentMeeting.LeadershipCoachAttendance = Convert.ToBoolean(ddMeetingLeadershipCoachAttendance.Value);
                        currentMeeting.MeetingNotes = (string.IsNullOrWhiteSpace(txtMeetingNotes.Text) ? null : txtMeetingNotes.Text);
                        currentMeeting.EditDate = DateTime.Now;
                        currentMeeting.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited meeting!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditMeetingPK.Value = "0";
                    divAddEditMeeting.Visible = false;

                    //Rebind the meeting table
                    BindMeetings();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a meeting
        /// and it deletes the meeting information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteMeeting LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteMeeting_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit meeting information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteMeetingPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteMeetingPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the meeting to remove
                            ProgramActionPlanFCCMeeting meetingToRemove = context.ProgramActionPlanFCCMeeting.Where(t => t.ProgramActionPlanFCCMeetingPK == rowToRemovePK).FirstOrDefault();

                            //Remove the meeting
                            context.ProgramActionPlanFCCMeeting.Remove(meetingToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.ProgramActionPlanFCCMeetingChanged
                                    .OrderByDescending(c => c.ProgramActionPlanFCCMeetingChangedPK)
                                    .Where(c => c.ProgramActionPlanFCCMeetingPK == meetingToRemove.ProgramActionPlanFCCMeetingPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the meeting table
                            BindMeetings();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the meeting!", 10000);
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the meeting, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the meeting to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the deMeetingDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deMeetingDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deMeetingDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates
            DateTime? meetingDate = (deMeetingDate.Value == null ? (DateTime?)null : deMeetingDate.Date);
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);

            //Validate
            if (actionPlanStartDate.HasValue == false || actionPlanEndDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The action plan start date and action plan end date in the basic information section must be entered before the Meeting Date can be evaluated.";
            }
            else if (meetingDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Meeting Date is required!";
            }
            else if (meetingDate.Value < actionPlanStartDate)
            {
                e.IsValid = false;
                e.ErrorText = "The Meeting Date must be on or after the action plan start date in the basic information section!";
            }
            else if (meetingDate.Value > actionPlanEndDate)
            {
                e.IsValid = false;
                e.ErrorText = "The Meeting Date must be on or before the action plan end date in the basic information section!";
            }
        }

        #endregion

        #region Leadership Team Members

        /// <summary>
        /// This method populates the leadership team member repeater with up-to-date information
        /// </summary>
        /// <param name="currentProgramFK">The program FK to filter the list</param>
        private void BindLeadershipTeamMembers(int? currentProgramFK)
        {
            //Make sure the dates and program are valid
            if (deActionPlanStartDate.Date > DateTime.MinValue &&
                deActionPlanEndDate.Date > DateTime.MinValue &&
                currentProgramFK.HasValue &&
                currentProgramFK.Value > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedules for the program and timeframe combination
                    List<Models.PLTMember> activePLTMembers = context.PLTMember.AsNoTracking()
                                                .Include(tm => tm.Program)
                                                .Where(tm => tm.ProgramFK == currentProgramFK.Value &&
                                                             tm.StartDate <= deActionPlanEndDate.Date &&
                                                             (tm.LeaveDate.HasValue == false ||
                                                                tm.LeaveDate >= deActionPlanStartDate.Date))
                                                .ToList();

                    //Bind the repeater
                    repeatLeadershipTeamMembers.DataSource = activePLTMembers;
                    repeatLeadershipTeamMembers.DataBind();
                }
            }
            else
            {
                //Clear the repeater
                repeatLeadershipTeamMembers.DataSource = new List<Models.PLTMember>();
                repeatLeadershipTeamMembers.DataBind();
            }
        }

        #endregion

        #region Leadership Coach Schedule

        /// <summary>
        /// This method populates the leadership coach schedule repeater with up-to-date information
        /// </summary>
        /// <param name="currentProgramFK">The program FK to filter the schedules</param>
        private void BindLeadershipCoachSchedule(int? currentProgramFK)
        {
            //Make sure the dates and program are valid
            if (deActionPlanStartDate.Date > DateTime.MinValue &&
                deActionPlanEndDate.Date > DateTime.MinValue &&
                currentProgramFK.HasValue &&
                currentProgramFK.Value > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedules for the program and timeframe combination
                    List<ProgramLCMeetingSchedule> programLCSchedules = context.ProgramLCMeetingSchedule.AsNoTracking()
                                                .Include(p => p.Program)
                                                .Where(p => p.ProgramFK == currentProgramFK.Value &&
                                                            (p.MeetingYear == deActionPlanStartDate.Date.Year ||
                                                                p.MeetingYear == deActionPlanEndDate.Date.Year))
                                                .ToList();

                    //Set the full leadership coach name
                    foreach (ProgramLCMeetingSchedule schedule in programLCSchedules)
                    {
                        //Get the leadership coach user record
                        PyramidUser leadershipCoachUserRecord = allLeadershipCoaches.Where(c => c.UserName == schedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUserRecord != null)
                        {
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                        else
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are program-specific and can be removed
                            leadershipCoachUserRecord = PyramidUser.GetUserRecordByUsername(schedule.LeadershipCoachUsername);

                            //Set the label text (include the username for searching
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                    }

                    //Bind the repeater
                    repeatLeadershipCoachSchedule.DataSource = programLCSchedules;
                    repeatLeadershipCoachSchedule.DataBind();
                }
            }
            else
            {
                //Clear the repeater
                repeatLeadershipCoachSchedule.DataSource = new List<ProgramLCMeetingSchedule>();
                repeatLeadershipCoachSchedule.DataBind();
            }
        }

        #endregion

        #region Ground Rules

        /// <summary>
        /// This method populates the ground rule repeater with up-to-date information
        /// </summary>
        private void BindGroundRules()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<ProgramActionPlanFCCGroundRule> GroundRules = context.ProgramActionPlanFCCGroundRule.AsNoTracking()
                                            .Where(t => t.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK)
                                            .ToList();
                repeatGroundRules.DataSource = GroundRules;
                repeatGroundRules.DataBind();
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetGroundRuleControlUsability(bool enabled)
        {
            //Enable/disable the controls
            txtGroundRuleDescription.ClientEnabled = enabled;
            txtGroundRuleNumber.ClientEnabled = enabled;

            //Show/hide the submit button
            submitGroundRule.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitGroundRule.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitGroundRule.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the ground rules
        /// and it opens a div that allows the user to add a ground rule
        /// </summary>
        /// <param name="sender">The lbAddGroundRule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddGroundRule_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditGroundRulePK.Value = "0";
            txtGroundRuleDescription.Text = "";
            txtGroundRuleNumber.Text = "";

            //Set the title
            lblAddEditGroundRule.Text = "Add Ground Rule";

            //Show the input div
            divAddEditGroundRule.Visible = true;

            //Set focus to the position field
            txtGroundRuleNumber.Focus();

            //Enable the controls
            SetGroundRuleControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a ground rule
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewGroundRule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewGroundRule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblGroundRulePK = (Label)item.FindControl("lblGroundRulePK");

            //Get the PK from the label
            int? groundRulePK = (string.IsNullOrWhiteSpace(lblGroundRulePK.Text) ? (int?)null : Convert.ToInt32(lblGroundRulePK.Text));

            if (groundRulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the ground rule to view
                    ProgramActionPlanFCCGroundRule currentGroundRule = context.ProgramActionPlanFCCGroundRule.AsNoTracking().Where(m => m.ProgramActionPlanFCCGroundRulePK == groundRulePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditGroundRule.Text = "View Ground Rule";
                    txtGroundRuleDescription.Text = currentGroundRule.GroundRuleDescription;
                    txtGroundRuleNumber.Text = currentGroundRule.GroundRuleNumber.ToString();
                    hfAddEditGroundRulePK.Value = groundRulePK.Value.ToString();
                }

                //Show the edit div
                divAddEditGroundRule.Visible = true;

                //Set focus to the first field
                txtGroundRuleNumber.Focus();

                //Disable the controls since this is a view
                SetGroundRuleControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected ground rule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a ground rule
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditGroundRule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditGroundRule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblGroundRulePK = (Label)item.FindControl("lblGroundRulePK");

            //Get the PK from the label
            int? groundRulePK = (string.IsNullOrWhiteSpace(lblGroundRulePK.Text) ? (int?)null : Convert.ToInt32(lblGroundRulePK.Text));

            if (groundRulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the ground rule to edit
                    ProgramActionPlanFCCGroundRule editGroundRule = context.ProgramActionPlanFCCGroundRule.AsNoTracking().Where(m => m.ProgramActionPlanFCCGroundRulePK == groundRulePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditGroundRule.Text = "Edit Ground Rule";
                    txtGroundRuleDescription.Text = editGroundRule.GroundRuleDescription;
                    txtGroundRuleNumber.Text = editGroundRule.GroundRuleNumber.ToString();
                    hfAddEditGroundRulePK.Value = groundRulePK.Value.ToString();
                }

                //Show the edit div
                divAddEditGroundRule.Visible = true;

                //Set focus to the first field
                txtGroundRuleNumber.Focus();

                //Enable the controls
                SetGroundRuleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected ground rule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the ground rule
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitGroundRule submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitGroundRule_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditGroundRulePK.Value = "0";
            divAddEditGroundRule.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitGroundRule_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the ground rule
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitGroundRule submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitGroundRule_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the ground rule PK
                int groundRulePK = Convert.ToInt32(hfAddEditGroundRulePK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    ProgramActionPlanFCCGroundRule currentGroundRule;

                    //Check to see if this is an add or an edit
                    if (groundRulePK == 0)
                    {
                        //Add
                        currentGroundRule = new ProgramActionPlanFCCGroundRule();
                        currentGroundRule.GroundRuleDescription = txtGroundRuleDescription.Text;
                        currentGroundRule.GroundRuleNumber = Convert.ToInt32(txtGroundRuleNumber.Text);
                        currentGroundRule.ProgramActionPlanFCCFK = currentActionPlanPK;
                        currentGroundRule.CreateDate = DateTime.Now;
                        currentGroundRule.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramActionPlanFCCGroundRule.Add(currentGroundRule);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added ground rule!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentGroundRule = context.ProgramActionPlanFCCGroundRule.Find(groundRulePK);
                        currentGroundRule.GroundRuleDescription = txtGroundRuleDescription.Text;
                        currentGroundRule.GroundRuleNumber = Convert.ToInt32(txtGroundRuleNumber.Text);
                        currentGroundRule.EditDate = DateTime.Now;
                        currentGroundRule.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited ground rule!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditGroundRulePK.Value = "0";
                    divAddEditGroundRule.Visible = false;

                    //Rebind the ground rule table
                    BindGroundRules();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a ground rule
        /// and it deletes the ground rule information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteGroundRule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteGroundRule_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit ground rule information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteGroundRulePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteGroundRulePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the ground rule to remove
                            ProgramActionPlanFCCGroundRule assignmentToRemove = context.ProgramActionPlanFCCGroundRule.Where(t => t.ProgramActionPlanFCCGroundRulePK == rowToRemovePK).FirstOrDefault();

                            //Remove the ground rule
                            context.ProgramActionPlanFCCGroundRule.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.ProgramActionPlanFCCGroundRuleChanged
                                    .OrderByDescending(c => c.ProgramActionPlanFCCGroundRuleChangedPK)
                                    .Where(c => c.ProgramActionPlanFCCGroundRulePK == assignmentToRemove.ProgramActionPlanFCCGroundRulePK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the ground rule table
                            BindGroundRules();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the ground rule!", 10000);
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the ground rule, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the ground rule!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the ground rule!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the ground rule to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the txtGroundRuleDescription field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The txtGroundRuleDescription BootstrapMemo</param>
        /// <param name="e">The validation event arguments</param>
        protected void txtGroundRuleDescription_Validation(object sender, ValidationEventArgs e)
        {
            //Get the ground rule PK
            int groundRulePK = Convert.ToInt32(hfAddEditGroundRulePK.Value);

            //Make sure that they don't exceed the limit of 6 ground rules
            if (groundRulePK == 0)
            {
                //This is an add, check for other ground rules
                using (PyramidContext context = new PyramidContext())
                {
                    int numOtherGroundRules = context.ProgramActionPlanFCCGroundRule.AsNoTracking().Where(r => r.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK).Count();

                    if (numOtherGroundRules >= 6)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Cannot add another ground rule.  You are already at the maximum of six ground rules for this action plan!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the txtGroundRuleNumber field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The txtGroundRuleNumber BootstrapTextBox</param>
        /// <param name="e">The validation event arguments</param>
        protected void txtGroundRuleNumber_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the ground rule number
            int groundRuleNumber;

            if (int.TryParse(txtGroundRuleNumber.Text, out groundRuleNumber) == false)
            {
                e.IsValid = false;
                e.ErrorText = "Rule Number must be a valid whole number!";
            }
        }

        #endregion

        #region Program Staff Information

        /// <summary>
        /// This method populates the staff information labels with up-to-date information
        /// </summary>
        /// <param name="currentProgramFK">The program FK to filter the information</param>
        private void BindProgramStaffInformation(int? currentProgramFK)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Make sure the dates and program are valid
            if (actionPlanStartDate > DateTime.MinValue &&
                actionPlanEndDate > DateTime.MinValue &&
                currentProgramFK.HasValue &&
                currentProgramFK.Value > 0)
            {

                //To hold the numbers
                int numActiveEmployees, numHiredEmployees, numTerminatedEmployees;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the counts
                    //Active employees
                    numActiveEmployees = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentProgramFK.Value &&
                                                                pe.HireDate <= actionPlanEndDate &&
                                                                (pe.TermDate.HasValue == false || pe.TermDate.Value >= actionPlanStartDate))
                                                    .Count();

                    //Hired in the timeframe
                    numHiredEmployees = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentProgramFK.Value &&
                                                                pe.HireDate >= actionPlanStartDate &&
                                                                pe.HireDate <= actionPlanEndDate)
                                                    .Count();

                    //Terminated in the timeframe
                    numTerminatedEmployees = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentProgramFK.Value &&
                                                                pe.TermDate.HasValue &&
                                                                pe.TermDate.Value >= actionPlanStartDate &&
                                                                pe.TermDate.Value <= actionPlanEndDate)
                                                    .Count();
                }

                //Set the labels
                lblTotalEmployees.Text = numActiveEmployees.ToString();
                lblTotalEmployeesHired.Text = numHiredEmployees.ToString();
                lblTotalEmployeesTerminated.Text = numTerminatedEmployees.ToString();
            }
            else
            {
                //Clear the labels
                lblTotalEmployees.Text = "";
                lblTotalEmployeesHired.Text = "";
                lblTotalEmployeesTerminated.Text = "";
            }
        }

        #endregion

        #region BOQ Information

        /// <summary>
        /// This method populates the most recent BOQ information fields with up-to-date information
        /// </summary>
        /// <param name="currentProgramFK">The program FK to filter the BOQ information</param>
        private void BindMostRecentBOQInformation(int? currentProgramFK)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Make sure the dates and program are valid
            if (actionPlanStartDate > DateTime.MinValue &&
                actionPlanEndDate > DateTime.MinValue &&
                currentProgramFK.HasValue &&
                currentProgramFK.Value > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action plan start date minus 6 months for the calculation below
                    DateTime startOfTimeframe = actionPlanStartDate.AddMonths(-6);

                    //Get the most recent BOQ FCC V2
                    BenchmarkOfQualityFCC mostRecentBOQ = context.BenchmarkOfQualityFCC.AsNoTracking()
                                                    .Where(boq => boq.ProgramFK == currentProgramFK.Value &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= actionPlanEndDate &&
                                                                  boq.VersionNumber == 2 &&
                                                                  boq.IsComplete == true)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();



                    //Get all the critical elements
                    List<CodeBOQCriticalElement> allCriticalElements = context.CodeBOQCriticalElement.AsNoTracking()
                                                            .Where(ce => ce.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQFCCV2)
                                                            .OrderBy(ce => ce.OrderBy)
                                                            .ToList();

                    //Make sure there is a most recent BOQ FCC
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityFCCPK > 0)
                    {
                        //Get the BOQ indicator information
                        List<spGetBOQFCCIndicatorValues_Result> mostRecentBOQIndicatorValues = context.spGetBOQFCCIndicatorValues(mostRecentBOQ.BenchmarkOfQualityFCCPK).ToList();

                        //Get the indicators that need improvement
                        List<spGetBOQFCCIndicatorValues_Result> indicatorsThatNeedImprovement = mostRecentBOQIndicatorValues.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQFCCV2IndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();

                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = indicatorsThatNeedImprovement;
                        repeatBOQIndicatorsToBeImproved.DataBind();

                        //Bind the labels
                        lblMostRecentBOQDate.Text = mostRecentBOQ.FormDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQFCCIndicatorValues_Result>();
                        repeatBOQIndicatorsToBeImproved.DataBind();

                        //Bind the labels
                        lblMostRecentBOQDate.Text = "N/A";
                    }

                    //Bind the repeaters
                    repeatBOQCriticalElements.DataSource = allCriticalElements;
                    repeatBOQCriticalElements.DataBind();
                }
            }
            else
            {
                //Bind the repeaters
                repeatBOQCriticalElements.DataSource = new List<CodeBOQCriticalElement>();
                repeatBOQCriticalElements.DataBind();

                repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQFCCIndicatorValues_Result>();
                repeatBOQIndicatorsToBeImproved.DataBind();

                //Bind the labels
                lblMostRecentBOQDate.Text = "N/A";
            }
        }

        #endregion

        #region Action Steps

        /// <summary>
        /// This method populates the action step repeater with up-to-date information
        /// </summary>
        private void BindActionSteps()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<ProgramActionPlanFCCActionStep> actionSteps = context.ProgramActionPlanFCCActionStep.AsNoTracking()
                                            .Where(t => t.ProgramActionPlanFCCFK == currentActionPlan.ProgramActionPlanFCCPK)
                                            .ToList();
                repeatActionSteps.DataSource = actionSteps;
                repeatActionSteps.DataBind();
            }
        }

        /// <summary>
        /// This method populates the action step indicator labels with up-to-date information
        /// </summary>
        private void BindActionStepIndicatorLabels()
        {
            //To hold the necessary values
            int selectedIndicatorFK;

            if (ddActionStepIndicator.Value != null &&
                int.TryParse(ddActionStepIndicator.Value.ToString(), out selectedIndicatorFK))
            {
                CodeBOQIndicator indicatorObject;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the indicator object
                    indicatorObject = context.CodeBOQIndicator.AsNoTracking()
                                                    .Include(i => i.CodeBOQCriticalElement)
                                                    .Where(i => i.CodeBOQIndicatorPK == selectedIndicatorFK)
                                                    .FirstOrDefault();
                }

                //Set the labels
                if (indicatorObject != null)
                {
                    lblActionStepCriticalElement.Text = indicatorObject.CodeBOQCriticalElement.Description;
                    lblActionStepIndicatorDescription.Text = indicatorObject.Description;
                }
                else
                {
                    lblActionStepCriticalElement.Text = "N/A";
                    lblActionStepIndicatorDescription.Text = "N/A";
                }
            }
            else
            {
                //Set the labels
                lblActionStepCriticalElement.Text = "N/A";
                lblActionStepIndicatorDescription.Text = "N/A";
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetActionStepControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddActionStepIndicator.ClientEnabled = enabled;
            txtActionStepProblemIssueTask.ClientEnabled = enabled;
            txtActionStepActivity.ClientEnabled = enabled;
            txtActionStepPersonsResponsible.ClientEnabled = enabled;
            deActionStepTargetDate.ClientEnabled = enabled;

            //Controls in the initial status section
            ddActionStepInitialStatus.ClientEnabled = enabled;
            deActionStepInitialStatusDate.ClientEnabled = enabled;

            //Show/hide the submit button
            submitActionStep.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitActionStep.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitActionStep.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the action steps
        /// and it opens a div that allows the user to add a action step
        /// </summary>
        /// <param name="sender">The lbAddActionStep LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddActionStep_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditActionStepPK.Value = "0";
            ddActionStepIndicator.Value = "";
            txtActionStepProblemIssueTask.Text = "";
            txtActionStepActivity.Text = "";
            txtActionStepPersonsResponsible.Text = "";
            deActionStepTargetDate.Value = "";

            //Set the title
            lblAddEditActionStep.Text = "Add Action Step";

            //Show the input div
            divAddEditActionStep.Visible = true;

            //Initial status section
            divActionStepInitialStatus.Visible = true;
            deActionStepInitialStatusDate.Value = "";
            ddActionStepInitialStatus.Value = "";

            //Hide the status history section
            divActionStepStatusHistory.Visible = false;

            //Hide the status input section
            divAddEditActionStepStatus.Visible = false;

            //Set focus to the first field
            ddActionStepIndicator.Focus();

            //Enable the controls
            SetActionStepControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a action step
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewActionStep LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewActionStep_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblActionStepPK = (Label)item.FindControl("lblActionStepPK");

            //Get the PK from the label
            int? actionStepPK = (string.IsNullOrWhiteSpace(lblActionStepPK.Text) ? (int?)null : Convert.ToInt32(lblActionStepPK.Text));

            if (actionStepPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action step to view
                    ProgramActionPlanFCCActionStep currentActionStep = context.ProgramActionPlanFCCActionStep
                                                                                .Include(a => a.CodeBOQIndicator)
                                                                                .AsNoTracking()
                                                                                .Where(a => a.ProgramActionPlanFCCActionStepPK == actionStepPK.Value)
                                                                                .FirstOrDefault();

                    //Fill the inputs
                    lblAddEditActionStep.Text = "View Action Step";
                    ddActionStepIndicator.SelectedItem = ddActionStepIndicator.Items.FindByValue(currentActionStep.BOQIndicatorCodeFK);
                    txtActionStepProblemIssueTask.Text = currentActionStep.ProblemIssueTask;
                    txtActionStepActivity.Text = currentActionStep.ActionStepActivity;
                    txtActionStepPersonsResponsible.Text = currentActionStep.PersonsResponsible;
                    deActionStepTargetDate.Date = currentActionStep.TargetDate;
                    hfAddEditActionStepPK.Value = actionStepPK.Value.ToString();
                }

                //Show the edit div
                divAddEditActionStep.Visible = true;

                //Hide the initial status section and clear the inputs
                divActionStepInitialStatus.Visible = false;
                deActionStepInitialStatusDate.Value = "";
                ddActionStepInitialStatus.Value = "";

                //Fill the indicator labels
                BindActionStepIndicatorLabels();

                //Show the status history section
                divActionStepStatusHistory.Visible = true;

                //Hide the status input section
                divAddEditActionStepStatus.Visible = false;

                //Bind the status history
                BindActionStepStatuses();

                //Set focus to the first field
                ddActionStepIndicator.Focus();

                //Disable the controls since this is a view
                SetActionStepControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected action step!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a action step
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditActionStep LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditActionStep_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblActionStepPK = (Label)item.FindControl("lblActionStepPK");

            //Get the PK from the label
            int? actionStepPK = (string.IsNullOrWhiteSpace(lblActionStepPK.Text) ? (int?)null : Convert.ToInt32(lblActionStepPK.Text));

            if (actionStepPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action step to edit
                    ProgramActionPlanFCCActionStep editActionStep = context.ProgramActionPlanFCCActionStep.AsNoTracking().Where(m => m.ProgramActionPlanFCCActionStepPK == actionStepPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditActionStep.Text = "Edit Action Step";
                    ddActionStepIndicator.SelectedItem = ddActionStepIndicator.Items.FindByValue(editActionStep.BOQIndicatorCodeFK);
                    txtActionStepProblemIssueTask.Text = editActionStep.ProblemIssueTask;
                    txtActionStepActivity.Text = editActionStep.ActionStepActivity;
                    txtActionStepPersonsResponsible.Text = editActionStep.PersonsResponsible;
                    deActionStepTargetDate.Date = editActionStep.TargetDate;
                    hfAddEditActionStepPK.Value = actionStepPK.Value.ToString();
                }

                //Show the edit div
                divAddEditActionStep.Visible = true;

                //Fill the indicator labels
                BindActionStepIndicatorLabels();

                //Show the status history section
                divActionStepStatusHistory.Visible = true;

                //Hide the status input section
                divAddEditActionStepStatus.Visible = false;

                //Hide the initial status section and clear the inputs
                divActionStepInitialStatus.Visible = false;
                deActionStepInitialStatusDate.Value = "";
                ddActionStepInitialStatus.Value = "";

                //Bind the status history
                BindActionStepStatuses();

                //Set focus to the first field
                ddActionStepIndicator.Focus();

                //Enable the controls
                SetActionStepControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected action step!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the action step
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitActionStep submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionStep_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditActionStepPK.Value = "0";
            divAddEditActionStep.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitActionStep_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the action step
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitActionStep submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionStep_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the actionStep PK
                int actionStepPK = Convert.ToInt32(hfAddEditActionStepPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    ProgramActionPlanFCCActionStep currentActionStep;

                    //Check to see if this is an add or an edit
                    if (actionStepPK == 0)
                    {
                        //Add
                        currentActionStep = new ProgramActionPlanFCCActionStep();
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.ProgramActionPlanFCCFK = currentActionPlanPK;
                        currentActionStep.CreateDate = DateTime.Now;
                        currentActionStep.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramActionPlanFCCActionStep.Add(currentActionStep);
                        context.SaveChanges();

                        //Add the initial status record
                        ProgramActionPlanFCCActionStepStatus currentStatusRecord = new ProgramActionPlanFCCActionStepStatus();
                        currentStatusRecord.ProgramActionPlanFCCActionStepFK = currentActionStep.ProgramActionPlanFCCActionStepPK;
                        currentStatusRecord.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepInitialStatus.Value);
                        currentStatusRecord.StatusDate = deActionStepInitialStatusDate.Date;
                        currentStatusRecord.CreateDate = DateTime.Now;
                        currentStatusRecord.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramActionPlanFCCActionStepStatus.Add(currentStatusRecord);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStep = context.ProgramActionPlanFCCActionStep.Find(actionStepPK);
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.ProgramActionPlanFCCFK = currentActionPlanPK;
                        currentActionStep.EditDate = DateTime.Now;
                        currentActionStep.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited action step!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditActionStepPK.Value = "0";
                    divAddEditActionStep.Visible = false;

                    //Rebind the action step table
                    BindActionSteps();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a action step
        /// and it deletes the action step information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteActionStep LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteActionStep_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit actionStep information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteActionStepPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteActionStepPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the action step to remove
                            ProgramActionPlanFCCActionStep actionStepToRemove = context.ProgramActionPlanFCCActionStep.Where(t => t.ProgramActionPlanFCCActionStepPK == rowToRemovePK).FirstOrDefault();

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.ProgramActionPlanFCCActionStepStatus
                                .Where(ass => ass.ProgramActionPlanFCCActionStepFK == actionStepToRemove.ProgramActionPlanFCCActionStepPK).ToList();
                            context.ProgramActionPlanFCCActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Remove the action step
                            context.ProgramActionPlanFCCActionStep.Remove(actionStepToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.ProgramActionPlanFCCActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.ProgramActionPlanFCCActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.ProgramActionPlanFCCActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.ProgramActionPlanFCCActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.ProgramActionPlanFCCActionStepChanged
                                    .OrderByDescending(c => c.ProgramActionPlanFCCActionStepChangedPK)
                                    .Where(c => c.ProgramActionPlanFCCActionStepPK == actionStepToRemove.ProgramActionPlanFCCActionStepPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the actionStep table
                            BindActionSteps();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the action step!", 10000);
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the action step, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the action step!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the action step!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the action step to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the value in the ddActionStepIndicator field
        /// changes
        /// </summary>
        /// <param name="sender">The ddActionStepIndicator BootstrapComboBox</param>
        /// <param name="e">The event arguments</param>
        protected void ddActionStepIndicator_ValueChanged(object sender, EventArgs e)
        {
            //Fill the indicator labels
            BindActionStepIndicatorLabels();

            //Re-focus on the control
            ddActionStepIndicator.Focus();
        }

        /// <summary>
        /// This method fires when the deActionStepTargetDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deActionStepTargetDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deActionStepTargetDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates
            DateTime? targetDate = (deActionStepTargetDate.Value == null ? (DateTime?)null : deActionStepTargetDate.Date);
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);

            //Validate
            if (actionPlanStartDate.HasValue == false || actionPlanEndDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The action plan start date and action plan end date in the basic information section must be entered before the Target Date can be evaluated.";
            }
            else if (targetDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Target Date is required!";
            }
            else if (targetDate.Value < actionPlanStartDate)
            {
                e.IsValid = false;
                e.ErrorText = "Target Date must be on or after the action plan start date in the basic information section!";
            }
            else if (targetDate.Value > actionPlanEndDate)
            {
                e.IsValid = false;
                e.ErrorText = "Target Date must be on or before the action plan end date in the basic information section!";
            }
        }

        /// <summary>
        /// This method fires when the ddActionStepInitialStatus field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The ddActionStepInitialStatus BootstrapComboBox</param>
        /// <param name="e">The validation event arguments</param>
        protected void ddActionStepInitialStatus_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the current action step PK
            int currentActionStepPK;

            //Only validate if this is an add
            if (int.TryParse(hfAddEditActionStepPK.Value, out currentActionStepPK) == false || currentActionStepPK == 0)
            {
                //Validate
                if(ddActionStepInitialStatus.SelectedItem == null)
                {
                    e.IsValid = false;
                    e.ErrorText = "Current Status is required!";
                }                
            }
        }

        /// <summary>
        /// This method fires when the deActionStepInitialStatusDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deActionStepInitialStatusDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deActionStepInitialStatusDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the current action step PK
            int currentActionStepPK;

            //Only validate if this is an add
            if(int.TryParse(hfAddEditActionStepPK.Value, out currentActionStepPK) == false || currentActionStepPK == 0)
            {
                //Get the necessary dates
                DateTime? initialStatusDate = (deActionStepInitialStatusDate.Value == null ? (DateTime?)null : deActionStepInitialStatusDate.Date);

                //Validate
                if (initialStatusDate.HasValue == false)
                {
                    e.IsValid = false;
                    e.ErrorText = "Current Status Date is required!";
                }
                else if (initialStatusDate > DateTime.Now)
                {
                    e.IsValid = false;
                    e.ErrorText = "Current Status Date cannot be in the future!";
                }
            }
        }

        #endregion

        #region Action Step Statuses

        /// <summary>
        /// This method populates the action step status repeater with up-to-date information
        /// </summary>
        private void BindActionStepStatuses()
        {
            //Get the necessary values
            int actionStepFK = Convert.ToInt32(hfAddEditActionStepPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<ProgramActionPlanFCCActionStepStatus> actionStepStatuses = context.ProgramActionPlanFCCActionStepStatus
                                            .Include(s => s.ProgramActionPlanFCCActionStep)
                                            .AsNoTracking()
                                            .Where(s => s.ProgramActionPlanFCCActionStepFK == actionStepFK)
                                            .ToList();
                repeatActionStepStatuses.DataSource = actionStepStatuses;
                repeatActionStepStatuses.DataBind();
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetActionStepStatusControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddActionStepStatus.ClientEnabled = enabled;
            deActionStepStatusDate.ClientEnabled = enabled;

            //Show/hide the submit button
            submitActionStepStatus.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitActionStepStatus.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitActionStepStatus.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the action step statuses
        /// and it opens a div that allows the user to add a action step status
        /// </summary>
        /// <param name="sender">The lbAddActionStepStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddActionStepStatus_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditActionStepStatusPK.Value = "0";
            ddActionStepStatus.Value = "";
            deActionStepStatusDate.Value = "";

            //Set the title
            lblAddEditActionStepStatus.Text = "Add Action Step Status";

            //Show the input div
            divAddEditActionStepStatus.Visible = true;

            //Set focus to the first field
            ddActionStepStatus.Focus();

            //Enable the controls
            SetActionStepStatusControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a action step status
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewActionStepStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewActionStepStatus_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblActionStepStatusPK = (Label)item.FindControl("lblActionStepStatusPK");

            //Get the PK from the label
            int? actionStepStatusPK = (string.IsNullOrWhiteSpace(lblActionStepStatusPK.Text) ? (int?)null : Convert.ToInt32(lblActionStepStatusPK.Text));

            if (actionStepStatusPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action step status to view
                    ProgramActionPlanFCCActionStepStatus currentActionStepStatus = context.ProgramActionPlanFCCActionStepStatus
                                                                                .AsNoTracking()
                                                                                .Where(a => a.ProgramActionPlanFCCActionStepStatusPK == actionStepStatusPK.Value)
                                                                                .FirstOrDefault();

                    //Fill the inputs
                    lblAddEditActionStepStatus.Text = "View Action Step Status";
                    ddActionStepStatus.SelectedItem = ddActionStepStatus.Items.FindByValue(currentActionStepStatus.ActionPlanActionStepStatusCodeFK);
                    deActionStepStatusDate.Date = currentActionStepStatus.StatusDate;
                    hfAddEditActionStepStatusPK.Value = actionStepStatusPK.Value.ToString();
                }

                //Show the edit div
                divAddEditActionStepStatus.Visible = true;

                //Set focus to the first field
                ddActionStepStatus.Focus();

                //Disable the controls since this is a view
                SetActionStepStatusControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected action step status!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a action step status
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditActionStepStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditActionStepStatus_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblActionStepStatusPK = (Label)item.FindControl("lblActionStepStatusPK");

            //Get the PK from the label
            int? actionStepStatusPK = (string.IsNullOrWhiteSpace(lblActionStepStatusPK.Text) ? (int?)null : Convert.ToInt32(lblActionStepStatusPK.Text));

            if (actionStepStatusPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action step status to edit
                    ProgramActionPlanFCCActionStepStatus editActionStepStatus = context.ProgramActionPlanFCCActionStepStatus.AsNoTracking().Where(m => m.ProgramActionPlanFCCActionStepStatusPK == actionStepStatusPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditActionStepStatus.Text = "Edit Action Step Status";
                    ddActionStepStatus.SelectedItem = ddActionStepStatus.Items.FindByValue(editActionStepStatus.ActionPlanActionStepStatusCodeFK);
                    deActionStepStatusDate.Date = editActionStepStatus.StatusDate;
                    hfAddEditActionStepStatusPK.Value = actionStepStatusPK.Value.ToString();
                }

                //Show the edit div
                divAddEditActionStepStatus.Visible = true;

                //Set focus to the first field
                ddActionStepStatus.Focus();

                //Enable the controls
                SetActionStepStatusControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected action step status!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the action step status
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitActionStepStatus submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionStepStatus_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditActionStepStatusPK.Value = "0";
            divAddEditActionStepStatus.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitActionStepStatus_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the action step status
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitActionStepStatus submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitActionStepStatus_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the necessary values
                int actionStepStatusPK = Convert.ToInt32(hfAddEditActionStepStatusPK.Value);
                int actionStepFK = Convert.ToInt32(hfAddEditActionStepPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    ProgramActionPlanFCCActionStepStatus currentActionStepStatus;

                    //Check to see if this is an add or an edit
                    if (actionStepStatusPK == 0)
                    {
                        //Add
                        currentActionStepStatus = new ProgramActionPlanFCCActionStepStatus();
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.ProgramActionPlanFCCActionStepFK = actionStepFK;
                        currentActionStepStatus.CreateDate = DateTime.Now;
                        currentActionStepStatus.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramActionPlanFCCActionStepStatus.Add(currentActionStepStatus);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step status!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStepStatus = context.ProgramActionPlanFCCActionStepStatus.Find(actionStepStatusPK);
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.ProgramActionPlanFCCActionStepFK = actionStepFK;
                        currentActionStepStatus.EditDate = DateTime.Now;
                        currentActionStepStatus.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited action step status!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditActionStepStatusPK.Value = "0";
                    divAddEditActionStepStatus.Visible = false;

                    //Rebind the action step status table
                    BindActionStepStatuses();

                    //Rebind the action step table
                    BindActionSteps();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a action step status
        /// and it deletes the action step status information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteActionStepStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteActionStepStatus_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit action step status information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteActionStepStatusPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteActionStepStatusPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the action step status to remove
                            ProgramActionPlanFCCActionStepStatus actionStepStatusToRemove = context.ProgramActionPlanFCCActionStepStatus.Where(t => t.ProgramActionPlanFCCActionStepStatusPK == rowToRemovePK).FirstOrDefault();

                            //Don't allow deletion if there are no other status records for this action step
                            if (context.ProgramActionPlanFCCActionStepStatus.Where(s => s.ProgramActionPlanFCCActionStepStatusPK != actionStepStatusToRemove.ProgramActionPlanFCCActionStepStatusPK &&
                                                                                        s.ProgramActionPlanFCCActionStepFK == actionStepStatusToRemove.ProgramActionPlanFCCActionStepFK)
                                .Count() > 0)
                            {
                                //Remove the action step status
                                context.ProgramActionPlanFCCActionStepStatus.Remove(actionStepStatusToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.ProgramActionPlanFCCActionStepStatusChanged
                                        .OrderByDescending(c => c.ProgramActionPlanFCCActionStepStatusChangedPK)
                                        .Where(c => c.ProgramActionPlanFCCActionStepStatusPK == actionStepStatusToRemove.ProgramActionPlanFCCActionStepStatusPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Rebind the action step status table
                                BindActionStepStatuses();

                                //Rebind the action step table
                                BindActionSteps();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the action step status!", 10000);
                            }
                            else
                            {
                                //Show an error message
                                msgSys.ShowMessageToUser("danger", "Deletion Failed", "Cannot delete the only action step status record!  There must be at least one status record for each action step.", 10000);
                            }
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the action step status, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the action step status!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the action step status!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the action step status to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the deActionStepStatusDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deActionStepStatusDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deActionStepStatusDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates
            DateTime? statusDate = (deActionStepStatusDate.Value == null ? (DateTime?)null : deActionStepStatusDate.Date);

            //Validate
            if (statusDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date is required!";
            }
            else if (statusDate > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date cannot be in the future!";
            }
        }

        #endregion
    }
}