using DevExpress.Web;
using PhoneNumbers;
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
    public partial class CoachingCircleLCMeetingDebrief : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "LCCD";
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
        private Models.CoachingCircleLCMeetingDebrief currentMeetingDebrief;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CCMeetingDebriefPK"]))
            {
                //Parse the member pk
                int.TryParse(Request.QueryString["CCMeetingDebriefPK"], out currentDebriefPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentDebriefPK == 0 && !string.IsNullOrWhiteSpace(hfCoachingCircleLCMeetingDebriefPK.Value))
            {
                int.TryParse(hfCoachingCircleLCMeetingDebriefPK.Value, out currentDebriefPK);
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
                currentMeetingDebrief = context.CoachingCircleLCMeetingDebrief
                                        .Include(d => d.State)
                                        .AsNoTracking()
                                        .Where(d => d.CoachingCircleLCMeetingDebriefPK == currentDebriefPK)
                                        .FirstOrDefault();

                //Check to see if the debrief exists
                if (currentMeetingDebrief == null)
                {
                    //The debrief doesn't exist, set the debrief to a new debrief object
                    currentMeetingDebrief = new Models.CoachingCircleLCMeetingDebrief();
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

                //Bind the dropdowns
                BindDropDowns();

                //Bind the tables
                BindTeamMembers();
                BindDebriefSessions();

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
                    lblPageTitle.Text = "Add New Coaching Circle/Community of Practice Meeting Debrief";
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
                    lblPageTitle.Text = "Edit Coaching Circle/Community of Practice Meeting Debrief";
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
                    lblPageTitle.Text = "View Coaching Circle/Community of Practice Meeting Debrief";
                }

                //Set focus to the first field
                txtDebriefCoachingCircle.Focus();

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
                txtDebriefCoachingCircle.Value = currentMeetingDebrief.CoachingCircleName;
                deDebriefYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentMeetingDebrief.DebriefYear));
                txtDebriefTargetAudience.Value = currentMeetingDebrief.TargetAudience;

                //Fill the leadership coach label
                PyramidUser currentLeadershipCoach = PyramidUser.GetUserRecordByUsername(currentMeetingDebrief.LeadershipCoachUsername);
                lblLeadershipCoach.Text = string.Format("{0} {1} ({2})", currentLeadershipCoach.FirstName, currentLeadershipCoach.LastName, currentLeadershipCoach.UserName);

                //Fill the state label
                lblDebriefState.Text = currentMeetingDebrief.State.Name;
            }
            else
            {
                if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH || 
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH)
                {
                    //Fill the leadership coach label
                    PyramidUser currentLeadershipCoach = PyramidUser.GetUserRecordByUsername(User.Identity.Name);
                    lblLeadershipCoach.Text = string.Format("{0} {1} ({2})", currentLeadershipCoach.FirstName, currentLeadershipCoach.LastName, currentLeadershipCoach.UserName);

                    //Fill the state label
                    lblDebriefState.Text = currentProgramRole.StateName;
                }
                else
                {
                    //User is not a coach, show a message after a redirect back to the dashboard
                    msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized because you are not logged in as a Leadership Coach or Hub Leadership Coach!", 10000);
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
            txtDebriefCoachingCircle.ClientEnabled = enabled;
            deDebriefYear.ClientEnabled = enabled;
            txtDebriefTargetAudience.ClientEnabled = enabled;

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
            List<CodeTeamPosition> allTeamPositions = new List<CodeTeamPosition>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allTeamPositions = context.CodeTeamPosition.AsNoTracking().OrderBy(t => t.OrderBy).ToList();
            }

            //Bind the team position dropdown
            ddTeamMemberPosition.DataSource = allTeamPositions;
            ddTeamMemberPosition.DataBind();
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
                    Response.Redirect(string.Format("/Pages/CoachingCircleLCMeetingDebrief.aspx?CCMeetingDebriefPK={0}&Action=Edit&messageType={1}",
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
                    Reports.PreBuiltReports.FormReports.RptCoachingCircleLCMeetingDebrief report = new Reports.PreBuiltReports.FormReports.RptCoachingCircleLCMeetingDebrief();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Coaching Circle/Community of Practice Meeting Debrief", currentDebriefPK);
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
                    Response.Redirect(string.Format("/Pages/CoachingCircleLCMeetingDebrief.aspx?CCMeetingDebriefPK={0}&Action={1}&messageType={2}&Print=True",
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
                currentMeetingDebrief.CoachingCircleName = (string.IsNullOrWhiteSpace(txtDebriefCoachingCircle.Text) ? null : txtDebriefCoachingCircle.Text);
                currentMeetingDebrief.TargetAudience = (string.IsNullOrWhiteSpace(txtDebriefTargetAudience.Text) ? null : txtDebriefTargetAudience.Text);
                currentMeetingDebrief.DebriefYear = deDebriefYear.Date.Year;

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
                        Models.CoachingCircleLCMeetingDebrief existingMeetingDebrief = context.CoachingCircleLCMeetingDebrief.Find(currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK);

                        //Set the member object to the new values
                        context.Entry(existingMeetingDebrief).CurrentValues.SetValues(currentMeetingDebrief);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCoachingCircleLCMeetingDebriefPK.Value = currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK.ToString();
                        currentDebriefPK = currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK;
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
                        currentMeetingDebrief.StateFK = currentProgramRole.CurrentStateFK.Value;

                        //Add it to the context
                        context.CoachingCircleLCMeetingDebrief.Add(currentMeetingDebrief);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCoachingCircleLCMeetingDebriefPK.Value = currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK.ToString();
                        currentDebriefPK = currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK;
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

        #endregion

        #region Group Members

        /// <summary>
        /// This method populates the team member repeater with up-to-date information
        /// </summary>
        private void BindTeamMembers()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<CoachingCircleLCMeetingDebriefTeamMember> TeamMembers = context.CoachingCircleLCMeetingDebriefTeamMember.AsNoTracking()
                                            .Include(t => t.CodeTeamPosition)
                                            .Where(t => t.CoachingCircleLCMeetingDebriefFK == currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK)
                                            .ToList();
                repeatTeamMembers.DataSource = TeamMembers;
                repeatTeamMembers.DataBind();
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetTeamMemberControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddTeamMemberPosition.ClientEnabled = enabled;
            txtTeamMemberFirstName.ClientEnabled = enabled;
            txtTeamMemberLastName.ClientEnabled = enabled;
            txtTeamMemberEmail.ClientEnabled = enabled;
            txtTeamMemberPhone.ClientEnabled = enabled;

            //Show/hide the submit button
            submitTeamMember.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitTeamMember.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitTeamMember.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the team members
        /// and it opens a div that allows the user to add a team member
        /// </summary>
        /// <param name="sender">The lbAddTeamMember LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTeamMember_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditTeamMemberPK.Value = "0";
            ddTeamMemberPosition.Value = "";
            txtTeamMemberFirstName.Text = "";
            txtTeamMemberLastName.Text = "";
            txtTeamMemberEmail.Text = "";
            txtTeamMemberPhone.Text = "";

            //Set the title
            lblAddEditTeamMember.Text = "Add Group Member";

            //Show the input div
            divAddEditTeamMember.Visible = true;

            //Set focus to the position field
            ddTeamMemberPosition.Focus();

            //Enable the controls
            SetTeamMemberControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a team member
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewTeamMember LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewTeamMember_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblTeamMemberPK = (Label)item.FindControl("lblTeamMemberPK");

            //Get the PK from the label
            int? teamMemberPK = (string.IsNullOrWhiteSpace(lblTeamMemberPK.Text) ? (int?)null : Convert.ToInt32(lblTeamMemberPK.Text));

            if (teamMemberPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the team member to view
                    CoachingCircleLCMeetingDebriefTeamMember currentTeamMember = context.CoachingCircleLCMeetingDebriefTeamMember.AsNoTracking().Where(m => m.CoachingCircleLCMeetingDebriefTeamMemberPK == teamMemberPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditTeamMember.Text = "View Group Member";
                    ddTeamMemberPosition.SelectedItem = ddTeamMemberPosition.Items.FindByValue(currentTeamMember.TeamPositionCodeFK);
                    txtTeamMemberFirstName.Text = currentTeamMember.FirstName;
                    txtTeamMemberLastName.Text = currentTeamMember.LastName;
                    txtTeamMemberEmail.Text = currentTeamMember.EmailAddress;
                    txtTeamMemberPhone.Text = currentTeamMember.PhoneNumber;
                    hfAddEditTeamMemberPK.Value = teamMemberPK.Value.ToString();
                }

                //Show the edit div
                divAddEditTeamMember.Visible = true;

                //Set focus to the first field
                ddTeamMemberPosition.Focus();

                //Disable the controls since this is a view
                SetTeamMemberControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected group member!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a team member
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditTeamMember LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTeamMember_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblTeamMemberPK = (Label)item.FindControl("lblTeamMemberPK");

            //Get the PK from the label
            int? teamMemberPK = (string.IsNullOrWhiteSpace(lblTeamMemberPK.Text) ? (int?)null : Convert.ToInt32(lblTeamMemberPK.Text));

            if (teamMemberPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the team member to edit
                    CoachingCircleLCMeetingDebriefTeamMember editTeamMember = context.CoachingCircleLCMeetingDebriefTeamMember.AsNoTracking().Where(m => m.CoachingCircleLCMeetingDebriefTeamMemberPK == teamMemberPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditTeamMember.Text = "Edit Group Member";
                    ddTeamMemberPosition.SelectedItem = ddTeamMemberPosition.Items.FindByValue(editTeamMember.TeamPositionCodeFK);
                    txtTeamMemberFirstName.Text = editTeamMember.FirstName;
                    txtTeamMemberLastName.Text = editTeamMember.LastName;
                    txtTeamMemberEmail.Text = editTeamMember.EmailAddress;
                    txtTeamMemberPhone.Text = editTeamMember.PhoneNumber;
                    hfAddEditTeamMemberPK.Value = teamMemberPK.Value.ToString();
                }

                //Show the edit div
                divAddEditTeamMember.Visible = true;

                //Set focus to the first field
                ddTeamMemberPosition.Focus();

                //Enable the controls
                SetTeamMemberControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected group member!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the team member
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitTeamMember submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTeamMember_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditTeamMemberPK.Value = "0";
            divAddEditTeamMember.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTeamMember_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the team member
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitTeamMember submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTeamMember_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the team member PK
                int teamMemberPK = Convert.ToInt32(hfAddEditTeamMemberPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    CoachingCircleLCMeetingDebriefTeamMember currentTeamMember;

                    //Check to see if this is an add or an edit
                    if (teamMemberPK == 0)
                    {
                        //Add
                        currentTeamMember = new CoachingCircleLCMeetingDebriefTeamMember();
                        currentTeamMember.TeamPositionCodeFK = Convert.ToInt32(ddTeamMemberPosition.Value);
                        currentTeamMember.FirstName = txtTeamMemberFirstName.Text;
                        currentTeamMember.LastName = txtTeamMemberLastName.Text;
                        currentTeamMember.EmailAddress = (string.IsNullOrWhiteSpace(txtTeamMemberEmail.Text) ? null : txtTeamMemberEmail.Text);
                        currentTeamMember.PhoneNumber = (string.IsNullOrWhiteSpace(txtTeamMemberPhone.Text) ? null : txtTeamMemberPhone.Text);
                        currentTeamMember.CoachingCircleLCMeetingDebriefFK = currentDebriefPK;
                        currentTeamMember.CreateDate = DateTime.Now;
                        currentTeamMember.Creator = User.Identity.Name;

                        //Save to the database
                        context.CoachingCircleLCMeetingDebriefTeamMember.Add(currentTeamMember);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added group member!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentTeamMember = context.CoachingCircleLCMeetingDebriefTeamMember.Find(teamMemberPK);
                        currentTeamMember.TeamPositionCodeFK = Convert.ToInt32(ddTeamMemberPosition.Value);
                        currentTeamMember.FirstName = txtTeamMemberFirstName.Text;
                        currentTeamMember.LastName = txtTeamMemberLastName.Text;
                        currentTeamMember.EmailAddress = (string.IsNullOrWhiteSpace(txtTeamMemberEmail.Text) ? null : txtTeamMemberEmail.Text);
                        currentTeamMember.PhoneNumber = (string.IsNullOrWhiteSpace(txtTeamMemberPhone.Text) ? null : txtTeamMemberPhone.Text);
                        currentTeamMember.EditDate = DateTime.Now;
                        currentTeamMember.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited group member!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditTeamMemberPK.Value = "0";
                    divAddEditTeamMember.Visible = false;

                    //Rebind the team member table
                    BindTeamMembers();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a team member
        /// and it deletes the team member information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteTeamMember LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTeamMember_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit team member information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteTeamMemberPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTeamMemberPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the team member to remove
                            CoachingCircleLCMeetingDebriefTeamMember assignmentToRemove = context.CoachingCircleLCMeetingDebriefTeamMember.Where(t => t.CoachingCircleLCMeetingDebriefTeamMemberPK == rowToRemovePK).FirstOrDefault();

                            //Remove the team member
                            context.CoachingCircleLCMeetingDebriefTeamMember.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CoachingCircleLCMeetingDebriefTeamMemberChanged
                                    .OrderByDescending(c => c.CoachingCircleLCMeetingDebriefTeamMemberChangedPK)
                                    .Where(c => c.CoachingCircleLCMeetingDebriefTeamMemberPK == assignmentToRemove.CoachingCircleLCMeetingDebriefTeamMemberPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the team member table
                            BindTeamMembers();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the group member!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the group member, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the group member!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the group member!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the group member to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
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
                    int nonMatchingSessions = context.CoachingCircleLCMeetingDebriefSession.AsNoTracking()
                                    .Where(s => s.CoachingCircleLCMeetingDebriefFK == currentDebriefPK
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
        /// This method fires when the validation for the txtTeamMemberPhone DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTeamMemberPhone BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTeamMemberPhone_Validation(object sender, ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtTeamMemberPhone.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtTeamMemberPhone.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
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
                List<CoachingCircleLCMeetingDebriefSession> DebriefSessions = context.CoachingCircleLCMeetingDebriefSession.AsNoTracking()
                                            .Include(s => s.CoachingCircleLCMeetingDebriefSessionAttendee)
                                            .Include(s => s.CoachingCircleLCMeetingDebriefSessionAttendee.Select(a => a.CoachingCircleLCMeetingDebriefTeamMember))
                                            .Where(t => t.CoachingCircleLCMeetingDebriefFK == currentMeetingDebrief.CoachingCircleLCMeetingDebriefPK)
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
            if (debriefPK > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the leadership team members
                    var members = context.CoachingCircleLCMeetingDebriefTeamMember.AsNoTracking()
                                    .Where(tm => tm.CoachingCircleLCMeetingDebriefFK == debriefPK)
                        .Select(tm => new
                        {
                            tm.CoachingCircleLCMeetingDebriefTeamMemberPK,
                            FullName = tm.FirstName + " " + tm.LastName
                        })
                        .OrderBy(tm => tm.FullName)
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
                //No debrief PK was passed, clear and disable the control
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
            tbDebriefSessionAttendees.ClientEnabled = enabled;
            txtDebriefSessionSummary.ClientEnabled = enabled;

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
            txtDebriefSessionSummary.Text = "";

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
                    CoachingCircleLCMeetingDebriefSession currentDebriefSession = context.CoachingCircleLCMeetingDebriefSession
                                                                                .Include(s => s.CoachingCircleLCMeetingDebriefSessionAttendee).AsNoTracking()
                                                                                .Where(s => s.CoachingCircleLCMeetingDebriefSessionPK == currentSessionPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditDebriefSession.Text = "View Session Debrief";
                    hfAddEditDebriefSessionPK.Value = currentSessionPK.Value.ToString();
                    deDebriefSessionDate.Value = currentDebriefSession.SessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionStartTime.Value = currentDebriefSession.SessionStartDateTime;
                    teDebriefSessionEndTime.Value = currentDebriefSession.SessionEndDateTime;
                    txtDebriefSessionSummary.Text = currentDebriefSession.SessionSummary;

                    //Fill the attendee tag box
                    string attendeeList = string.Join(",", currentDebriefSession.CoachingCircleLCMeetingDebriefSessionAttendee.Select(a => a.CoachingCircleLCMeetingDebriefTeamMemberFK).ToList());
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
                    CoachingCircleLCMeetingDebriefSession editDebriefSession = context.CoachingCircleLCMeetingDebriefSession
                                                                                .Include(s => s.CoachingCircleLCMeetingDebriefSessionAttendee).AsNoTracking()
                                                                                .Where(s => s.CoachingCircleLCMeetingDebriefSessionPK == currentSessionPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditDebriefSession.Text = "Edit Session Debrief";
                    hfAddEditDebriefSessionPK.Value = currentSessionPK.Value.ToString();
                    deDebriefSessionDate.Value = editDebriefSession.SessionStartDateTime.Date; //Make sure to only use the date part of the DateTime
                    teDebriefSessionStartTime.Value = editDebriefSession.SessionStartDateTime;
                    teDebriefSessionEndTime.Value = editDebriefSession.SessionEndDateTime;
                    txtDebriefSessionSummary.Text = editDebriefSession.SessionSummary;

                    //Fill the attendee tag box
                    string attendeeList = string.Join(",", editDebriefSession.CoachingCircleLCMeetingDebriefSessionAttendee.Select(a => a.CoachingCircleLCMeetingDebriefTeamMemberFK).ToList());
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
                    CoachingCircleLCMeetingDebriefSession currentDebriefSession;

                    //Calculate the start and end datetimes
                    DateTime startTime = teDebriefSessionStartTime.DateTime;
                    DateTime endTime = teDebriefSessionEndTime.DateTime;

                    DateTime sessionStartDateTime = deDebriefSessionDate.Date.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
                    DateTime sessionEndDateTime = deDebriefSessionDate.Date.AddHours(endTime.Hour).AddMinutes(endTime.Minute);

                    //Check to see if this is an add or an edit
                    if (currentDebriefSessionPK == 0)
                    {
                        //Add
                        currentDebriefSession = new CoachingCircleLCMeetingDebriefSession();
                        currentDebriefSession.SessionStartDateTime = sessionStartDateTime;
                        currentDebriefSession.SessionEndDateTime = sessionEndDateTime;
                        currentDebriefSession.SessionSummary = txtDebriefSessionSummary.Text;
                        currentDebriefSession.CoachingCircleLCMeetingDebriefFK = currentDebriefPK;
                        currentDebriefSession.CreateDate = DateTime.Now;
                        currentDebriefSession.Creator = User.Identity.Name;

                        //Save to the database
                        context.CoachingCircleLCMeetingDebriefSession.Add(currentDebriefSession);
                        context.SaveChanges();

                        //Get the selected team members
                        List<int> selectedTeamMembers = tbDebriefSessionAttendees.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of attendees
                        foreach (int teamMemberPK in selectedTeamMembers)
                        {
                            CoachingCircleLCMeetingDebriefSessionAttendee newAttendeeRow = new CoachingCircleLCMeetingDebriefSessionAttendee();

                            newAttendeeRow.CreateDate = DateTime.Now;
                            newAttendeeRow.Creator = User.Identity.Name;
                            newAttendeeRow.CoachingCircleLCMeetingDebriefSessionFK = currentDebriefSession.CoachingCircleLCMeetingDebriefSessionPK;
                            newAttendeeRow.CoachingCircleLCMeetingDebriefTeamMemberFK = teamMemberPK;
                            context.CoachingCircleLCMeetingDebriefSessionAttendee.Add(newAttendeeRow);
                        }

                        //Save the participants
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added session debrief!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentDebriefSession = context.CoachingCircleLCMeetingDebriefSession.Include(s => s.CoachingCircleLCMeetingDebriefSessionAttendee).Where(s => s.CoachingCircleLCMeetingDebriefSessionPK == currentDebriefSessionPK).FirstOrDefault();
                        currentDebriefSession.SessionStartDateTime = sessionStartDateTime;
                        currentDebriefSession.SessionEndDateTime = sessionEndDateTime;
                        currentDebriefSession.SessionSummary = txtDebriefSessionSummary.Text;
                        currentDebriefSession.EditDate = DateTime.Now;
                        currentDebriefSession.Editor = User.Identity.Name;

                        //Get the selected attendees
                        List<int> selectedTeamMembers = tbDebriefSessionAttendees.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of participants
                        foreach (int teamMemberFK in selectedTeamMembers)
                        {
                            //Get the attendee object if it is already in the database
                            CoachingCircleLCMeetingDebriefSessionAttendee existingAttendee = currentDebriefSession.CoachingCircleLCMeetingDebriefSessionAttendee.Where(a => a.CoachingCircleLCMeetingDebriefTeamMemberFK == teamMemberFK).FirstOrDefault();

                            if (existingAttendee == null || existingAttendee.CoachingCircleLCMeetingDebriefSessionAttendeePK == 0)
                            {
                                //Add missing participants
                                existingAttendee = new CoachingCircleLCMeetingDebriefSessionAttendee();
                                existingAttendee.CreateDate = DateTime.Now;
                                existingAttendee.Creator = User.Identity.Name;
                                existingAttendee.CoachingCircleLCMeetingDebriefSessionFK = currentDebriefSessionPK;
                                existingAttendee.CoachingCircleLCMeetingDebriefTeamMemberFK = teamMemberFK;
                                context.CoachingCircleLCMeetingDebriefSessionAttendee.Add(existingAttendee);
                            }
                        }

                        //To hold the participant PKs that will be removed
                        List<int> deletedParticipantPKs = new List<int>();

                        //Get all the participants that should no longer be linked
                        foreach (CoachingCircleLCMeetingDebriefSessionAttendee attendee in currentDebriefSession.CoachingCircleLCMeetingDebriefSessionAttendee)
                        {
                            //If the team member is not selected, needs to be removed
                            bool keepParticipant = selectedTeamMembers.Exists(p => p == attendee.CoachingCircleLCMeetingDebriefTeamMemberFK);

                            if (keepParticipant == false)
                            {
                                deletedParticipantPKs.Add(attendee.CoachingCircleLCMeetingDebriefSessionAttendeePK);
                            }
                        }

                        //Delete the particpant rows
                        context.CoachingCircleLCMeetingDebriefSessionAttendee.Where(p => deletedParticipantPKs.Contains(p.CoachingCircleLCMeetingDebriefSessionAttendeePK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.CoachingCircleLCMeetingDebriefSessionAttendeeChanged.Where(ac => deletedParticipantPKs.Contains(ac.CoachingCircleLCMeetingDebriefSessionAttendeePK)).Update(pc => new CoachingCircleLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });

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
                            Models.CoachingCircleLCMeetingDebriefSession sessionToRemove = context.CoachingCircleLCMeetingDebriefSession.Where(s => s.CoachingCircleLCMeetingDebriefSessionPK == rowToRemovePK).FirstOrDefault();
                            List<int> attendeePKsToRemove = context.CoachingCircleLCMeetingDebriefSessionAttendee.Where(a => a.CoachingCircleLCMeetingDebriefSessionFK == rowToRemovePK).Select(a => a.CoachingCircleLCMeetingDebriefSessionAttendeePK).ToList();

                            //Remove the linked items (in order)
                            context.CoachingCircleLCMeetingDebriefSessionAttendee.Where(a => attendeePKsToRemove.Contains(a.CoachingCircleLCMeetingDebriefSessionAttendeePK)).Delete();

                            //Remove the session
                            context.CoachingCircleLCMeetingDebriefSession.Remove(sessionToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //Update the change rows and set the deleter
                            context.CoachingCircleLCMeetingDebriefSessionAttendeeChanged.Where(c => attendeePKsToRemove.Contains(c.CoachingCircleLCMeetingDebriefSessionAttendeeChangedPK)).Update(c => new CoachingCircleLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });

                            //Get the delete change row and set the deleter
                            context.CoachingCircleLCMeetingDebriefSessionChanged.Where(c => c.CoachingCircleLCMeetingDebriefSessionPK == sessionToRemove.CoachingCircleLCMeetingDebriefSessionPK).Update(c => new CoachingCircleLCMeetingDebriefSessionChanged() { Deleter = User.Identity.Name });

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

        #endregion

    }
}