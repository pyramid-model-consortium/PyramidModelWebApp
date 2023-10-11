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
    public partial class SLTActionPlan : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "SLTAP";
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
        private Models.SLTActionPlan currentActionPlan;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["SLTActionPlanPK"]))
            {
                //Parse the pk
                int.TryParse(Request.QueryString["SLTActionPlanPK"], out currentActionPlanPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentActionPlanPK == 0 && !string.IsNullOrWhiteSpace(hfSLTActionPlanPK.Value))
            {
                int.TryParse(hfSLTActionPlanPK.Value, out currentActionPlanPK);
            }

            //Check to see if this is an edit
            isEdit = currentActionPlanPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect back to the dashboard
                Response.Redirect("/Pages/SLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentActionPlan = context.SLTActionPlan.AsNoTracking()
                                        .Include(d => d.State)
                                        .Where(d => d.SLTActionPlanPK == currentActionPlanPK).FirstOrDefault();

                //Check to see if the debrief exists
                if (currentActionPlan == null)
                {
                    //The debrief doesn't exist, set the debrief to a new debrief object
                    currentActionPlan = new Models.SLTActionPlan();
                }
                else
                {
                    //Show a message to the user if this is not the most recent action plan for this state workgroup
                    int numMoreRecentPlans = context.SLTActionPlan.AsNoTracking()
                                                    .Where(ap => ap.SLTActionPlanPK != currentActionPlan.SLTActionPlanPK &&
                                                                 ap.WorkGroupFK == currentActionPlan.WorkGroupFK &&
                                                                 ap.ActionPlanStartDate > currentActionPlan.ActionPlanStartDate)
                                                    .Count();

                    if (numMoreRecentPlans > 0)
                    {
                        divActionPlanAlert.Visible = true;
                        lblActionPlanAlert.Text = "This is not the most recent action plan for this state workgroup.  Any new or changed information should be added to the most recent action plan.";
                    }
                }
            }

            //Prevent users from viewing action plans from other states
            if (isEdit && !currentProgramRole.StateFKs.Contains(currentActionPlan.StateFK))
            {
                //Add a message that will show after redirect
                msgSys.AddMessageToQueue("danger", "Not Found", "The action plan you are attempting to access does not exist.", 10000);

                //Redirect the user back to the dashboard
                Response.Redirect("/Pages/SLTDashboard.aspx");
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

                //Bind the drop-downs
                BindDropDowns();

                //Fill the form with data
                FillFormWithDataFromObject();

                if (isEdit)
                {
                    //Bind the tables
                    BindMeetings();
                    BindGroundRules();

                    //Bind the state info
                    BindStateInformation();

                    //Bind the BOQ information
                    BindMostRecentBOQInformation();

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
                ddState.Focus();

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
                lblActionPlanLastUpdated.Text = (currentActionPlan.EditDate.HasValue ? currentActionPlan.EditDate.Value.ToString("MM/dd/yyyy") : currentActionPlan.CreateDate.ToString("MM/dd/yyyy"));
                ddState.SelectedItem = ddState.Items.FindByValue(currentActionPlan.StateFK);
                ddWorkGroupLead.SelectedItem = ddWorkGroupLead.Items.FindByValue(currentActionPlan.WorkGroupLeadFK);
                deActionPlanStartDate.Value = currentActionPlan.ActionPlanStartDate;
                deActionPlanEndDate.Value = currentActionPlan.ActionPlanEndDate;
                txtMissionStatement.Text = currentActionPlan.MissionStatement;
                txtAdditionalNotes.Text = currentActionPlan.AdditionalNotes;

                //Bind the work group lead email label
                BindWorkGroupLeadEmailLabel();
            }
            else
            {
                lblActionPlanLastUpdated.Text = DateTime.Now.ToString("MM/dd/yyyy");
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
            ddState.ClientEnabled = enabled;
            deActionPlanStartDate.ClientEnabled = enabled;
            deActionPlanEndDate.ClientEnabled = enabled;
            ddWorkGroup.ClientEnabled = enabled;
            ddWorkGroupLead.ClientEnabled = enabled;
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
            List<State> allStates = new List<State>();
            List<CodeTeamPosition> allTeamPositions = new List<CodeTeamPosition>();
            List<CodeBOQIndicator> allIndicators = new List<CodeBOQIndicator>();
            List<CodeActionPlanActionStepStatus> allActionStepStatuses = new List<CodeActionPlanActionStepStatus>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allStates = context.State.AsNoTracking()
                                             .Where(p => currentProgramRole.StateFKs.Contains(p.StatePK))
                                             .OrderBy(p => p.Name)
                                             .ToList();
                allIndicators = context.CodeBOQIndicator.Include(i => i.CodeBOQCriticalElement).AsNoTracking().Where(i => i.CodeBOQCriticalElement.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQSLT).OrderBy(i => i.OrderBy).ToList();
                allActionStepStatuses = context.CodeActionPlanActionStepStatus.AsNoTracking().OrderBy(s => s.OrderBy).ToList();
            }

            //Bind the state dropdown
            ddState.DataSource = allStates;
            ddState.DataBind();

            if (isEdit)
            {
                BindSLTMemberControls(currentActionPlan.StateFK, currentActionPlan.ActionPlanStartDate, currentActionPlan.ActionPlanEndDate, currentActionPlan.WorkGroupFK, currentActionPlan.WorkGroupLeadFK);
                BindSLTWorkGroupDropDown(currentActionPlan.StateFK, currentActionPlan.ActionPlanStartDate, currentActionPlan.ActionPlanEndDate, currentActionPlan.WorkGroupFK);
            }
            else
            {
                BindSLTMemberControls(null, null, null, null, null);
                BindSLTWorkGroupDropDown(null, null, null, null);
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
                    Response.Redirect(string.Format("/Pages/SLTDashboard.aspx"));
                }
                else
                {
                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Action Plan was successfully added!<br/><br/>More detailed information can now be added to the Action Plan.", 10000);

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/SLTActionPlan.aspx?SLTActionPlanPK={0}&Action=Edit",
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

            //Redirect the user to the SLT dashboard
            Response.Redirect("/Pages/SLTDashboard.aspx");
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
                    Reports.PreBuiltReports.FormReports.RptSLTActionPlan report = new Reports.PreBuiltReports.FormReports.RptSLTActionPlan();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "State Leadership Team Action Plan", currentActionPlanPK);
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
                    Response.Redirect(string.Format("/Pages/SLTActionPlan.aspx?SLTActionPlanPK={0}&Action={1}&Print=True",
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
                currentActionPlan.MissionStatement = (string.IsNullOrWhiteSpace(txtMissionStatement.Text) ? null : txtMissionStatement.Text);
                currentActionPlan.AdditionalNotes = (string.IsNullOrWhiteSpace(txtAdditionalNotes.Text) ? null : txtAdditionalNotes.Text);
                currentActionPlan.StateFK = Convert.ToInt32(ddState.Value);
                currentActionPlan.WorkGroupLeadFK = Convert.ToInt32(ddWorkGroupLead.Value);
                currentActionPlan.WorkGroupFK = Convert.ToInt32(ddWorkGroup.Value);

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit-only fields
                        currentActionPlan.EditDate = DateTime.Now;
                        currentActionPlan.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.SLTActionPlan existingActionPlan = context.SLTActionPlan.Find(currentActionPlan.SLTActionPlanPK);

                        //Set the object to the new values
                        context.Entry(existingActionPlan).CurrentValues.SetValues(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTActionPlanPK.Value = currentActionPlan.SLTActionPlanPK.ToString();
                        currentActionPlanPK = currentActionPlan.SLTActionPlanPK;

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
                        context.SLTActionPlan.Add(currentActionPlan);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTActionPlanPK.Value = currentActionPlan.SLTActionPlanPK.ToString();
                        currentActionPlanPK = currentActionPlan.SLTActionPlanPK;

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
        /// This method fires when the validation for the ddWorkGroup DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddWorkGroup BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddWorkGroup_Validation(object sender, ValidationEventArgs e)
        {
            //Get the values
            int? selectedStateFK = (ddState.Value == null ? (int?)null : Convert.ToInt32(ddState.Value));
            int? selectedWorkGroupFK = (ddWorkGroup.Value == null ? (int?)null : Convert.ToInt32(ddWorkGroup.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);

            //Validate
            if (selectedWorkGroupFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Work Group is required!";
            }
            else if (currentActionPlan.IsPrefilled == false || chkReviewedBasicInfo.Checked)
            {
                //Check for duplication
                if (actionPlanStartDate.HasValue && actionPlanEndDate.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check to see if there are any schedules for this state, workgroup and timeframe
                        List<Models.SLTActionPlan> duplicateActionPlans = context.SLTActionPlan.AsNoTracking()
                                        .Where(d => d.StateFK == selectedStateFK.Value &&
                                                d.WorkGroupFK == selectedWorkGroupFK.Value &&
                                                d.SLTActionPlanPK != currentActionPlanPK &&
                                                ((d.ActionPlanStartDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanStartDate >= deActionPlanStartDate.Date) ||
                                                (d.ActionPlanEndDate <= deActionPlanEndDate.Date &&
                                                d.ActionPlanEndDate >= deActionPlanStartDate.Date))).ToList();

                        //Check the count of duplicate action plans
                        if (duplicateActionPlans.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "There is already an action plan for the selected state workgroup within the timeframe entered here!";
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
            //Get the values
            int? selectedStateFK = (ddState.Value == null ? (int?)null : Convert.ToInt32(ddState.Value));
            int? selectedWorkGroupFK = (ddWorkGroup.Value == null ? (int?)null : Convert.ToInt32(ddWorkGroup.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);

            //Check to see if the date was selected
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
                        List<Models.SLTActionPlanMeeting> invalidMeetingDates = context.SLTActionPlanMeeting
                                                    .AsNoTracking()
                                                    .Where(m => m.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK &&
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
                            List<Models.SLTActionPlanActionStep> invalidActionSteps = context.SLTActionPlanActionStep
                                                        .Include(s => s.CodeBOQIndicator)
                                                        .Include(s => s.CodeBOQIndicator.CodeBOQCriticalElement)
                                                        .AsNoTracking()
                                                        .Where(s => s.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK &&
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
        /// This method fires when the user selects a date from the ActionPlanDate control.
        /// </summary>
        /// <param name="sender">The ActionPlanDate BootstrapDateEdit</param>
        /// <param name="e">EventArgs</param>
        protected void ActionPlanDate_ValueChanged(object sender, EventArgs e)
        {
            var currentDateEdit = (BootstrapDateEdit)sender;

            //Get the state, dates, work group, and work group lead
            int? selectedStateFK = (ddState.SelectedItem == null ? (int?)null : Convert.ToInt32(ddState.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);
            int? selectedWorkGroupFK = (ddWorkGroup.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroup.Value));
            int? selectedWorkGroupLeadFK = (ddWorkGroupLead.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroupLead.Value));

            //Bind the drop-downs that are dependent on the selected information
            BindSLTMemberControls(selectedStateFK, actionPlanStartDate, actionPlanEndDate, selectedWorkGroupFK, selectedWorkGroupLeadFK);
            BindSLTWorkGroupDropDown(selectedStateFK, actionPlanStartDate, actionPlanEndDate, selectedWorkGroupFK);

            //Bind the state information
            BindStateInformation();

            //Bind the BOQ information
            BindMostRecentBOQInformation();

            //Re-focus on the control
            currentDateEdit.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a state from the ddState control.
        /// </summary>
        /// <param name="sender">The ddState BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddState_ValueChanged(object sender, EventArgs e)
        {
            //Get the necessary information
            int? selectedStateFK = (ddState.SelectedItem == null ? (int?)null : Convert.ToInt32(ddState.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);
            int? selectedWorkGroupFK = (ddWorkGroup.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroup.Value));
            int? selectedWorkGroupLeadFK = (ddWorkGroupLead.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroupLead.Value));

            //Bind the drop-downs that are dependent on the selected information
            BindSLTMemberControls(selectedStateFK, actionPlanStartDate, actionPlanEndDate, selectedWorkGroupFK, selectedWorkGroupLeadFK);
            BindSLTWorkGroupDropDown(selectedStateFK, actionPlanStartDate, actionPlanEndDate, selectedWorkGroupFK);

            //Bind the state information
            BindStateInformation();

            //Bind the BOQ information
            BindMostRecentBOQInformation();

            //Re-focus on the control
            ddState.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a workgroup from the ddWorkGroup control.
        /// </summary>
        /// <param name="sender">The ddWorkGroup BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddWorkGroup_ValueChanged(object sender, EventArgs e)
        {
            //Get the necessary information
            int? selectedStateFK = (ddState.SelectedItem == null ? (int?)null : Convert.ToInt32(ddState.Value));
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);
            int? selectedWorkGroupFK = (ddWorkGroup.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroup.Value));
            int? selectedWorkGroupLeadFK = (ddWorkGroupLead.SelectedItem == null ? (int?)null : Convert.ToInt32(ddWorkGroupLead.Value));

            //Bind the drop-downs that are dependent on the changed information
            BindSLTMemberControls(selectedStateFK, actionPlanStartDate, actionPlanEndDate, selectedWorkGroupFK, selectedWorkGroupLeadFK);

            //Re-focus on the control
            ddWorkGroup.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a work group lead from the ddWorkGroupLead control.
        /// </summary>
        /// <param name="sender">The ddWorkGroupLead BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddWorkGroupLead_ValueChanged(object sender, EventArgs e)
        {
            //Bind the email label
            BindWorkGroupLeadEmailLabel();

            //Re-focus on the control
            ddWorkGroupLead.Focus();
        }

        /// <summary>
        /// This method binds the work group lead email label based on the selected work group lead
        /// </summary>
        private void BindWorkGroupLeadEmailLabel()
        {
            //Try to get the email
            if (ddWorkGroupLead.SelectedItem != null && int.TryParse(ddWorkGroupLead.Value.ToString(), out int workGroupLeadPK))
            {
                //To hold the selected work group lead
                Models.SLTMember selectedWorkGroupLead;

                //To hold the selected State
                using (PyramidContext context = new PyramidContext())
                {
                    selectedWorkGroupLead = context.SLTMember.AsNoTracking().Where(cm => cm.SLTMemberPK == workGroupLeadPK).FirstOrDefault();
                }

                //Set the label
                if (selectedWorkGroupLead != null)
                {
                    lblWorkGroupLeadEmail.Text = selectedWorkGroupLead.EmailAddress;
                }
                else
                {
                    lblWorkGroupLeadEmail.Text = "";
                }
            }
            else
            {
                //Clear the label
                lblWorkGroupLeadEmail.Text = "";
            }
        }

        /// <summary>
        /// This method binds the work group dropdown
        /// </summary>
        private void BindSLTWorkGroupDropDown(int? currentStateFK, DateTime? currentActionPlanStartDate, DateTime? currentActionPlanEndDate, int? currentWorkGroupFK)
        {
            //Only bind the dropdowns if the dates, state, and work group have been selected
            if (currentActionPlanStartDate.HasValue &&
                currentActionPlanEndDate.HasValue &&
                currentStateFK.HasValue)
            {
                //Get the work groups
                List<Models.SLTWorkGroup> activeWorkGroups;

                using (PyramidContext context = new PyramidContext())
                {
                    activeWorkGroups = context.SLTWorkGroup.AsNoTracking()
                                                .Where(wg => wg.StateFK == currentStateFK.Value &&  //Only get work groups for the state
                                                       wg.StartDate <= currentActionPlanEndDate.Value &&  //Only get the workgroups that are active
                                                       (wg.EndDate.HasValue == false ||
                                                            wg.EndDate >= currentActionPlanStartDate.Value))
                                                .ToList();
                }

                //----Work Group drop-down-----

                //Bind the work group drop-down
                ddWorkGroup.DataSource = activeWorkGroups;
                ddWorkGroup.DataBind();

                //Try to re-select the work group
                if (currentWorkGroupFK.HasValue)
                {
                    ddWorkGroup.SelectedItem = ddWorkGroup.Items.FindByValue(currentWorkGroupFK.Value);
                }

                //----End Work Group drop-down-----
            }
            else
            {
                //Clear the drop-down
                ddWorkGroup.Value = "";
            }
        }

        /// <summary>
        /// This method binds the controls that are filled with active leadership team members
        /// </summary>
        private void BindSLTMemberControls(int? currentStateFK, DateTime? currentActionPlanStartDate, DateTime? currentActionPlanEndDate, int? currentWorkGroupFK, int? currentWorkGroupLeadFK)
        {
            //Only bind the dropdowns if the dates, state, and work group have been selected
            if (currentActionPlanStartDate.HasValue &&
                currentActionPlanEndDate.HasValue &&
                currentWorkGroupFK.HasValue)
            {
                //To hold the active SLT members
                List<Models.SLTMember> activeSLTMembers;

                //Get the active SLT members
                using (PyramidContext context = new PyramidContext())
                {
                    activeSLTMembers = context.SLTMemberWorkGroupAssignment.AsNoTracking()
                                                .Include(a => a.SLTMember)
                                                .Include(a => a.SLTMember.State)
                                                .Where(a => a.SLTMember.StateFK == currentStateFK.Value &&  //Only get SLT members for this state
                                                       a.SLTWorkGroupFK == currentWorkGroupFK.Value &&  //Only get the SLT members that have an active workgroup assignment to this workgroup
                                                       a.StartDate <= currentActionPlanEndDate.Value &&
                                                       (a.EndDate.HasValue == false ||
                                                            a.EndDate.Value >= currentActionPlanStartDate.Value) &&
                                                       a.SLTMember.StartDate <= currentActionPlanEndDate.Value &&  //Only get the SLT members that are active
                                                       (a.SLTMember.LeaveDate.HasValue == false ||
                                                            a.SLTMember.LeaveDate >= currentActionPlanStartDate.Value))
                                                .Select(a => a.SLTMember)
                                                .Include(a => a.State)
                                                .ToList();
                }

                //----Work Group Lead drop-down-----

                //Bind the work group lead drop-down
                ddWorkGroupLead.DataSource = activeSLTMembers.Select(cm => new {
                    cm.SLTMemberPK,
                    IDNumberAndName = string.Format("({0}) {1} {2}", cm.IDNumber, cm.FirstName, cm.LastName)
                }).ToList();
                ddWorkGroupLead.DataBind();

                //Try to re-select the work group lead
                if (currentWorkGroupLeadFK.HasValue)
                {
                    ddWorkGroupLead.SelectedItem = ddWorkGroupLead.Items.FindByValue(currentWorkGroupLeadFK.Value);
                }

                //If there is no selected work group lead, clear the email label
                if (ddWorkGroupLead.SelectedItem == null)
                {
                    //Clear the email label
                    lblWorkGroupLeadEmail.Text = "";
                }

                //----End Work Group Lead drop-down-----

                //----Leadership Team member repeater----

                repeatLeadershipTeamMembers.DataSource = activeSLTMembers;
                repeatLeadershipTeamMembers.DataBind();

                //----End Leadership Team member repeater----
            }
            else
            {
                //Clear the drop-downs
                ddWorkGroupLead.Value = "";

                //Clear the email label
                lblWorkGroupLeadEmail.Text = "";

                //Bind the repeater to an empty list
                repeatLeadershipTeamMembers.DataSource = new List<Models.SLTMember>();
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
                List<SLTActionPlanMeeting> Meetings = context.SLTActionPlanMeeting.AsNoTracking()
                                            .Where(t => t.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK)
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
                    SLTActionPlanMeeting currentMeeting = context.SLTActionPlanMeeting.AsNoTracking().Where(m => m.SLTActionPlanMeetingPK == meetingPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditMeeting.Text = "View Meeting";
                    deMeetingDate.Date = currentMeeting.MeetingDate;
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
                    SLTActionPlanMeeting editMeeting = context.SLTActionPlanMeeting.AsNoTracking().Where(m => m.SLTActionPlanMeetingPK == meetingPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditMeeting.Text = "Edit Meeting";
                    deMeetingDate.Date = editMeeting.MeetingDate;
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
                    SLTActionPlanMeeting currentMeeting;

                    //Check to see if this is an add or an edit
                    if (meetingPK == 0)
                    {
                        //Add
                        currentMeeting = new SLTActionPlanMeeting();
                        currentMeeting.MeetingDate = deMeetingDate.Date;
                        currentMeeting.MeetingNotes = (string.IsNullOrWhiteSpace(txtMeetingNotes.Text) ? null : txtMeetingNotes.Text);
                        currentMeeting.SLTActionPlanFK = currentActionPlanPK;
                        currentMeeting.CreateDate = DateTime.Now;
                        currentMeeting.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTActionPlanMeeting.Add(currentMeeting);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added meeting!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentMeeting = context.SLTActionPlanMeeting.Find(meetingPK);
                        currentMeeting.MeetingDate = deMeetingDate.Date;
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
                            SLTActionPlanMeeting meetingToRemove = context.SLTActionPlanMeeting.Where(t => t.SLTActionPlanMeetingPK == rowToRemovePK).FirstOrDefault();

                            //Remove the meeting
                            context.SLTActionPlanMeeting.Remove(meetingToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTActionPlanMeetingChanged
                                    .OrderByDescending(c => c.SLTActionPlanMeetingChangedPK)
                                    .Where(c => c.SLTActionPlanMeetingPK == meetingToRemove.SLTActionPlanMeetingPK)
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
            //Get the meeting date and action plan dates
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

        #region Ground Rules

        /// <summary>
        /// This method populates the ground rule repeater with up-to-date information
        /// </summary>
        private void BindGroundRules()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<SLTActionPlanGroundRule> GroundRules = context.SLTActionPlanGroundRule.AsNoTracking()
                                            .Where(t => t.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK)
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
                    SLTActionPlanGroundRule currentGroundRule = context.SLTActionPlanGroundRule.AsNoTracking().Where(m => m.SLTActionPlanGroundRulePK == groundRulePK.Value).FirstOrDefault();

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
                    SLTActionPlanGroundRule editGroundRule = context.SLTActionPlanGroundRule.AsNoTracking().Where(m => m.SLTActionPlanGroundRulePK == groundRulePK.Value).FirstOrDefault();

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
                    SLTActionPlanGroundRule currentGroundRule;

                    //Check to see if this is an add or an edit
                    if (groundRulePK == 0)
                    {
                        //Add
                        currentGroundRule = new SLTActionPlanGroundRule();
                        currentGroundRule.GroundRuleDescription = txtGroundRuleDescription.Text;
                        currentGroundRule.GroundRuleNumber = Convert.ToInt32(txtGroundRuleNumber.Text);
                        currentGroundRule.SLTActionPlanFK = currentActionPlanPK;
                        currentGroundRule.CreateDate = DateTime.Now;
                        currentGroundRule.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTActionPlanGroundRule.Add(currentGroundRule);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added ground rule!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentGroundRule = context.SLTActionPlanGroundRule.Find(groundRulePK);
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
                            SLTActionPlanGroundRule assignmentToRemove = context.SLTActionPlanGroundRule.Where(t => t.SLTActionPlanGroundRulePK == rowToRemovePK).FirstOrDefault();

                            //Remove the ground rule
                            context.SLTActionPlanGroundRule.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTActionPlanGroundRuleChanged
                                    .OrderByDescending(c => c.SLTActionPlanGroundRuleChangedPK)
                                    .Where(c => c.SLTActionPlanGroundRulePK == assignmentToRemove.SLTActionPlanGroundRulePK)
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
                    int numOtherGroundRules = context.SLTActionPlanGroundRule.AsNoTracking().Where(r => r.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK).Count();

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

        #region State Information

        /// <summary>
        /// This method populates the staff information labels with up-to-date information
        /// </summary>
        private void BindStateInformation()
        {
            //To hold the necessary values
            int selectedStateFK;
            DateTime? actionPlanStartDate = (deActionPlanStartDate.Value == null ? (DateTime?)null : deActionPlanStartDate.Date);
            DateTime? actionPlanEndDate = (deActionPlanEndDate.Value == null ? (DateTime?)null : deActionPlanEndDate.Date);

            //Make sure the dates and state are valid
            if (actionPlanStartDate.HasValue &&
                actionPlanEndDate.HasValue &&
                ddState.Value != null &&
                int.TryParse(ddState.Value.ToString(), out selectedStateFK))
            {
                //To hold the numbers
                int numActivePrograms;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the counts
                    //Active programs
                    numActivePrograms = context.Program.AsNoTracking()
                                                    .Where(p => p.StateFK == selectedStateFK &&
                                                                p.ProgramStartDate <= actionPlanEndDate.Value &&
                                                                (p.ProgramEndDate.HasValue == false || p.ProgramEndDate.Value >= actionPlanStartDate.Value))
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
        private void BindMostRecentBOQInformation()
        {
            //To hold the necessary values
            int selectedStateFK;
            DateTime actionPlanStartDate = deActionPlanStartDate.Date;
            DateTime actionPlanEndDate = deActionPlanEndDate.Date;

            //Make sure the dates and state are valid
            if (actionPlanStartDate > DateTime.MinValue &&
                actionPlanEndDate > DateTime.MinValue &&
                ddState.Value != null &&
                int.TryParse(ddState.Value.ToString(), out selectedStateFK))
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the action plan start date minus 6 months for the calculation below
                    DateTime startOfTimeframe = actionPlanStartDate.AddMonths(-6);

                    //Get the most recent BOQ
                    BenchmarkOfQualitySLT mostRecentBOQ = context.BenchmarkOfQualitySLT.AsNoTracking()
                                                    .Where(boq => boq.StateFK == selectedStateFK &&
                                                                  boq.FormDate >= startOfTimeframe &&
                                                                  boq.FormDate <= actionPlanEndDate)
                                                    .OrderByDescending(boq => boq.FormDate)
                                                    .FirstOrDefault();

                    //Get all the critical elements
                    List<CodeBOQCriticalElement> allCriticalElements = context.CodeBOQCriticalElement.AsNoTracking()
                                                        .Where(ce => ce.BOQTypeCodeFK == (int)CodeBOQType.BOQTypes.BOQSLT)
                                                        .OrderBy(ce => ce.OrderBy)
                                                        .ToList();

                    //Make sure there is a most recent BOQ
                    if (mostRecentBOQ != null && mostRecentBOQ.BenchmarkOfQualitySLTPK > 0)
                    {
                        //Get the BOQ indicator information
                        List<spGetBOQSLTIndicatorValues_Result> mostRecentBOQIndicatorValues = context.spGetBOQSLTIndicatorValues(mostRecentBOQ.BenchmarkOfQualitySLTPK).ToList();

                        //Get the indicators that need improvement
                        List<spGetBOQSLTIndicatorValues_Result> indicatorsThatNeedImprovement = mostRecentBOQIndicatorValues.Where(iv => iv.IndicatorValue != (int)CodeBOQIndicatorValue.BOQSLTIndicatorValues.IN_PLACE).OrderBy(iv => iv.IndicatorNumber).ToList();

                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = indicatorsThatNeedImprovement;
                        repeatBOQIndicatorsToBeImproved.DataBind();

                        //Bind the labels
                        lblMostRecentBOQDate.Text = mostRecentBOQ.FormDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        //Bind the repeaters
                        repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQSLTIndicatorValues_Result>();
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

                repeatBOQIndicatorsToBeImproved.DataSource = new List<spGetBOQSLTIndicatorValues_Result>();
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
                List<SLTActionPlanActionStep> actionSteps = context.SLTActionPlanActionStep.AsNoTracking()
                                            .Where(t => t.SLTActionPlanFK == currentActionPlan.SLTActionPlanPK)
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
                    SLTActionPlanActionStep currentActionStep = context.SLTActionPlanActionStep
                                                                                .Include(a => a.CodeBOQIndicator)
                                                                                .AsNoTracking()
                                                                                .Where(a => a.SLTActionPlanActionStepPK == actionStepPK.Value)
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
                    SLTActionPlanActionStep editActionStep = context.SLTActionPlanActionStep.AsNoTracking().Where(m => m.SLTActionPlanActionStepPK == actionStepPK.Value).FirstOrDefault();

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
                    SLTActionPlanActionStep currentActionStep;

                    //Check to see if this is an add or an edit
                    if (actionStepPK == 0)
                    {
                        //Add
                        currentActionStep = new SLTActionPlanActionStep();
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.SLTActionPlanFK = currentActionPlanPK;
                        currentActionStep.CreateDate = DateTime.Now;
                        currentActionStep.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTActionPlanActionStep.Add(currentActionStep);
                        context.SaveChanges();

                        //Add the initial status record
                        SLTActionPlanActionStepStatus currentStatusRecord = new SLTActionPlanActionStepStatus();
                        currentStatusRecord.SLTActionPlanActionStepFK = currentActionStep.SLTActionPlanActionStepPK;
                        currentStatusRecord.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepInitialStatus.Value);
                        currentStatusRecord.StatusDate = deActionStepInitialStatusDate.Date;
                        currentStatusRecord.CreateDate = DateTime.Now;
                        currentStatusRecord.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTActionPlanActionStepStatus.Add(currentStatusRecord);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStep = context.SLTActionPlanActionStep.Find(actionStepPK);
                        currentActionStep.BOQIndicatorCodeFK = Convert.ToInt32(ddActionStepIndicator.Value);
                        currentActionStep.ProblemIssueTask = (string.IsNullOrWhiteSpace(txtActionStepProblemIssueTask.Text) ? null : txtActionStepProblemIssueTask.Text);
                        currentActionStep.ActionStepActivity = (string.IsNullOrWhiteSpace(txtActionStepActivity.Text) ? null : txtActionStepActivity.Text);
                        currentActionStep.PersonsResponsible = (string.IsNullOrWhiteSpace(txtActionStepPersonsResponsible.Text) ? null : txtActionStepPersonsResponsible.Text);
                        currentActionStep.TargetDate = deActionStepTargetDate.Date;
                        currentActionStep.SLTActionPlanFK = currentActionPlanPK;
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
                            SLTActionPlanActionStep actionStepToRemove = context.SLTActionPlanActionStep.Where(t => t.SLTActionPlanActionStepPK == rowToRemovePK).FirstOrDefault();

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.SLTActionPlanActionStepStatus
                                .Where(ass => ass.SLTActionPlanActionStepFK == actionStepToRemove.SLTActionPlanActionStepPK).ToList();
                            context.SLTActionPlanActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Remove the action step
                            context.SLTActionPlanActionStep.Remove(actionStepToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.SLTActionPlanActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.SLTActionPlanActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.SLTActionPlanActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.SLTActionPlanActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.SLTActionPlanActionStepChanged
                                    .OrderByDescending(c => c.SLTActionPlanActionStepChangedPK)
                                    .Where(c => c.SLTActionPlanActionStepPK == actionStepToRemove.SLTActionPlanActionStepPK)
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
                List<SLTActionPlanActionStepStatus> actionStepStatuses = context.SLTActionPlanActionStepStatus
                                            .Include(s => s.SLTActionPlanActionStep)
                                            .AsNoTracking()
                                            .Where(s => s.SLTActionPlanActionStepFK == actionStepFK)
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
                    SLTActionPlanActionStepStatus currentActionStepStatus = context.SLTActionPlanActionStepStatus
                                                                                .AsNoTracking()
                                                                                .Where(a => a.SLTActionPlanActionStepStatusPK == actionStepStatusPK.Value)
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
                    SLTActionPlanActionStepStatus editActionStepStatus = context.SLTActionPlanActionStepStatus.AsNoTracking().Where(m => m.SLTActionPlanActionStepStatusPK == actionStepStatusPK.Value).FirstOrDefault();

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
                    SLTActionPlanActionStepStatus currentActionStepStatus;

                    //Check to see if this is an add or an edit
                    if (actionStepStatusPK == 0)
                    {
                        //Add
                        currentActionStepStatus = new SLTActionPlanActionStepStatus();
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.SLTActionPlanActionStepFK = actionStepFK;
                        currentActionStepStatus.CreateDate = DateTime.Now;
                        currentActionStepStatus.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTActionPlanActionStepStatus.Add(currentActionStepStatus);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added action step status!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentActionStepStatus = context.SLTActionPlanActionStepStatus.Find(actionStepStatusPK);
                        currentActionStepStatus.ActionPlanActionStepStatusCodeFK = Convert.ToInt32(ddActionStepStatus.Value);
                        currentActionStepStatus.StatusDate = deActionStepStatusDate.Date;
                        currentActionStepStatus.SLTActionPlanActionStepFK = actionStepFK;
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
                            SLTActionPlanActionStepStatus actionStepStatusToRemove = context.SLTActionPlanActionStepStatus.Where(t => t.SLTActionPlanActionStepStatusPK == rowToRemovePK).FirstOrDefault();

                            //Don't allow deletion if there are no other status records for this action step
                            if (context.SLTActionPlanActionStepStatus.Where(s => s.SLTActionPlanActionStepStatusPK != actionStepStatusToRemove.SLTActionPlanActionStepStatusPK &&
                                                                                        s.SLTActionPlanActionStepFK == actionStepStatusToRemove.SLTActionPlanActionStepFK)
                                .Count() > 0)
                            {
                                //Remove the action step status
                                context.SLTActionPlanActionStepStatus.Remove(actionStepStatusToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.SLTActionPlanActionStepStatusChanged
                                        .OrderByDescending(c => c.SLTActionPlanActionStepStatusChangedPK)
                                        .Where(c => c.SLTActionPlanActionStepStatusPK == actionStepStatusToRemove.SLTActionPlanActionStepStatusPK)
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