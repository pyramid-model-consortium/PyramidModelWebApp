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
    public partial class CWLTActionPlan : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "CWLTAP";
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
        private Models.CWLTActionPlan currentActionPlan;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CWLTActionPlanPK"]))
            {
                //Parse the pk
                int.TryParse(Request.QueryString["CWLTActionPlanPK"], out currentActionPlanPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentActionPlanPK == 0 && !string.IsNullOrWhiteSpace(hfCWLTActionPlanPK.Value))
            {
                int.TryParse(hfCWLTActionPlanPK.Value, out currentActionPlanPK);
            }

            //Check to see if this is an edit
            isEdit = currentActionPlanPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect back to the dashboard
                Response.Redirect("/Pages/CWLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.CWLTActionPlan.AsNoTracking()
                                        .Include(d => d.Hub)
                                        .Where(d => d.CWLTActionPlanPK == currentActionPlanPK).FirstOrDefault();

                //Check to see if the debrief exists
                if (currentActionPlan == null)
                {
                    //The debrief doesn't exist, set the debrief to a new debrief object
                    currentActionPlan = new Models.CWLTActionPlan();
                }
                else
                {
                    //Show a message to the user if this is not the most recent action plan for the hub
                    int numMoreRecentPlans = context.CWLTActionPlan.AsNoTracking()
                                                    .Where(ap => ap.CWLTActionPlanPK != currentActionPlan.CWLTActionPlanPK &&
                                                                 ap.HubFK == currentActionPlan.HubFK &&
                                                                 ap.ActionPlanStartDate > currentActionPlan.ActionPlanStartDate)
                                                    .Count();

                    if (numMoreRecentPlans > 0)
                    {
                        divActionPlanAlert.Visible = true;
                        lblActionPlanAlert.Text = "This is not the most recent action plan for this hub.  Any new or changed information should be added to the most recent action plan.";
                    }
                }
            }

            //Prevent users from viewing action plans from other hubs
            if (isEdit && !currentProgramRole.HubFKs.Contains(currentActionPlan.HubFK))
            {
                //Add a message that will show after redirect
                msgSys.AddMessageToQueue("danger", "Not Found", "The action plan you are attempting to access does not exist.", 10000);

                //Redirect the user back to the dashboard
                Response.Redirect("/Pages/CWLTDashboard.aspx");
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
                BindLeadershipCoachList(currentActionPlan.HubFK);

                //Bind the drop-downs
                BindDropDowns();
                BindLeadershipCoachDropDown(currentActionPlan.HubFK, currentActionPlan.LeadershipCoachUsername);

                //Fill the form with data
                FillFormWithDataFromObject();

                if (isEdit)
                {
                    //Set the leadership coach labels
                    SetLeadershipCoachLabels();

                    //Bind the tables
                    BindMeetings();
                    BindLeadershipCoachSchedule(currentActionPlan.HubFK);
                    BindGroundRules();

                    //Bind the hub info
                    BindHubInformation(currentActionPlan.HubFK);

                    //Bind the BOQ information
                    BindMostRecentBOQInformation(currentActionPlan.HubFK);

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
                    lblPageTitle.Text = "Add New Action Plan";
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
                    lblPageTitle.Text = "Edit Action Plan";
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
                    lblPageTitle.Text = "View Action Plan";
                }

                //Set focus to the first field
                ddHub.Focus();

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
                ddHub.SelectedItem = ddHub.Items.FindByValue(currentActionPlan.HubFK);
                ddHubCoordinator.SelectedItem = ddHubCoordinator.Items.FindByValue(currentActionPlan.HubCoordinatorFK);
                deActionPlanStartDate.Value = currentActionPlan.ActionPlanStartDate;
                deActionPlanEndDate.Value = currentActionPlan.ActionPlanEndDate;
                ddIsLeadershipCoachInvolved.Value = currentActionPlan.IsLeadershipCoachInvolved;
                ddLeadershipCoach.Value = currentActionPlan.LeadershipCoachUsername;
                txtMissionStatement.Text = currentActionPlan.MissionStatement;
                txtAdditionalNotes.Text = currentActionPlan.AdditionalNotes;

                //Bind the coordinator email label
                BindHubCoordinatorEmailLabel();
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
            ddHub.ClientEnabled = enabled;
            deActionPlanStartDate.ClientEnabled = enabled;
            deActionPlanEndDate.ClientEnabled = enabled;
            ddHubCoordinator.ClientEnabled = enabled;
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
            List<Hub> allHubs = new List<Hub>();
            List<CodeBOQIndicator> allIndicators = new List<CodeBOQIndicator>();
            List<CodeActionPlanActionStepStatus> allActionStepStatuses = new List<CodeActionPlanActionStepStatus>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allHubs = context.Hub.AsNoTracking()
                                             .Where(p => currentProgramRole.HubFKs.Contains(p.HubPK))
                                             .OrderBy(p => p.Name)
                                             .ToList();
                allIndicators = context.CodeBOQIndicator.Include(i => i.CodeBOQCriticalElement).AsNoTracking().Where(i => i.CodeBOQCriticalElement.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQCWLT).OrderBy(i => i.OrderBy).ToList();
                allActionStepStatuses = context.CodeActionPlanActionStepStatus.AsNoTracking().OrderBy(s => s.OrderBy).ToList();
            }

            //Bind the hub dropdown
            ddHub.DataSource = allHubs;
            ddHub.DataBind();

            //Bind the CWLT member drop-downs
            if (isEdit)
            {
                BindCWLTMemberControls(currentActionPlan.HubFK, currentActionPlan.ActionPlanStartDate, currentActionPlan.ActionPlanEndDate, currentActionPlan.HubCoordinatorFK);
            }
            else
            {
                BindCWLTMemberControls(null, null, null, null);
            }

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
        /// This method binds the list of leadership coaches
        /// </summary>
        /// <param name="currentHubFK">The hub FK used to filter the dropdown</param>
        private void BindLeadershipCoachList(int? currentHubFK)
        {
            //Make sure the program is valid
            if (currentHubFK.HasValue && currentHubFK.Value > 0)
            {
                if (currentHubFK.Value == currentActionPlan.HubFK)
                {
                    //Get the leadership coaches for the hub and
                    //always include the coach saved to the form in case
                    //the coach's role was removed.
                    allLeadershipCoaches = PyramidUser.GetHubLeadershipCoachUserRecords(new List<int> { currentHubFK.Value }, currentActionPlan.LeadershipCoachUsername);
                }
                else
                {
                    //Get the leadership coaches for the hub
                    allLeadershipCoaches = PyramidUser.GetHubLeadershipCoachUserRecords(new List<int> { currentHubFK.Value });
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
        /// <param name="currentHubFK">The hub FK used to filter the dropdown</param>
        /// <param name="currentLCUsername">The current selected leadership coach username (if any)</param>
        private void BindLeadershipCoachDropDown(int? currentHubFK, string currentLCUsername)
        {
            //Make sure the program is valid
            if (currentHubFK.HasValue && currentHubFK.Value > 0)
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

            //Get the leadership coach username
            leadershipCoachUsername = (ddLeadershipCoach.Value == null ? null : Convert.ToString(ddLeadershipCoach.Value));

            //Make sure the selected leadership coach username is valid before continuing
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
                    Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx"));
                }
                else
                {
                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Action Plan was successfully added!<br/><br/>More detailed information can now be added to the Action Plan.", 10000);

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/CWLTActionPlan.aspx?CWLTActionPlanPK={0}&Action=Edit",
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

            //Redirect the user to the CWLT dashboard
            Response.Redirect("/Pages/CWLTDashboard.aspx");
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
                    Reports.PreBuiltReports.FormReports.RptCWLTActionPlan report = new Reports.PreBuiltReports.FormReports.RptCWLTActionPlan();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Community-Wide Leadership Team Action Plan", currentActionPlanPK);
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
                    Response.Redirect(string.Format("/Pages/CWLTActionPlan.aspx?CWLTActionPlanPK={0}&Action={1}&Print=True",
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
                currentActionPlan.HubFK = Convert.ToInt32(ddHub.Value);
                currentActionPlan.HubCoordinatorFK = Convert.ToInt32(ddHubCoordinator.Value);

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit-only fields
                        currentActionPlan.EditDate = DateTime.Now;
                        currentActionPlan.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.CWLTActionPlan existingActionPlan = context.CWLTActionPlan.Find(currentActionPlan.CWLTActionPlanPK);

                        //Set the object to the new values
                        context.Entry(existingActionPlan).CurrentValues.SetValues(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTActionPlanPK.Value = currentActionPlan.CWLTActionPlanPK.ToString();
                        currentActionPlanPK = currentActionPlan.CWLTActionPlanPK;

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
                        context.CWLTActionPlan.Add(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCWLTActionPlanPK.Value = currentActionPlan.CWLTActionPlanPK.ToString();
                        currentActionPlanPK = currentActionPlan.CWLTActionPlanPK;

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
        /// This method fires when the validation for the ddHub DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddHub BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddHub_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedHubFK;

            //Get the hub FK
            selectedHubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

            //Validate
            if (selectedHubFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Hub is required!";
            }
            else if (currentActionPlan.IsPrefilled == false || chkReviewedBasicInfo.Checked)
            {
                //Check for duplication
                if (deActionPlanStartDate.Date != DateTime.MinValue && deActionPlanEndDate.Date != DateTime.MinValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check to see if there are any schedules
                        List<Models.CWLTActionPlan> duplicateActionPlans = context.CWLTActionPlan.AsNoTracking()
                                        .Where(d => d.HubFK == selectedHubFK.Value &&
                                                d.CWLTActionPlanPK != currentActionPlanPK &&
                                                ((d.ActionPlanStartDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanStartDate >= deActionPlanStartDate.Date) ||
                                                (d.ActionPlanEndDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanEndDate >= deActionPlanStartDate.Date))).ToList();

                        //Check the count of duplicate schedules
                        if (duplicateActionPlans.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "There is already an action plan for the selected hub within the timeframe entered here!";
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
            if(ddIsLeadershipCoachInvolved.Value != null && 
                bool.TryParse(ddIsLeadershipCoachInvolved.Value.ToString(), out bool isLeadershipCoachInvolved) && 
                isLeadershipCoachInvolved == true)
            {
                //Get the selected leadership coach's username
                string leadershipCoachUsername = (ddLeadershipCoach.Value == null ? null : Convert.ToString(ddLeadershipCoach.Value));

                //Get the selected hub FK
                int? selectedHubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

                if (string.IsNullOrWhiteSpace(leadershipCoachUsername))
                {
                    e.IsValid = false;
                    e.ErrorText = "Primary Leadership Coach is required!";
                }
                else if (selectedHubFK.HasValue)
                {
                    //Both the coach and hub are selected, make sure the coach is allowed for that hub
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the matching leadership coach roles for the selected leadership coach and the selected hub
                        List<UserProgramRole> matchingLeadershipCoachRoles = context.UserProgramRole.AsNoTracking()
                                .Include(upr => upr.Program)
                                .Where(upr => upr.Username == leadershipCoachUsername &&
                                                upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH &&
                                                upr.Program.HubFK == selectedHubFK.Value)
                                .ToList();

                        //Validate
                        if (currentActionPlan.HubFK == selectedHubFK &&
                                currentActionPlan.LeadershipCoachUsername == leadershipCoachUsername)
                        {
                            //Allow saving the action plan if the hub FK and leadership coach have not changed since the leadership coach
                            //may lose the role in their account, but we don't have a date for that.
                            e.IsValid = true;
                        }
                        else if (matchingLeadershipCoachRoles == null || matchingLeadershipCoachRoles.Count <= 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "That combination of Hub and Primary Leadership Coach is not valid!";
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
            //Get the selected hub FK
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);

            //Check to see if the start date was entered
            if (actionPlanStartDate.HasValue == false)
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
                        List<Models.CWLTActionPlanMeeting> invalidMeetingDates = context.CWLTActionPlanMeeting
                                                    .AsNoTracking()
                                                    .Where(m => m.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK &&
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
                            List<Models.CWLTActionPlanActionStep> invalidActionSteps = context.CWLTActionPlanActionStep
                                                        .Include(s => s.CodeBOQIndicator)
                                                        .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                        .AsNoTracking()
                                                        .Where(s => s.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK &&
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

            //Get the necessary information
            int? selectedHubFK = (ddHub.SelectedItem == null ? (int?)null : Convert.ToInt32(ddHub.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);
            int? selectedHubCoordinatorFK = (ddHubCoordinator.SelectedItem == null ? (int?)null : Convert.ToInt32(ddHubCoordinator.Value));

            //Bind the CWLT member drop-downs
            BindCWLTMemberControls(selectedHubFK, actionPlanStartDate, actionPlanEndDate, selectedHubCoordinatorFK);

            //Bind the leadership coach list
            BindLeadershipCoachList(selectedHubFK);

            //Bind the leadership coach schedules
            BindLeadershipCoachSchedule(selectedHubFK);

            //Bind the hub information
            BindHubInformation(selectedHubFK);

            //Bind the BOQ information
            BindMostRecentBOQInformation(selectedHubFK);

            //Re-focus on the control
            currentDateEdit.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a hub from the ddHub control.
        /// </summary>
        /// <param name="sender">The ddHub BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddHub_ValueChanged(object sender, EventArgs e)
        {
            //Get the necessary information
            int? selectedHubFK = (ddHub.SelectedItem == null ? (int?)null : Convert.ToInt32(ddHub.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);
            int? selectedHubCoordinatorFK = (ddHubCoordinator.SelectedItem == null ? (int?)null : Convert.ToInt32(ddHubCoordinator.Value));
            string selectedLCUsername = (ddLeadershipCoach.Value != null ? ddLeadershipCoach.Value.ToString() : null);

            //Bind the CWLT member drop-downs
            BindCWLTMemberControls(selectedHubFK, actionPlanStartDate, actionPlanEndDate, selectedHubCoordinatorFK);

            //Bind the leadership coach list
            BindLeadershipCoachList(selectedHubFK);

            //Bind the leadership coach dropdown
            BindLeadershipCoachDropDown(selectedHubFK, selectedLCUsername);

            //Bind the leadership coach schedules
            BindLeadershipCoachSchedule(selectedHubFK);

            //Bind the hub information
            BindHubInformation(selectedHubFK);

            //Bind the BOQ information
            BindMostRecentBOQInformation(selectedHubFK);

            //Re-focus on the control
            ddHub.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a hub coordinator from the ddHubCoordinator control.
        /// </summary>
        /// <param name="sender">The ddHubCoordinator BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddHubCoordinator_ValueChanged(object sender, EventArgs e)
        {
            //Bind the coordinator email label
            BindHubCoordinatorEmailLabel();

            //Re-focus on the control
            ddHubCoordinator.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a leadership coach from the ddLeadershipCoach control.
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

        /// <summary>
        /// This method binds the hub coordinator email label based on the selected hub coordinator
        /// </summary>
        private void BindHubCoordinatorEmailLabel()
        {
            //Try to get the email
            if(ddHubCoordinator.SelectedItem != null && int.TryParse(ddHubCoordinator.Value.ToString(), out int hubCoordinatorPK))
            {
                //To hold the selected Hub Coordinator object
                Models.CWLTMember selectedCoordinator;

                //To hold the selected Hub
                using(PyramidContext context = new PyramidContext())
                {
                    selectedCoordinator = context.CWLTMember.AsNoTracking().Where(cm => cm.CWLTMemberPK == hubCoordinatorPK).FirstOrDefault();
                }

                //Set the label
                if(selectedCoordinator != null)
                {
                    lblHubCoordinatorEmail.Text = selectedCoordinator.EmailAddress;
                }
                else
                {
                    lblHubCoordinatorEmail.Text = "";
                }
            }
            else
            {
                //Clear the label
                lblHubCoordinatorEmail.Text = "";
            }
        }

        /// <summary>
        /// This method binds the controls that are filled with active CWLT members
        /// </summary>
        private void BindCWLTMemberControls(int? currentHubFK, DateTime? actionPlanStartDate, DateTime? actionPlanEndDate, int? currentHubCoordinatorFK)
        {
            if (actionPlanStartDate.HasValue &&
                actionPlanEndDate.HasValue &&
                currentHubFK.HasValue)
            {
                //To hold the active CWLT members
                List<Models.CWLTMember> activeCWLTMembers;

                //Get the active CWLT members
                using(PyramidContext context = new PyramidContext())
                {
                    activeCWLTMembers = context.CWLTMember.AsNoTracking()
                                                .Include(cm => cm.Hub)
                                                .Where(cm => cm.HubFK == currentHubFK.Value && 
                                                       cm.StartDate <= actionPlanEndDate && 
                                                       (cm.LeaveDate.HasValue == false || 
                                                            cm.LeaveDate >= actionPlanStartDate))
                                                .ToList();
                }

                //----Hub Coordinator drop-down-----

                //Bind the hub coordinator drop-down
                ddHubCoordinator.DataSource = activeCWLTMembers.Select(cm => new { 
                    cm.CWLTMemberPK,
                    IDNumberAndName = string.Format("({0}) {1} {2}", cm.IDNumber, cm.FirstName, cm.LastName)
                }).ToList();
                ddHubCoordinator.DataBind();

                //Try to re-select the coordinator
                if (currentHubCoordinatorFK.HasValue)
                {
                    ddHubCoordinator.SelectedItem = ddHubCoordinator.Items.FindByValue(currentHubCoordinatorFK.Value);
                }

                //If there is no selected coordinator, clear the email label
                if(ddHubCoordinator.SelectedItem == null)
                {
                    //Clear the email label
                    lblHubCoordinatorEmail.Text = "";
                }

                //----End Hub Coordinator drop-down-----

                //----Leadership Team member repeater----

                repeatLeadershipTeamMembers.DataSource = activeCWLTMembers;
                repeatLeadershipTeamMembers.DataBind();

                //----End Leadership Team member repeater----
            }
            else
            {
                //Clear the drop-downs
                ddHubCoordinator.Value = "";

                //Clear the email label
                lblHubCoordinatorEmail.Text = "";

                //Bind the repeater to an empty list
                repeatLeadershipTeamMembers.DataSource = new List<Models.CWLTMember>();
                repeatLeadershipTeamMembers.DataBind();
            }
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
                List<CWLTActionPlanMeeting> Meetings = context.CWLTActionPlanMeeting.AsNoTracking()
                                            .Where(t => t.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK)
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
                    CWLTActionPlanMeeting currentMeeting = context.CWLTActionPlanMeeting.AsNoTracking().Where(m => m.CWLTActionPlanMeetingPK == meetingPK.Value).FirstOrDefault();

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
                    CWLTActionPlanMeeting editMeeting = context.CWLTActionPlanMeeting.AsNoTracking().Where(m => m.CWLTActionPlanMeetingPK == meetingPK.Value).FirstOrDefault();

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
                    CWLTActionPlanMeeting currentMeeting;

                    //Check to see if this is an add or an edit
                    if (meetingPK == 0)
                    {
                        //Add
                        currentMeeting = new CWLTActionPlanMeeting();
                        currentMeeting.MeetingDate = deMeetingDate.Date;
                        currentMeeting.LeadershipCoachAttendance = Convert.ToBoolean(ddMeetingLeadershipCoachAttendance.Value);
                        currentMeeting.MeetingNotes = (string.IsNullOrWhiteSpace(txtMeetingNotes.Text) ? null : txtMeetingNotes.Text);
                        currentMeeting.CWLTActionPlanFK = currentActionPlanPK;
                        currentMeeting.CreateDate = DateTime.Now;
                        currentMeeting.Creator = User.Identity.Name;

                        //Save to the database
                        context.CWLTActionPlanMeeting.Add(currentMeeting);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added meeting!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentMeeting = context.CWLTActionPlanMeeting.Find(meetingPK);
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
                            CWLTActionPlanMeeting meetingToRemove = context.CWLTActionPlanMeeting.Where(t => t.CWLTActionPlanMeetingPK == rowToRemovePK).FirstOrDefault();

                            //Remove the meeting
                            context.CWLTActionPlanMeeting.Remove(meetingToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CWLTActionPlanMeetingChanged
                                    .OrderByDescending(c => c.CWLTActionPlanMeetingChangedPK)
                                    .Where(c => c.CWLTActionPlanMeetingPK == meetingToRemove.CWLTActionPlanMeetingPK)
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

        #region Leadership Coach Schedule

        /// <summary>
        /// This method populates the leadership coach schedule repeater with up-to-date information
        /// </summary>
        /// <param name="currentHubFK">The hub FK</param>
        private void BindLeadershipCoachSchedule(int? currentHubFK)
        {
            //Make sure the timeframe and hub are valid
            if (deActionPlanStartDate.Date > DateTime.MinValue &&
                deActionPlanEndDate.Date > DateTime.MinValue &&
                currentHubFK.HasValue &&
                currentHubFK.Value > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedules for the hub and timeframe combination
                    List<HubLCMeetingSchedule> hubLCSchedules = context.HubLCMeetingSchedule.AsNoTracking()
                                                .Include(p => p.Hub)
                                                .Where(p => p.HubFK == currentHubFK.Value &&
                                                            (p.MeetingYear == deActionPlanStartDate.Date.Year ||
                                                                p.MeetingYear == deActionPlanEndDate.Date.Year))
                                                .ToList();

                    //Set the full leadership coach name
                    foreach (HubLCMeetingSchedule schedule in hubLCSchedules)
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
                            //This is necessary now that the leadership coach roles are hub-specific and can be removed
                            leadershipCoachUserRecord = PyramidUser.GetUserRecordByUsername(schedule.LeadershipCoachUsername);

                            //Set the label text (include the username for searching
                            schedule.LeadershipCoachUsername = string.Format("{0} {1} ({2})", leadershipCoachUserRecord.FirstName, leadershipCoachUserRecord.LastName, leadershipCoachUserRecord.UserName);
                        }
                    }

                    //Bind the repeater
                    repeatLeadershipCoachSchedule.DataSource = hubLCSchedules;
                    repeatLeadershipCoachSchedule.DataBind();
                }
            }
            else
            {
                //Clear the repeater
                repeatLeadershipCoachSchedule.DataSource = new List<HubLCMeetingSchedule>();
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
                List<CWLTActionPlanGroundRule> GroundRules = context.CWLTActionPlanGroundRule.AsNoTracking()
                                            .Where(t => t.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK)
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
                    CWLTActionPlanGroundRule currentGroundRule = context.CWLTActionPlanGroundRule.AsNoTracking().Where(m => m.CWLTActionPlanGroundRulePK == groundRulePK.Value).FirstOrDefault();

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
                    CWLTActionPlanGroundRule editGroundRule = context.CWLTActionPlanGroundRule.AsNoTracking().Where(m => m.CWLTActionPlanGroundRulePK == groundRulePK.Value).FirstOrDefault();

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
                    CWLTActionPlanGroundRule currentGroundRule;

                    //Check to see if this is an add or an edit
                    if (groundRulePK == 0)
                    {
                        //Add
                        currentGroundRule = new CWLTActionPlanGroundRule();
                        currentGroundRule.GroundRuleDescription = txtGroundRuleDescription.Text;
                        currentGroundRule.GroundRuleNumber = Convert.ToInt32(txtGroundRuleNumber.Text);
                        currentGroundRule.CWLTActionPlanFK = currentActionPlanPK;
                        currentGroundRule.CreateDate = DateTime.Now;
                        currentGroundRule.Creator = User.Identity.Name;

                        //Save to the database
                        context.CWLTActionPlanGroundRule.Add(currentGroundRule);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added ground rule!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentGroundRule = context.CWLTActionPlanGroundRule.Find(groundRulePK);
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
                            CWLTActionPlanGroundRule assignmentToRemove = context.CWLTActionPlanGroundRule.Where(t => t.CWLTActionPlanGroundRulePK == rowToRemovePK).FirstOrDefault();

                            //Remove the ground rule
                            context.CWLTActionPlanGroundRule.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CWLTActionPlanGroundRuleChanged
                                    .OrderByDescending(c => c.CWLTActionPlanGroundRuleChangedPK)
                                    .Where(c => c.CWLTActionPlanGroundRulePK == assignmentToRemove.CWLTActionPlanGroundRulePK)
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
                    int numOtherGroundRules = context.CWLTActionPlanGroundRule.AsNoTracking().Where(r => r.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK).Count();

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

        #region Hub Information

        /// <summary>
        /// This method populates the staff information labels with up-to-date information
        /// </summary>
        /// <param name="currentHubFK">The hub FK</param>
        private void BindHubInformation(int? currentHubFK)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Make sure the timeframe and hub are valid
            if (actionPlanStartDate > DateTime.MinValue &&
                actionPlanEndDate > DateTime.MinValue &&
                currentHubFK.HasValue &&
                currentHubFK.Value > 0)
            {
                //To hold the numbers
                int numActivePrograms;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the counts
                    //Active programs
                    numActivePrograms = context.Program.AsNoTracking()
                                                    .Where(p => p.HubFK == currentHubFK.Value &&
                                                                p.ProgramStartDate <= actionPlanEndDate &&
                                                                (p.ProgramEndDate.HasValue == false || p.ProgramEndDate.Value >= actionPlanStartDate))
                                                    .Count();
                }

                //Set the labels
                lblTotalActivePrograms.Text = numActivePrograms.ToString();
            }
            else
            {
                //Clear the labels
                lblTotalActivePrograms.Text = "";
            }
        }

        #endregion

        #region BOQ Information

        /// <summary>
        /// This method populates the most recent BOQ information fields with up-to-date information
        /// </summary>
        /// <param name="currentHubFK">The hub FK</param>
        private void BindMostRecentBOQInformation(int? currentHubFK)
        {
            //To hold the start and end date
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Make sure the timeframe and hub are valid
            if (actionPlanStartDate > DateTime.MinValue &&
                actionPlanEndDate > DateTime.MinValue &&
                currentHubFK.HasValue &&
                currentHubFK.Value > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action plan start date minus 6 months for the calculation below
                    DateTime startOfTimeframe = actionPlanStartDate.AddMonths(-6);

                    //Get the most recent BOQ
                    BenchmarkOfQualityCWLT mostRecentBOQ = context.BenchmarkOfQualityCWLT.AsNoTracking()
                                                    .Where(boq => boq.HubFK == currentHubFK.Value &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= actionPlanEndDate)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();

                    //Get all the critical elements
                    List<CodeBOQCriticalElement> allCriticalElements = context.CodeBOQCriticalElement.AsNoTracking()
                                                        .Where(ce => ce.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQCWLT)
                                                        .OrderBy(ce => ce.OrderBy)
                                                        .ToList();

                    //Make sure there is a most recent BOQ
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualityCWLTPK > 0)
                    {
                        //Get the BOQ indicator information
                        List<spGetBOQCWLTIndicatorValues_Result> mostRecentBOQIndicatorValues = context.spGetBOQCWLTIndicatorValues(mostRecentBOQ.BenchmarkOfQualityCWLTPK).ToList();

                        //Get the indicators that need improvement
                        List<spGetBOQCWLTIndicatorValues_Result> indicatorsThatNeedImprovement = mostRecentBOQIndicatorValues.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQCWLTIndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();

                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = indicatorsThatNeedImprovement;
                        repeatBOQIndicatorsToBeImproved.DataBind();

                        //Bind the labels
                        lblMostRecentBOQDate.Text = mostRecentBOQ.FormDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQCWLTIndicatorValues_Result>();
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

                repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQCWLTIndicatorValues_Result>();
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
                List<CWLTActionPlanActionStep> actionSteps = context.CWLTActionPlanActionStep.AsNoTracking()
                                            .Where(t => t.CWLTActionPlanFK == currentActionPlan.CWLTActionPlanPK)
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
                    CWLTActionPlanActionStep currentActionStep = context.CWLTActionPlanActionStep
                                                                                .Include(a => a.CodeBOQIndicator)
                                                                                .AsNoTracking()
                                                                                .Where(a => a.CWLTActionPlanActionStepPK == actionStepPK.Value)
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
                    CWLTActionPlanActionStep editActionStep = context.CWLTActionPlanActionStep.AsNoTracking().Where(m => m.CWLTActionPlanActionStepPK == actionStepPK.Value).FirstOrDefault();

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
                    CWLTActionPlanActionStep currentActionStep;

                    //Check to see if this is an add or an edit
                    if (actionStepPK == 0)
                    {
                        //Add
                        currentActionStep = new CWLTActionPlanActionStep();
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.CWLTActionPlanFK = currentActionPlanPK;
                        currentActionStep.CreateDate = DateTime.Now;
                        currentActionStep.Creator = User.Identity.Name;

                        //Save to the database
                        context.CWLTActionPlanActionStep.Add(currentActionStep);
                        context.SaveChanges();

                        //Add the initial status record
                        CWLTActionPlanActionStepStatus currentStatusRecord = new CWLTActionPlanActionStepStatus();
                        currentStatusRecord.CWLTActionPlanActionStepFK = currentActionStep.CWLTActionPlanActionStepPK;
                        currentStatusRecord.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepInitialStatus.Value);
                        currentStatusRecord.StatusDate = deActionStepInitialStatusDate.Date;
                        currentStatusRecord.CreateDate = DateTime.Now;
                        currentStatusRecord.Creator = User.Identity.Name;

                        //Save to the database
                        context.CWLTActionPlanActionStepStatus.Add(currentStatusRecord);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStep = context.CWLTActionPlanActionStep.Find(actionStepPK);
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.CWLTActionPlanFK = currentActionPlanPK;
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
                            CWLTActionPlanActionStep actionStepToRemove = context.CWLTActionPlanActionStep.Where(t => t.CWLTActionPlanActionStepPK == rowToRemovePK).FirstOrDefault();

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.CWLTActionPlanActionStepStatus
                                .Where(ass => ass.CWLTActionPlanActionStepFK == actionStepToRemove.CWLTActionPlanActionStepPK).ToList();
                            context.CWLTActionPlanActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Remove the action step
                            context.CWLTActionPlanActionStep.Remove(actionStepToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.CWLTActionPlanActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.CWLTActionPlanActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.CWLTActionPlanActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.CWLTActionPlanActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.CWLTActionPlanActionStepChanged
                                    .OrderByDescending(c => c.CWLTActionPlanActionStepChangedPK)
                                    .Where(c => c.CWLTActionPlanActionStepPK == actionStepToRemove.CWLTActionPlanActionStepPK)
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
                if (ddActionStepInitialStatus.SelectedItem == null)
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
            if (int.TryParse(hfAddEditActionStepPK.Value, out currentActionStepPK) == false || currentActionStepPK == 0)
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
                List<CWLTActionPlanActionStepStatus> actionStepStatuses = context.CWLTActionPlanActionStepStatus
                                            .Include(s => s.CWLTActionPlanActionStep)
                                            .AsNoTracking()
                                            .Where(s => s.CWLTActionPlanActionStepFK == actionStepFK)
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
                    CWLTActionPlanActionStepStatus currentActionStepStatus = context.CWLTActionPlanActionStepStatus
                                                                                .AsNoTracking()
                                                                                .Where(a => a.CWLTActionPlanActionStepStatusPK == actionStepStatusPK.Value)
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
                    CWLTActionPlanActionStepStatus editActionStepStatus = context.CWLTActionPlanActionStepStatus.AsNoTracking().Where(m => m.CWLTActionPlanActionStepStatusPK == actionStepStatusPK.Value).FirstOrDefault();

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
                    CWLTActionPlanActionStepStatus currentActionStepStatus;

                    //Check to see if this is an add or an edit
                    if (actionStepStatusPK == 0)
                    {
                        //Add
                        currentActionStepStatus = new CWLTActionPlanActionStepStatus();
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.CWLTActionPlanActionStepFK = actionStepFK;
                        currentActionStepStatus.CreateDate = DateTime.Now;
                        currentActionStepStatus.Creator = User.Identity.Name;

                        //Save to the database
                        context.CWLTActionPlanActionStepStatus.Add(currentActionStepStatus);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step status!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStepStatus = context.CWLTActionPlanActionStepStatus.Find(actionStepStatusPK);
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.CWLTActionPlanActionStepFK = actionStepFK;
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
                            CWLTActionPlanActionStepStatus actionStepStatusToRemove = context.CWLTActionPlanActionStepStatus.Where(t => t.CWLTActionPlanActionStepStatusPK == rowToRemovePK).FirstOrDefault();

                            //Don't allow deletion if there are no other status records for this action step
                            if (context.CWLTActionPlanActionStepStatus.Where(s => s.CWLTActionPlanActionStepStatusPK != actionStepStatusToRemove.CWLTActionPlanActionStepStatusPK &&
                                                                                        s.CWLTActionPlanActionStepFK == actionStepStatusToRemove.CWLTActionPlanActionStepFK)
                                .Count() > 0)
                            {
                                //Remove the action step status
                                context.CWLTActionPlanActionStepStatus.Remove(actionStepStatusToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.CWLTActionPlanActionStepStatusChanged
                                        .OrderByDescending(c => c.CWLTActionPlanActionStepStatusChangedPK)
                                        .Where(c => c.CWLTActionPlanActionStepStatusPK == actionStepStatusToRemove.CWLTActionPlanActionStepStatusPK)
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