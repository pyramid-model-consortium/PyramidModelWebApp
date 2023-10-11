using DevExpress.Web;
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
    public partial class HubLCMeetingDebrief : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "LCHD";
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
        private Models.HubLCMeetingDebrief currentMeetingDebrief;
        private int currentDebriefPK = 0;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["LCMeetingDebriefPK"]))
            {
                //Parse the member pk
                int.TryParse(Request.QueryString["LCMeetingDebriefPK"], out currentDebriefPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentDebriefPK == 0 && !string.IsNullOrWhiteSpace(hfHubLCMeetingDebriefPK.Value))
            {
                int.TryParse(hfHubLCMeetingDebriefPK.Value, out currentDebriefPK);
            }

            //Check to see if this is an edit
            isEdit = currentDebriefPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/LeadershipCoachDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the debrief object
                currentMeetingDebrief = context.HubLCMeetingDebrief.AsNoTracking()
                                        .Include(d => d.Hub)
                                        .Where(d => d.HubLCMeetingDebriefPK == currentDebriefPK).FirstOrDefault();

                //Check to see if the debrief exists
                if (currentMeetingDebrief == null)
                {
                    //The debrief doesn't exist, set the debrief to a new debrief object
                    currentMeetingDebrief = new Models.HubLCMeetingDebrief();

                    //Set the hub labels to blank
                    lblHubProgramCount.Text = "";
                }
                else if(currentMeetingDebrief.Hub.Program != null)
                { 
                    //Set the hub labels
                    lblHubProgramCount.Text = currentMeetingDebrief.Hub.Program.Count.ToString();
                }
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "DebriefAdded":
                            msgSys.ShowMessageToUser("success", "Success", "The meeting debrief was successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

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

                //Bind the tables
                BindTeamMembers();
                BindDebriefSessions();

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
                    lblPageTitle.Text = "Add New Hub Leadership Team Meeting Debrief";
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
                    lblPageTitle.Text = "Edit Hub Leadership Team Meeting Debrief";
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
                    lblPageTitle.Text = "View Hub Leadership Team Meeting Debrief";
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
                //Fill the input fields
                ddHub.SelectedItem = ddHub.Items.FindByValue(currentMeetingDebrief.HubFK);
                deDebriefYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentMeetingDebrief.DebriefYear));
                txtDebriefLeadOrganization.Value = currentMeetingDebrief.LeadOrganization;
                txtDebriefAddress.Value = currentMeetingDebrief.LocationAddress;
                txtDebriefEmail.Value = currentMeetingDebrief.PrimaryContactEmail;
                txtDebriefPhone.Value = currentMeetingDebrief.PrimaryContactPhone;

                //Fill the leadership coach label
                PyramidUser currentLeadershipCoach = PyramidUser.GetUserRecordByUsername(currentMeetingDebrief.LeadershipCoachUsername);
                lblLeadershipCoach.Text = string.Format("{0} {1} ({2})", currentLeadershipCoach.FirstName, currentLeadershipCoach.LastName, currentLeadershipCoach.UserName);
            }
            else
            {
                if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH)
                {
                    //Fill the leadership coach label
                    PyramidUser currentLeadershipCoach = PyramidUser.GetUserRecordByUsername(User.Identity.Name);
                    lblLeadershipCoach.Text = string.Format("{0} {1} ({2})", currentLeadershipCoach.FirstName, currentLeadershipCoach.LastName, currentLeadershipCoach.UserName);
                }
                else
                {
                    //User is not a coach, show a message after a redirect back to the dashboard
                    msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized because you are not logged in as a Hub Leadership Coach!", 10000);
                    Response.Redirect("/Pages/LeadershipCoachDashboard.aspx");
                }
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddHub.ClientEnabled = enabled;
            deDebriefYear.ClientEnabled = enabled;
            txtDebriefLeadOrganization.ClientEnabled = enabled;
            txtDebriefAddress.ClientEnabled = enabled;
            txtDebriefEmail.ClientEnabled = enabled;
            txtDebriefPhone.ClientEnabled = enabled;

            //Show/hide the submit button
            submitMeetingDebrief.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitMeetingDebrief.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            //To hold all the necessary items
            List<Hub> allHubs = new List<Hub>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allHubs = context.Hub.AsNoTracking().Where(h => currentProgramRole.HubFKs.Contains(h.HubPK)).OrderBy(h => h.Name).ToList();
            }

            //Bind the hub dropdown
            ddHub.DataSource = allHubs;
            ddHub.DataBind();
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitMeetingDebrief user control 
        /// </summary>
        /// <param name="sender">The submitMeetingDebrief control</param>
        /// <param name="e">The Click event</param>
        protected void submitMeetingDebrief_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect differently if add or edit
                if (isEdit)
                {
                    //Redirect the user to the dashboard
                    Response.Redirect(string.Format("/Pages/LeadershipCoachDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/HubLCMeetingDebrief.aspx?LCMeetingDebriefPK={0}&Action=Edit&messageType={1}",
                                                        currentDebriefPK, successMessageType));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitMeetingDebrief user control 
        /// </summary>
        /// <param name="sender">The submitMeetingDebrief control</param>
        /// <param name="e">The Click event</param>
        protected void submitMeetingDebrief_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the CWLT dashboard
            Response.Redirect(string.Format("/Pages/LeadershipCoachDashboard.aspx?messageType={0}", "DebriefCancelled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitMeetingDebrief control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitMeetingDebrief_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitMeetingDebrief.ValidationGroup))
            {
                //To hold the success message type
                string successMessageType;

                //Submit the form
                successMessageType = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptHubLCMeetingDebrief report = new Reports.PreBuiltReports.FormReports.RptHubLCMeetingDebrief();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Hub Leadership Team Meeting Debrief", currentDebriefPK);
                }
                else
                {
                    //Get the action
                    string action = "View";
                    if (!string.IsNullOrWhiteSpace(successMessageType))
                    {
                        //The save was successful, the user will be editing
                        action = "Edit";
                    }

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/HubLCMeetingDebrief.aspx?LCMeetingDebriefPK={0}&Action={1}&messageType={2}&Print=True",
                                                        currentDebriefPK, action, successMessageType));
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
        private string SaveForm(bool showMessages)
        {
            //To hold the type of change
            string successMessageType = null;

            //Check the user permissions
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentMeetingDebrief.DebriefYear = deDebriefYear.Date.Year;
                currentMeetingDebrief.LeadOrganization = (string.IsNullOrWhiteSpace(txtDebriefLeadOrganization.Text) ? null : txtDebriefLeadOrganization.Text);
                currentMeetingDebrief.LocationAddress = (string.IsNullOrWhiteSpace(txtDebriefAddress.Text) ? null : txtDebriefAddress.Text);
                currentMeetingDebrief.PrimaryContactEmail = (string.IsNullOrWhiteSpace(txtDebriefEmail.Text) ? null : txtDebriefEmail.Text);
                currentMeetingDebrief.PrimaryContactPhone = (string.IsNullOrWhiteSpace(txtDebriefPhone.Text) ? null : txtDebriefPhone.Text);
                currentMeetingDebrief.HubFK = Convert.ToInt32(ddHub.Value);

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "DebriefEdited";

                        //Set the edit-only fields
                        currentMeetingDebrief.EditDate = DateTime.Now;
                        currentMeetingDebrief.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.HubLCMeetingDebrief existingMeetingDebrief = context.HubLCMeetingDebrief.Find(currentMeetingDebrief.HubLCMeetingDebriefPK);

                        //Set the member object to the new values
                        context.Entry(existingMeetingDebrief).CurrentValues.SetValues(currentMeetingDebrief);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfHubLCMeetingDebriefPK.Value = currentMeetingDebrief.HubLCMeetingDebriefPK.ToString();
                        currentDebriefPK = currentMeetingDebrief.HubLCMeetingDebriefPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "DebriefAdded";

                        //Set the create-only fields
                        currentMeetingDebrief.CreateDate = DateTime.Now;
                        currentMeetingDebrief.Creator = User.Identity.Name;
                        currentMeetingDebrief.LeadershipCoachUsername = User.Identity.Name;

                        //Add it to the context
                        context.HubLCMeetingDebrief.Add(currentMeetingDebrief);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfHubLCMeetingDebriefPK.Value = currentMeetingDebrief.HubLCMeetingDebriefPK.ToString();
                        currentDebriefPK = currentMeetingDebrief.HubLCMeetingDebriefPK;
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
        /// This method fires when the validation for the ddHub DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddHub BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddHub_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedHubFK;
            string leadershipCoachUsername;
            int selectedYear;

            //Get the necessary values
            leadershipCoachUsername = (isEdit ? currentMeetingDebrief.LeadershipCoachUsername : User.Identity.Name);

            //Get the hub FK
            selectedHubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

            //Validate
            if (selectedHubFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Hub is required!";
            }
            else
            {
                //Check for duplication
                if (deDebriefYear.Date != DateTime.MinValue)
                {
                    //Get the selected year
                    selectedYear = deDebriefYear.Date.Year;

                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check to see if there are any schedules for this coach and year and hub
                        List<Models.HubLCMeetingDebrief> duplicateDebriefs = context.HubLCMeetingDebrief.AsNoTracking()
                                        .Where(d => d.HubFK == selectedHubFK.Value &&
                                                d.DebriefYear == selectedYear &&
                                                d.HubLCMeetingDebriefPK != currentDebriefPK &&
                                                d.LeadershipCoachUsername == leadershipCoachUsername).ToList();

                        //Check the count of duplicate schedules
                        if (duplicateDebriefs.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "A meeting debrief already exists for this combination of leadership coach, hub, and year!";
                        }

                        //Check for invalid session attendees
                        List<Models.HubLCMeetingDebriefSessionAttendee> invalidAttendees;
                        invalidAttendees = context.HubLCMeetingDebriefSessionAttendee.AsNoTracking()
                                                    .Include(a => a.CWLTMember)
                                                    .Include(a => a.HubLCMeetingDebriefSession)
                                                    .Where(a => a.HubLCMeetingDebriefSession.HubLCMeetingDebriefFK == currentDebriefPK &&
                                                                (a.CWLTMember.HubFK != selectedHubFK ||
                                                                a.CWLTMember.StartDate.Year > deDebriefYear.Date.Year ||
                                                                         (a.CWLTMember.LeaveDate.HasValue == true &&
                                                                            a.CWLTMember.LeaveDate.Value.Year < deDebriefYear.Date.Year)))
                                                    .ToList();

                        if (invalidAttendees.Count > 0)
                        {
                            //Set the validation
                            e.IsValid = false;
                            e.ErrorText = "At least one session debrief is invalid because at least one attendee was not active in the selected hub during the selected year.  See the notification message for details.";

                            //Format the invalid attendee and session information into a list of strings
                            List<string> errorTextList = invalidAttendees.Select(a => string.Format("Session Date: {0:MM/dd/yyyy}, Invalid Attendee: ({1}) {2} {3}",
                                                                                        a.HubLCMeetingDebriefSession.SessionStartDateTime,
                                                                                        a.CWLTMember.IDNumber,
                                                                                        a.CWLTMember.FirstName,
                                                                                        a.CWLTMember.LastName)).ToList();

                            //Create a single string that is separated by breaks
                            string errorTextString = string.Join("<br/>", errorTextList);

                            //Show the message
                            msgSys.ShowMessageToUser("danger", "Hub/Year Validation Error", string.Format("Invalid session debriefs:<br/><br/>{0}", errorTextString), 200000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deDebriefYear DevExpress
        /// BootstrapDateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deDebriefYear BootstrapDateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deDebriefYear_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int selectedYear;

            //Check to see if the debrief year was selected
            if (deDebriefYear.Date == DateTime.MinValue)
            {
                e.IsValid = false;
                e.ErrorText = "Year is required!";
            }
            else if (currentDebriefPK > 0)
            {
                //This only applies to edits
                //Get the selected year
                selectedYear = deDebriefYear.Date.Year;

                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if there are any debrief sessions for this debrief that are not in the year
                    int nonMatchingSessions = context.HubLCMeetingDebriefSession.AsNoTracking()
                                    .Where(s => s.HubLCMeetingDebriefFK == currentDebriefPK
                                             && s.SessionStartDateTime.Year != selectedYear).Count();

                    //Check the count of non matching sessions
                    if (nonMatchingSessions > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Year must be the same as the year part of the session debriefs' Session Date!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtDebriefPhone DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtDebriefPhone BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtDebriefPhone_Validation(object sender, ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtDebriefPhone.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtDebriefPhone.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }

        /// <summary>
        /// This method fires when the user selects a hub from the ddHub control.
        /// </summary>
        /// <param name="sender">The ddHub BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddHub_ValueChanged(object sender, EventArgs e)
        {
            //To hold the necessary values
            int? selectedHubFK;

            //Get the hub FK
            selectedHubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

            //Make sure the selected hub is valid before continuing
            if (selectedHubFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the hub info
                    Hub selectedHub = context.Hub.AsNoTracking().Include(p => p.Program).Where(h => h.HubPK == selectedHubFK).FirstOrDefault();

                    //Make sure the hub exists
                    if (selectedHub != null && selectedHub.Program != null)
                    {
                        //Set the hub-specific labels
                        lblHubProgramCount.Text = selectedHub.Program.Count.ToString();
                    }
                }
                
                //Only rebind the team members if this is an edit since the
                //team members aren't shown for new unsaved forms
                if (isEdit)
                {
                    //Bind the team members
                    BindTeamMembers();
                    BindAttendeeTagBox(currentDebriefPK, (tbDebriefSessionAttendees.Value == null ? null : tbDebriefSessionAttendees.Value.ToString()));
                }
            }

            //Set the focus
            ddHub.Focus();
        }

        /// <summary>
        /// This method fires when the user selects a year from the deDebriefYear control.
        /// </summary>
        /// <param name="sender">The deDebriefYear BootstrapDateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deDebriefYear_ValueChanged(object sender, EventArgs e)
        {
            //Only rebind the team members if this is an edit since the
            //team members aren't shown for new unsaved forms
            if (isEdit)
            {
                //Bind the team members
                BindTeamMembers();
                BindAttendeeTagBox(currentDebriefPK, (tbDebriefSessionAttendees.Value == null ? null : tbDebriefSessionAttendees.Value.ToString()));
            }

            //Set the focus
            deDebriefYear.Focus();
        }

        #endregion

        #region Team Members

        /// <summary>
        /// This method populates the team member repeater with up-to-date information
        /// </summary>
        private void BindTeamMembers()
        {
            //To hold the necessary values
            int selectedHubFK;

            //Make sure the year and hub are valid
            if (deDebriefYear.Date > DateTime.MinValue &&
                ddHub.Value != null &&
                int.TryParse(ddHub.Value.ToString(), out selectedHubFK))
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the members for the hub and year combination
                    List<Models.CWLTMember> activeCWLTMembers = context.CWLTMember.AsNoTracking()
                                                .Include(tm => tm.Hub)
                                                .Where(tm => tm.HubFK == selectedHubFK &&
                                                             tm.StartDate.Year <= deDebriefYear.Date.Year &&
                                                             (tm.LeaveDate.HasValue == false ||
                                                                tm.LeaveDate.Value.Year >= deDebriefYear.Date.Year))
                                                .ToList();

                    //Bind the repeater
                    repeatTeamMembers.DataSource = activeCWLTMembers;
                    repeatTeamMembers.DataBind();
                }
            }
            else
            {
                //Clear the repeater
                repeatTeamMembers.DataSource = new List<Models.CWLTMember>();
                repeatTeamMembers.DataBind();
            }
        }

        #endregion

        #region Debrief Sessions

        /// <summary>
        /// This method populates the debrief session repeater with up-to-date information
        /// </summary>
        private void BindDebriefSessions()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<HubLCMeetingDebriefSession> DebriefSessions = context.HubLCMeetingDebriefSession.AsNoTracking()
                                            .Include(s => s.HubLCMeetingDebriefSessionAttendee)
                                            .Include(s => s.HubLCMeetingDebriefSessionAttendee.Select(a => a.CWLTMember))
                                            .Where(t => t.HubLCMeetingDebriefFK == currentMeetingDebrief.HubLCMeetingDebriefPK)
                                            .ToList();
                repeatDebriefSessions.DataSource = DebriefSessions;
                repeatDebriefSessions.DataBind();
            }
        }

        /// <summary>
        /// This method binds the attendee tag box for the debrief sessions
        /// and fills the box with all the leadership team members.
        /// </summary>
        private void BindAttendeeTagBox(int debriefPK, string valueToSelect)
        {
            //To hold the necessary values
            int selectedHubFK;

            if (debriefPK > 0 &&
                deDebriefYear.Date > DateTime.MinValue &&
                ddHub.Value != null &&
                int.TryParse(ddHub.Value.ToString(), out selectedHubFK))
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the leadership team members
                    var members = context.CWLTMember.AsNoTracking()
                                                .Where(tm => tm.HubFK == selectedHubFK &&
                                                             tm.StartDate.Year <= deDebriefYear.Date.Year &&
                                                             (tm.LeaveDate.HasValue == false ||
                                                                tm.LeaveDate.Value.Year >= deDebriefYear.Date.Year))
                        .Select(tm => new
                        {
                            tm.CWLTMemberPK,
                            IDNumberAndName = "(" + tm.IDNumber + ") " + tm.FirstName + " " + tm.LastName
                        })
                        .OrderBy(tm => tm.IDNumberAndName)
                        .ToList();

                    //Bind the tag box
                    tbDebriefSessionAttendees.DataSource = members;
                    tbDebriefSessionAttendees.DataBind();
                }

                //Check to see how many team members there are
                if (tbDebriefSessionAttendees.Items.Count > 0)
                {
                    //There is at least 1 member, enable the tag box
                    tbDebriefSessionAttendees.ClientEnabled = true;

                    //Try to select the member(s) passed to this method
                    tbDebriefSessionAttendees.Value = valueToSelect;
                }
                else
                {
                    //There are no members in the list, disable the control
                    tbDebriefSessionAttendees.ClientEnabled = false;
                }
            }
            else
            {
                //Not all necessary information was available, clear and disable the control
                tbDebriefSessionAttendees.Value = "";
                tbDebriefSessionAttendees.ClientEnabled = false;
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetDebriefSessionControlUsability(bool enabled)
        {
            //Enable/disable the controls
            deDebriefSessionDate.ClientEnabled = enabled;
            teDebriefSessionStartTime.ClientEnabled = enabled;
            teDebriefSessionEndTime.ClientEnabled = enabled;
            deDebriefSessionNextDate.ClientEnabled = enabled;
            teDebriefSessionNextStartTime.ClientEnabled = enabled;
            teDebriefSessionNextEndTime.ClientEnabled = enabled;
            tbDebriefSessionAttendees.ClientEnabled = enabled;
            chkDebriefSessionReviewedActionPlan.ClientEnabled = enabled;
            chkDebriefSessionReviewedBOQ.ClientEnabled = enabled;
            chkDebriefSessionReviewedOther.ClientEnabled = enabled;
            txtDebriefSessionReviewedOtherSpecify.ClientEnabled = enabled;
            txtDebriefSessionSummary.ClientEnabled = enabled;
            txtDebriefSessionNextSteps.ClientEnabled = enabled;

            //Show/hide the submit button
            submitDebriefSession.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitDebriefSession.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitDebriefSession.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the session debriefs
        /// and it opens a div that allows the user to add a session debrief
        /// </summary>
        /// <param name="sender">The lbAddDebriefSession LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddDebriefSession_Click(object sender, EventArgs e)
        {
            //Bind the attendee tag box
            BindAttendeeTagBox(currentDebriefPK, null);

            //Clear inputs in the input div
            hfAddEditDebriefSessionPK.Value = "0";
            deDebriefSessionDate.Value = "";
            teDebriefSessionStartTime.Value = "";
            teDebriefSessionEndTime.Value = "";
            deDebriefSessionNextDate.Value = "";
            teDebriefSessionNextStartTime.Value = "";
            teDebriefSessionNextEndTime.Value = "";
            chkDebriefSessionReviewedActionPlan.Checked = false;
            chkDebriefSessionReviewedBOQ.Checked = false;
            chkDebriefSessionReviewedOther.Checked = false;
            txtDebriefSessionReviewedOtherSpecify.Text = "";
            txtDebriefSessionSummary.Text = "";
            txtDebriefSessionNextSteps.Text = "";

            //Set the title
            lblAddEditDebriefSession.Text = "Add Session Debrief";

            //Show the input div
            divAddEditDebriefSession.Visible = true;

            //Set focus to the first field
            deDebriefSessionDate.Focus();

            //Enable the controls
            SetDebriefSessionControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a session debrief
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewDebriefSession LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewDebriefSession_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblDebriefSessionPK = (Label)item.FindControl("lblDebriefSessionPK");

            //Get the PK from the label
            int? currentSessionPK = (string.IsNullOrWhiteSpace(lblDebriefSessionPK.Text) ? (int?)null : Convert.ToInt32(lblDebriefSessionPK.Text));

            if (currentSessionPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the session debrief to view
                    HubLCMeetingDebriefSession currentDebriefSession = context.HubLCMeetingDebriefSession
                                                                                .Include(s => s.HubLCMeetingDebriefSessionAttendee).AsNoTracking()
                                                                                .Where(s => s.HubLCMeetingDebriefSessionPK == currentSessionPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditDebriefSession.Text = "View Session Debrief";
                    hfAddEditDebriefSessionPK.Value = currentSessionPK.Value.ToString();
                    deDebriefSessionDate.Value = currentDebriefSession.SessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionStartTime.Value = currentDebriefSession.SessionStartDateTime;
                    teDebriefSessionEndTime.Value = currentDebriefSession.SessionEndDateTime;
                    deDebriefSessionNextDate.Value = currentDebriefSession.NextSessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionNextStartTime.Value = currentDebriefSession.NextSessionStartDateTime;
                    teDebriefSessionNextEndTime.Value = currentDebriefSession.NextSessionEndDateTime;
                    chkDebriefSessionReviewedActionPlan.Checked = currentDebriefSession.ReviewedActionPlan;
                    chkDebriefSessionReviewedBOQ.Checked = currentDebriefSession.ReviewedBOQ;
                    chkDebriefSessionReviewedOther.Checked = currentDebriefSession.ReviewedOtherItem;
                    txtDebriefSessionReviewedOtherSpecify.Text = currentDebriefSession.ReviewedOtherItemSpecify;
                    txtDebriefSessionSummary.Text = currentDebriefSession.SessionSummary;
                    txtDebriefSessionNextSteps.Text = currentDebriefSession.SessionNextSteps;

                    //Fill the attendee tag box
                    string attendeeList = string.Join(",", currentDebriefSession.HubLCMeetingDebriefSessionAttendee.Select(a => a.CWLTMemberFK).ToList());
                    BindAttendeeTagBox(currentDebriefPK, attendeeList);
                }

                //Show the edit div
                divAddEditDebriefSession.Visible = true;

                //Set focus to the first field
                deDebriefSessionDate.Focus();

                //Disable the controls since this is a view
                SetDebriefSessionControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected session debrief!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a session debrief
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditDebriefSession LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditDebriefSession_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblDebriefSessionPK = (Label)item.FindControl("lblDebriefSessionPK");

            //Get the PK from the label
            int? currentSessionPK = (string.IsNullOrWhiteSpace(lblDebriefSessionPK.Text) ? (int?)null : Convert.ToInt32(lblDebriefSessionPK.Text));

            if (currentSessionPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the session debrief to edit
                    HubLCMeetingDebriefSession editDebriefSession = context.HubLCMeetingDebriefSession
                                                                                .Include(s => s.HubLCMeetingDebriefSessionAttendee).AsNoTracking()
                                                                                .Where(s => s.HubLCMeetingDebriefSessionPK == currentSessionPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditDebriefSession.Text = "Edit Session Debrief";
                    hfAddEditDebriefSessionPK.Value = currentSessionPK.Value.ToString();
                    deDebriefSessionDate.Value = editDebriefSession.SessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionStartTime.Value = editDebriefSession.SessionStartDateTime;
                    teDebriefSessionEndTime.Value = editDebriefSession.SessionEndDateTime;
                    deDebriefSessionNextDate.Value = editDebriefSession.NextSessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionNextStartTime.Value = editDebriefSession.NextSessionStartDateTime;
                    teDebriefSessionNextEndTime.Value = editDebriefSession.NextSessionEndDateTime;
                    chkDebriefSessionReviewedActionPlan.Checked = editDebriefSession.ReviewedActionPlan;
                    chkDebriefSessionReviewedBOQ.Checked = editDebriefSession.ReviewedBOQ;
                    chkDebriefSessionReviewedOther.Checked = editDebriefSession.ReviewedOtherItem;
                    txtDebriefSessionReviewedOtherSpecify.Text = editDebriefSession.ReviewedOtherItemSpecify;
                    txtDebriefSessionSummary.Text = editDebriefSession.SessionSummary;
                    txtDebriefSessionNextSteps.Text = editDebriefSession.SessionNextSteps;

                    //Fill the attendee tag box
                    string attendeeList = string.Join(",", editDebriefSession.HubLCMeetingDebriefSessionAttendee.Select(a => a.CWLTMemberFK).ToList());
                    BindAttendeeTagBox(currentDebriefPK, attendeeList);
                }

                //Show the edit div
                divAddEditDebriefSession.Visible = true;

                //Set focus to the first field
                deDebriefSessionDate.Focus();

                //Enable the controls
                SetDebriefSessionControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected session debrief!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the session debrief
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitDebriefSession submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitDebriefSession_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditDebriefSessionPK.Value = "0";
            divAddEditDebriefSession.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitDebriefSession_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the session debrief
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitDebriefSession submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitDebriefSession_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the session debrief PK
                int currentDebriefSessionPK = Convert.ToInt32(hfAddEditDebriefSessionPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    HubLCMeetingDebriefSession currentDebriefSession;

                    //Calculate the start and end datetimes
                    DateTime startTime = teDebriefSessionStartTime.DateTime;
                    DateTime endTime = teDebriefSessionEndTime.DateTime;
                    DateTime nextStartTime = teDebriefSessionNextStartTime.DateTime;
                    DateTime nextEndTime = teDebriefSessionNextEndTime.DateTime;

                    DateTime sessionStartDateTime = deDebriefSessionDate.Date.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
                    DateTime sessionEndDateTime = deDebriefSessionDate.Date.AddHours(endTime.Hour).AddMinutes(endTime.Minute);
                    DateTime nextSessionStartDateTime = deDebriefSessionNextDate.Date.AddHours(nextStartTime.Hour).AddMinutes(nextStartTime.Minute);
                    DateTime nextSessionEndDateTime = deDebriefSessionNextDate.Date.AddHours(nextEndTime.Hour).AddMinutes(nextEndTime.Minute);

                    //Check to see if this is an add or an edit
                    if (currentDebriefSessionPK == 0)
                    {
                        //Add
                        currentDebriefSession = new HubLCMeetingDebriefSession();
                        currentDebriefSession.NextSessionEndDateTime = nextSessionEndDateTime;
                        currentDebriefSession.NextSessionStartDateTime = nextSessionStartDateTime;
                        currentDebriefSession.SessionStartDateTime = sessionStartDateTime;
                        currentDebriefSession.SessionEndDateTime = sessionEndDateTime;
                        currentDebriefSession.ReviewedActionPlan = chkDebriefSessionReviewedActionPlan.Checked;
                        currentDebriefSession.ReviewedBOQ = chkDebriefSessionReviewedBOQ.Checked;
                        currentDebriefSession.ReviewedOtherItem = chkDebriefSessionReviewedOther.Checked;
                        currentDebriefSession.ReviewedOtherItemSpecify = (string.IsNullOrWhiteSpace(txtDebriefSessionReviewedOtherSpecify.Text) ? null : txtDebriefSessionReviewedOtherSpecify.Text);
                        currentDebriefSession.SessionSummary = txtDebriefSessionSummary.Text;
                        currentDebriefSession.SessionNextSteps = txtDebriefSessionNextSteps.Text;
                        currentDebriefSession.HubLCMeetingDebriefFK = currentDebriefPK;
                        currentDebriefSession.CreateDate = DateTime.Now;
                        currentDebriefSession.Creator = User.Identity.Name;

                        //Save to the database
                        context.HubLCMeetingDebriefSession.Add(currentDebriefSession);
                        context.SaveChanges();

                        //Get the selected team members
                        List<int> selectedTeamMembers = tbDebriefSessionAttendees.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of attendees
                        foreach (int teamMemberPK in selectedTeamMembers)
                        {
                            HubLCMeetingDebriefSessionAttendee newAttendeeRow = new HubLCMeetingDebriefSessionAttendee();

                            newAttendeeRow.CreateDate = DateTime.Now;
                            newAttendeeRow.Creator = User.Identity.Name;
                            newAttendeeRow.HubLCMeetingDebriefSessionFK = currentDebriefSession.HubLCMeetingDebriefSessionPK;
                            newAttendeeRow.CWLTMemberFK = teamMemberPK;
                            context.HubLCMeetingDebriefSessionAttendee.Add(newAttendeeRow);
                        }

                        //Save the participants
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added session debrief!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentDebriefSession = context.HubLCMeetingDebriefSession.Include(s => s.HubLCMeetingDebriefSessionAttendee).Where(s => s.HubLCMeetingDebriefSessionPK == currentDebriefSessionPK).FirstOrDefault();
                        currentDebriefSession.NextSessionEndDateTime = nextSessionEndDateTime;
                        currentDebriefSession.NextSessionStartDateTime = nextSessionStartDateTime;
                        currentDebriefSession.SessionStartDateTime = sessionStartDateTime;
                        currentDebriefSession.SessionEndDateTime = sessionEndDateTime;
                        currentDebriefSession.ReviewedActionPlan = chkDebriefSessionReviewedActionPlan.Checked;
                        currentDebriefSession.ReviewedBOQ = chkDebriefSessionReviewedBOQ.Checked;
                        currentDebriefSession.ReviewedOtherItem = chkDebriefSessionReviewedOther.Checked;
                        currentDebriefSession.ReviewedOtherItemSpecify = (string.IsNullOrWhiteSpace(txtDebriefSessionReviewedOtherSpecify.Text) ? null : txtDebriefSessionReviewedOtherSpecify.Text);
                        currentDebriefSession.SessionSummary = txtDebriefSessionSummary.Text;
                        currentDebriefSession.SessionNextSteps = txtDebriefSessionNextSteps.Text;
                        currentDebriefSession.EditDate = DateTime.Now;
                        currentDebriefSession.Editor = User.Identity.Name;

                        //Get the selected attendees
                        List<int> selectedTeamMembers = tbDebriefSessionAttendees.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of participants
                        foreach (int teamMemberFK in selectedTeamMembers)
                        {
                            //Get the attendee object if it is already in the database
                            HubLCMeetingDebriefSessionAttendee existingAttendee = currentDebriefSession.HubLCMeetingDebriefSessionAttendee.Where(a => a.CWLTMemberFK == teamMemberFK).FirstOrDefault();

                            if (existingAttendee == null || existingAttendee.HubLCMeetingDebriefSessionAttendeePK == 0)
                            {
                                //Add missing participants
                                existingAttendee = new HubLCMeetingDebriefSessionAttendee();
                                existingAttendee.CreateDate = DateTime.Now;
                                existingAttendee.Creator = User.Identity.Name;
                                existingAttendee.HubLCMeetingDebriefSessionFK = currentDebriefSessionPK;
                                existingAttendee.CWLTMemberFK = teamMemberFK;
                                context.HubLCMeetingDebriefSessionAttendee.Add(existingAttendee);
                            }
                        }

                        //To hold the participant PKs that will be removed
                        List<int> deletedParticipantPKs = new List<int>();

                        //Get all the participants that should no longer be linked
                        foreach (HubLCMeetingDebriefSessionAttendee attendee in currentDebriefSession.HubLCMeetingDebriefSessionAttendee)
                        {
                            //If the team member is not selected, needs to be removed
                            bool keepParticipant = selectedTeamMembers.Exists(p => p == attendee.CWLTMemberFK);

                            if (keepParticipant == false)
                            {
                                deletedParticipantPKs.Add(attendee.HubLCMeetingDebriefSessionAttendeePK);
                            }
                        }

                        //Delete the particpant rows
                        context.HubLCMeetingDebriefSessionAttendee.Where(p => deletedParticipantPKs.Contains(p.HubLCMeetingDebriefSessionAttendeePK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.HubLCMeetingDebriefSessionAttendeeChanged.Where(ac => deletedParticipantPKs.Contains(ac.HubLCMeetingDebriefSessionAttendeePK)).Update(pc => new HubLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited session debrief!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditDebriefSessionPK.Value = "0";
                    divAddEditDebriefSession.Visible = false;

                    //Rebind the session debrief table
                    BindDebriefSessions();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a session debrief
        /// and it deletes the session debrief information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteDebriefSession LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteDebriefSession_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit session debrief information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteDebriefSessionPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteDebriefSessionPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the items to remove
                            Models.HubLCMeetingDebriefSession sessionToRemove = context.HubLCMeetingDebriefSession.Where(s => s.HubLCMeetingDebriefSessionPK == rowToRemovePK).FirstOrDefault();
                            List<int> attendeePKsToRemove = context.HubLCMeetingDebriefSessionAttendee.Where(a => a.HubLCMeetingDebriefSessionFK == rowToRemovePK).Select(a => a.HubLCMeetingDebriefSessionAttendeePK).ToList();

                            //Remove the linked items (in order)
                            context.HubLCMeetingDebriefSessionAttendee.Where(a => attendeePKsToRemove.Contains(a.HubLCMeetingDebriefSessionAttendeePK)).Delete();

                            //Remove the session
                            context.HubLCMeetingDebriefSession.Remove(sessionToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //Update the change rows and set the deleter
                            context.HubLCMeetingDebriefSessionAttendeeChanged.Where(c => attendeePKsToRemove.Contains(c.HubLCMeetingDebriefSessionAttendeeChangedPK)).Update(c => new HubLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });

                            //Get the delete change row and set the deleter
                            context.HubLCMeetingDebriefSessionChanged.Where(c => c.HubLCMeetingDebriefSessionPK == sessionToRemove.HubLCMeetingDebriefSessionPK).Update(c => new HubLCMeetingDebriefSessionChanged() { Deleter = User.Identity.Name });

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the session debrief table
                            BindDebriefSessions();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the session debrief!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the session debrief, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the session debrief!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the session debrief!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the session debrief to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        protected void deDebriefSessionDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the session date and debrief year
            DateTime? sessionDate = (deDebriefSessionDate.Value == null ? (DateTime?)null : deDebriefSessionDate.Date);
            int? debriefYear = (deDebriefYear.Value == null ? (int?)null : deDebriefYear.Date.Year);

            //Validate
            if (debriefYear.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The debrief year in the basic information section must be entered before the Session Date!";
            }
            else if (sessionDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Session Date is required!";
            }
            else if (sessionDate.Value.Year != debriefYear.Value)
            {
                e.IsValid = false;
                e.ErrorText = "The Session Date's year must be the same as the debrief year in the basic information section!";
            }
            else if (sessionDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Session Date cannot be in the future!";
            }
        }

        protected void teDebriefSessionEndTime_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end time
            DateTime? startTime = (teDebriefSessionStartTime.Value == null ? (DateTime?)null : teDebriefSessionStartTime.DateTime);
            DateTime? endTime = (teDebriefSessionEndTime.Value == null ? (DateTime?)null : teDebriefSessionEndTime.DateTime);


            //Perform validation
            if (endTime.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Session End Time is required!";
            }
            else if (startTime >= endTime)
            {
                e.IsValid = false;
                e.ErrorText = "Session End Time must be after the Session Start Time!";
            }
        }

        protected void deDebriefSessionNextDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the session date and next session date
            DateTime? sessionDate = (deDebriefSessionDate.Value == null ? (DateTime?)null : deDebriefSessionDate.Date);
            DateTime? nextSessionDate = (deDebriefSessionNextDate.Value == null ? (DateTime?)null : deDebriefSessionNextDate.Date);

            //Validate
            if (nextSessionDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Next Session Date is required!";
            }
            else if (sessionDate.HasValue && nextSessionDate.Value <= sessionDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Next Session Date must be after the Session Date!";
            }
        }

        protected void teDebriefSessionNextEndTime_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end time
            DateTime? startTime = (teDebriefSessionNextStartTime.Value == null ? (DateTime?)null : teDebriefSessionNextStartTime.DateTime);
            DateTime? endTime = (teDebriefSessionNextEndTime.Value == null ? (DateTime?)null : teDebriefSessionNextEndTime.DateTime);


            //Perform validation
            if (endTime.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Next Session End Time is required!";
            }
            else if (startTime >= endTime)
            {
                e.IsValid = false;
                e.ErrorText = "Next Session End Time must be after the Next Session Start Time!";
            }
        }

        protected void txtDebriefSessionReviewedOtherSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Validate
            if (chkDebriefSessionReviewedOther.Checked && string.IsNullOrWhiteSpace(txtDebriefSessionReviewedOtherSpecify.Text))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Other Focus Item is required!";
            }
        }

        #endregion

    }
}